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

[assembly: AssemblyVersion("1.3")]
namespace Cursed.BetterBipods
{
    [BepInPlugin("dll.cursed.betterbipods", "CursedDlls - Better Bipods", "1.3")]
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

        /*
		 * Skiddie prevention
		 */
        [HarmonyPatch(typeof(HighScoreManager), nameof(HighScoreManager.UpdateScore), new Type[] { typeof(string), typeof(int), typeof(Action<int, int>) })]
        [HarmonyPatch(typeof(HighScoreManager), nameof(HighScoreManager.UpdateScore), new Type[] { typeof(SteamLeaderboard_t), typeof(int) })]
        [HarmonyPrefix]
        public static bool HSM_UpdateScore()
        {
            return false;
        }
    }
}
