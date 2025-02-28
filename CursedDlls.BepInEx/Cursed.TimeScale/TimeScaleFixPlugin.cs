﻿using System;
using System.Globalization;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using RUST.Steamworks;
using Steamworks;
using UnityEngine;
using Valve.VR;

namespace Cursed.TimeScale
{
    [BepInPlugin("dll.cursed.timescale", "CursedDlls - Time Scaler", "1.7")]
    public class TimeScaleFixPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> _pluginEnabled;

        private static ConfigEntry<float> _timeScaleIncrement;
        private static ConfigEntry<string> _wristMenuDateTimeFormat;

        private void Awake()
        {
            _pluginEnabled = Config.Bind("General", "PluginEnabled", false,
                "Enables TimeScaleFix. TimeScaleFix adds a few fixes for timescale editing, such as pitching game sounds and adding a timescale to the wrist menu.");

            _timeScaleIncrement = Config.Bind("General", "TimeScaleIncrement", 0.125f,
                "How much time scale is increased/decreased at a time");
            _wristMenuDateTimeFormat = Config.Bind("General", "WristMenuDateTimeFormat", "hh:mm:ss tt",
                "What the format of the wrist menu's clock is. Search for \"Custom date and time format strings\" to see the elligible characters you can use.");

            if (_pluginEnabled.Value)
            {
                Harmony harmony = Harmony.CreateAndPatchAll(typeof(TimeScaleFixPlugin));

                // stealing a bit of code from myself again
                // if the wrist menu has an additional postfix it's someone else's patch, so just remove ours
                MethodInfo FVRWristMenuUpdate = AccessTools.Method(typeof(FVRWristMenu), nameof(FVRWristMenu.Update));
                if (FVRWristMenuUpdate != null)
                {
                    Patches wristUpdatePatches = Harmony.GetPatchInfo(FVRWristMenuUpdate);
                    if (wristUpdatePatches.Postfixes.Count > 1)
                        harmony.Unpatch(FVRWristMenuUpdate, HarmonyPatchType.All, harmony.Id);
                }
            }
        }

        [HarmonyPatch(typeof(AudioSource), "pitch", MethodType.Setter)]
        [HarmonyPrefix]
        public static void FixPitch(ref float value)
        {
            value *= Time.timeScale;
        }

        [HarmonyPatch(typeof(FVRWristMenu), nameof(FVRWristMenu.TurnClockWise))]
        [HarmonyPatch(typeof(FVRWristMenuSection_MoveMode), nameof(FVRWristMenuSection_MoveMode.BTN_TurnClockwise))]
        [HarmonyPrefix]
        public static bool IncreaseTimeScale()
        {
            DiffTimeScale(+1);
            return false;
        }

        [HarmonyPatch(typeof(FVRWristMenu), nameof(FVRWristMenu.TurnCounterClockWise))]
        [HarmonyPatch(typeof(FVRWristMenuSection_MoveMode), nameof(FVRWristMenuSection_MoveMode.BTM_TurnCounterClockwise))]
        [HarmonyPrefix]
        public static bool DecreaseTimeScale()
        {
            DiffTimeScale(-1);
            return false;
        }

        [HarmonyPatch(typeof(FVRWristMenu), nameof(FVRWristMenu.Awake))]
        [HarmonyPostfix]
        public static void OverflowClockText(FVRWristMenu __instance)
        {
            __instance.SetSelectedButton(0);
            __instance.Clock.verticalOverflow = VerticalWrapMode.Overflow;
            __instance.Clock.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        [HarmonyPatch(typeof(FVRWristMenu), nameof(FVRWristMenu.Update))]
        [HarmonyPostfix]
        public static void UpdateTimeScaleText(FVRWristMenu __instance, bool ___m_isActive)
        {
            if (___m_isActive)
            {
                __instance.Clock.text = $"Time Scale: {Time.timeScale.ToString(CultureInfo.InvariantCulture)}";
                if (!String.IsNullOrEmpty(_wristMenuDateTimeFormat.Value))
                {
                    try { __instance.Clock.text += $"\n{DateTime.Now.ToString(_wristMenuDateTimeFormat.Value)}"; }
                    catch { } //yes I know this is bad but if users want custom things in their wrist menu, let them
                }
            }
        }

        private static void DiffTimeScale(int dir)
        {
            Time.timeScale += _timeScaleIncrement.Value * dir;
            Time.fixedDeltaTime = Time.timeScale / SteamVR.instance.hmd_DisplayFrequency;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
            SM.PlayGlobalUISound(SM.GlobalUISound.Beep, GM.CurrentPlayerBody.Head.position);
        }
    }
}