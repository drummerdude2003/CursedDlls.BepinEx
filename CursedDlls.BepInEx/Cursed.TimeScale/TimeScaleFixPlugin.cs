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

[assembly: AssemblyVersion("1.4")]
namespace Cursed.TimeScale
{
    [BepInPlugin("dll.cursed.timescale", "CursedDlls - Time Scaler", "1.4")]
    public class TimeScaleFixPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<float> _timeScaleIncrement;
        private static ConfigEntry<string> _wristMenuDateTimeFormat;

        private void Awake()
        {
            _timeScaleIncrement = Config.Bind("General", "TimeScaleIncrement", 0.125f,
                "How much time scale is increased/decreased at a time");
            _wristMenuDateTimeFormat = Config.Bind("General", "WristMenuDateTimeFormat", "hh:mm:ss tt",
                "What the format of the wrist menu's clock is. Search for \"Custom date and time format strings\" to see the elligible characters you can use.");

            Harmony.CreateAndPatchAll(typeof(TimeScaleFixPlugin));
        }

        [HarmonyPatch(typeof(AudioSource), "pitch", MethodType.Setter)]
        [HarmonyPrefix]
        public static void FixPitch(ref float value)
        {
            value *= Time.timeScale;
        }

        [HarmonyPatch(typeof(FVRWristMenu), nameof(FVRWristMenu.TurnClockWise))]
        [HarmonyPrefix]
        public static bool IncreaseTimeScale(FVRWristMenu __instance)
        {
            DiffTimeScale(__instance, +1);
            return false;
        }

        [HarmonyPatch(typeof(FVRWristMenu), nameof(FVRWristMenu.TurnCounterClockWise))]
        [HarmonyPrefix]
        public static bool DecreaseTimeScale(FVRWristMenu __instance)
        {
            DiffTimeScale(__instance, -1);
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

        private static void DiffTimeScale(FVRWristMenu __instance, int dir)
        {
            Time.timeScale += _timeScaleIncrement.Value * dir;
            Time.fixedDeltaTime = Time.timeScale / SteamVR.instance.hmd_DisplayFrequency;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
            __instance.Aud.pitch = 1f;
            __instance.Aud.PlayOneShot(__instance.AudClip_Engage, 1f);
        }
    }
}