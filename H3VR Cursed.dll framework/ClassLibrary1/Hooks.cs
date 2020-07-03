using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using BepInEx;
using BepInEx.Harmony;
using FistVR;
using System.Reflection.Emit;
using Valve.VR.InteractionSystem;

public static class Hooks
{
    public static void InstallHooks()
    {
        Harmony.CreateAndPatchAll(typeof(Hooks));
    }

    [HarmonyPatch(typeof(ClosedBoltWeapon), "Awake")]
    [HarmonyPostfix]
    public static void PostAwake(ClosedBoltWeapon __instance)
    {
        if (__instance.FireSelector_Modes.Length == 0)
        {
            return;
        }
        for (int i = 0; i < __instance.FireSelector_Modes.Length; i++)
        {
            if (__instance.FireSelector_Modes[i].ModeType == ClosedBoltWeapon.FireSelectorModeType.FullAuto)
            {
                return;
            }
        }
        List<ClosedBoltWeapon.FireSelectorMode> modes = new List<ClosedBoltWeapon.FireSelectorMode>(__instance.FireSelector_Modes);
        ClosedBoltWeapon.FireSelectorMode full_auto = new ClosedBoltWeapon.FireSelectorMode
        {
            ModeType = ClosedBoltWeapon.FireSelectorModeType.FullAuto,
            SelectorPosition = __instance.FireSelector_Modes[__instance.FireSelector_Modes.Length - 1].SelectorPosition
        };
        modes.Add(full_auto);
        __instance.FireSelector_Modes = modes.ToArray();
    }



    [HarmonyPatch(typeof(Handgun), "Awake")]
    [HarmonyPostfix]
    public static void PostAwake(Handgun __instance)
    {
        bool has_full_auto = false;
        if (__instance.FireSelectorModes.Length != 0)
        {
            for (int i = 0; i < __instance.FireSelectorModes.Length; i++)
            {
                if (__instance.FireSelectorModes[i].ModeType == Handgun.FireSelectorModeType.FullAuto)
                {
                    has_full_auto = true;
                    break;
                }
            }
            if (!has_full_auto)
            {
                Handgun.FireSelectorMode full_auto = new Handgun.FireSelectorMode
                {
                    SelectorPosition = __instance.FireSelectorModes[__instance.FireSelectorModes.Length - 1].SelectorPosition,
                    ModeType = Handgun.FireSelectorModeType.FullAuto
                };
                __instance.FireSelectorModes = new List<Handgun.FireSelectorMode>(__instance.FireSelectorModes)
        {
            full_auto
        }.ToArray();
            }
        }
    }

    private static FastInvokeHandler updateSafetyPos = MethodInvoker.GetHandler(AccessTools.Method(typeof(Handgun), "UpdateSafetyPos"));

    [HarmonyPatch(typeof(Handgun), "ToggleSafety")]
    [HarmonyPrefix]
    public static bool ToggleSafetyPrefix(Handgun __instance, ref int ___m_fireSelectorMode, ref bool ___m_isSafetyEngaged, ref bool ___m_isHammerCocked, ref bool __result)
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
        if (__instance.Slide.CurPos == HandgunSlide.SlidePos.Forward || __instance.Slide.CurPos >= HandgunSlide.SlidePos.Locked)
        {
            if (___m_isSafetyEngaged)
            {
                __instance.PlayAudioEvent(FirearmAudioEventType.Safety, 1f);
                ___m_isSafetyEngaged = false;
                if (__instance.DoesSafetyDisengageCockHammer)
                {
                    __instance.CockHammer(true);
                }
                ___m_fireSelectorMode = 0;
            }
            else if (__instance.FireSelectorModes[___m_fireSelectorMode].ModeType != Handgun.FireSelectorModeType.FullAuto)
            {
                __instance.PlayAudioEvent(FirearmAudioEventType.Safety, 0.7f);
                ___m_fireSelectorMode = (___m_fireSelectorMode + 1) % __instance.FireSelectorModes.Length;
            }
            else
            {
                ___m_fireSelectorMode = 0;
                bool flag = true;
                if (__instance.DoesSafetyRequireCockedHammer && !___m_isHammerCocked)
                {
                    flag = false;
                }
                if (flag)
                {
                    ___m_isSafetyEngaged = true;
                    if (__instance.DoesSafetyEngagingDecock)
                    {
                        __instance.DeCockHammer(true, true);
                    }
                    __instance.PlayAudioEvent(FirearmAudioEventType.Safety, 1f);
                }
            }
            updateSafetyPos(__instance);
            __result = true;
            return false;
        }
        __result = false;
        return false;
    }
}