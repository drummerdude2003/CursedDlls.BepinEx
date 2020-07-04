using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using UnityEngine;
using Valve.VR;

namespace Cursed.TimeScale
{
    [BepInPlugin("dll.cursed.timescale", "Time scaler", "1.0")]
    public class TimeScaleFixPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<float> _timeScaleIncrement;

        private void Awake()
        {
            _timeScaleIncrement = Config.Bind("General", "TimeScaleIncrement", 0.125f,
                "How much time scale is increased/decreased at a time");
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

        [HarmonyPatch(typeof(FVRWristMenu), nameof(FVRWristMenu.Update))]
        [HarmonyPostfix]
        public static void UpdateTimeScaleText(FVRWristMenu __instance, bool ___m_isActive)
        {
            if (___m_isActive) __instance.Clock.text = Time.timeScale.ToString(CultureInfo.InvariantCulture);
        }

        private static void DiffTimeScale(FVRWristMenu self, int dir)
        {
            self.Aud.PlayOneShot(self.AudClip_Engage, 1f);
            self.Aud.pitch = 1f;
            Time.timeScale += _timeScaleIncrement.Value * dir;
            Time.fixedDeltaTime = Time.timeScale / SteamVR.instance.hmd_DisplayFrequency;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        }
    }
}