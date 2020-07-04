using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Cursed.TimeScale
{
    [BepInPlugin("dll.cursed.timescalefix", "Time scaler", "1.0")]
    public class TimeScaleFixPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(TimeScaleFixPlugin));
        }

        [HarmonyPatch(typeof(AudioSource), "pitch", MethodType.Setter)]
        [HarmonyPrefix]
        public static void FixPitch(ref float value)
        {
            value *= Time.timeScale;
        }
    }
}