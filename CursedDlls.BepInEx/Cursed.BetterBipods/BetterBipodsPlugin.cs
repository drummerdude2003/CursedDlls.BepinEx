using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using FistVR;
using HarmonyLib;
using RUST.Steamworks;
using Steamworks;
using UnityEngine;

[assembly: AssemblyVersion("1.4")]
namespace Cursed.BetterBipods
{
    [BepInPlugin("dll.cursed.betterbipods", "CursedDlls - Better Bipods", "1.4")]
    public class BetterBipodsPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(BetterBipodsPlugin));
        }

        [HarmonyPatch(typeof(FVRFireArmBipod), "UpdateBipod")]
        [HarmonyPrefix]
        public static void RemoveBipodRecoil(ref float ___RecoilDamping)
        {
            ___RecoilDamping = 0f;
        }

        [HarmonyPatch(typeof(FVRFireArmBipod), "UpdateBipod")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RemoveBipodUpRecoil(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Vector3), nameof(Vector3.Distance))),
                    new CodeMatch(OpCodes.Stloc_S))
                .Insert(
                    new CodeInstruction(OpCodes.Pop),
                    new CodeInstruction(OpCodes.Ldc_R4, 0f))
                .InstructionEnumeration();
        }
    }
}
