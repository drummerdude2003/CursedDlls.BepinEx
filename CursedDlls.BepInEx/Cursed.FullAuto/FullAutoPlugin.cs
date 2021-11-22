using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using FistVR;
using HarmonyLib;
using RUST.Steamworks;
using Steamworks;

[assembly: AssemblyVersion("1.4")]
namespace Cursed.FullAuto
{
    [BepInPlugin("dll.cursed.fullauto", "CursedDlls - Full Auto", "1.4")]
    public class FullAutoPlugin : BaseUnityPlugin
    {
        private static readonly FastInvokeHandler UpdateSafetyPos =
            MethodInvoker.GetHandler(AccessTools.Method(typeof(Handgun), "UpdateSafetyPos"));

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(FullAutoPlugin));
        }

        [HarmonyPatch(typeof(ClosedBoltWeapon), "Awake")]
        [HarmonyPostfix]
        public static void PostAwake(ClosedBoltWeapon __instance)
        {
            if (__instance.FireSelector_Modes.Length == 0) return;
            if (__instance.FireSelector_Modes.Any(t => t.ModeType == ClosedBoltWeapon.FireSelectorModeType.FullAuto))
                return;

            var modes = new List<ClosedBoltWeapon.FireSelectorMode>(__instance.FireSelector_Modes);
            var full_auto = new ClosedBoltWeapon.FireSelectorMode
            {
                ModeType = ClosedBoltWeapon.FireSelectorModeType.FullAuto,
                SelectorPosition = __instance.FireSelector_Modes[__instance.FireSelector_Modes.Length - 1]
                    .SelectorPosition
            };
            modes.Add(full_auto);
            __instance.FireSelector_Modes = modes.ToArray();
        }


        [HarmonyPatch(typeof(Handgun), "Awake")]
        [HarmonyPostfix]
        public static void PostAwake(Handgun __instance)
        {
            if (__instance.FireSelectorModes.Length == 0) return;
            var hasFullAuto =
                __instance.FireSelectorModes.Any(t => t.ModeType == Handgun.FireSelectorModeType.FullAuto);

            if (hasFullAuto) return;
            var fullAuto = new Handgun.FireSelectorMode
            {
                SelectorPosition = __instance.FireSelectorModes[__instance.FireSelectorModes.Length - 1]
                    .SelectorPosition,
                ModeType = Handgun.FireSelectorModeType.FullAuto
            };
            __instance.FireSelectorModes = new List<Handgun.FireSelectorMode>(__instance.FireSelectorModes)
            {
                fullAuto
            }.ToArray();
        }

        [HarmonyPatch(typeof(Handgun), "ToggleSafety")]
        [HarmonyPrefix]
        public static bool ToggleSafetyPrefix(Handgun __instance, ref int ___m_fireSelectorMode,
            ref bool ___m_isSafetyEngaged, ref bool ___m_isHammerCocked, ref bool __result)
        {
            if (!__instance.HasSafety)
            {
                __result = false;
                return false;
            }

            if (__instance.DoesSafetyRequireSlideForward && __instance.Slide.CurPos != HandgunSlide.SlidePos.Forward)
            {
                __result = false;
                return false;
            }

            if (__instance.Slide.CurPos == HandgunSlide.SlidePos.Forward ||
                __instance.Slide.CurPos >= HandgunSlide.SlidePos.Locked)
            {
                if (___m_isSafetyEngaged)
                {
                    __instance.PlayAudioEvent(FirearmAudioEventType.Safety);
                    ___m_isSafetyEngaged = false;
                    if (__instance.DoesSafetyDisengageCockHammer) __instance.CockHammer(true);
                    ___m_fireSelectorMode = 0;
                }
                else if (__instance.FireSelectorModes[___m_fireSelectorMode].ModeType !=
                         Handgun.FireSelectorModeType.FullAuto)
                {
                    __instance.PlayAudioEvent(FirearmAudioEventType.Safety, 0.7f);
                    ___m_fireSelectorMode = (___m_fireSelectorMode + 1) % __instance.FireSelectorModes.Length;
                }
                else
                {
                    ___m_fireSelectorMode = 0;
                    var flag = !(__instance.DoesSafetyRequireCockedHammer && !___m_isHammerCocked);
                    if (flag)
                    {
                        ___m_isSafetyEngaged = true;
                        if (__instance.DoesSafetyEngagingDecock) __instance.DeCockHammer(true, true);
                        __instance.PlayAudioEvent(FirearmAudioEventType.Safety);
                    }
                }

                UpdateSafetyPos(__instance);
                __result = true;
                return false;
            }

            __result = false;
            return false;
        }
    }
}