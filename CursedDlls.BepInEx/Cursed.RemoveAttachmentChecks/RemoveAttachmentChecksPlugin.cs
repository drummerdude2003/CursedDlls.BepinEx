using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using RUST.Steamworks;
using Steamworks;
using UnityEngine;

[assembly: AssemblyVersion("1.1")]
namespace Cursed.RemoveAttachmentChecks
{
    [BepInPlugin("dll.cursed.removeattachmentchecks", "CursedDlls - Remove Attachment Checks", "1.1")]
    public class RemoveAttachmentChecksPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> _removeAttachmentsAtAnyTime;
        private static ConfigEntry<bool> _typeChecksDisabled;

        private void Awake()
        {
            _removeAttachmentsAtAnyTime = Config.Bind("General", "RemoveAttachmentsAtAnyTime", false,
                "Allows the removal of attachments even when other attachments are on that attachment. Warning: becomes very janky when it comes to muzzle devices!");
            _typeChecksDisabled = Config.Bind("General", "TypeChecksDisabled", true,
                "Disables type checking on rounds. This lets you insert any round you want into any gun, magazine, clip, speedloader, or collection of palmed rounds.");

            Harmony.CreateAndPatchAll(typeof(RemoveAttachmentChecksPlugin));
        }

        public static bool TypeCheck(bool condition)
        {
            return condition || _typeChecksDisabled.Value;
        }

        /*
         * Type patches
         * Patch instructions that are simiilar to Type == Type to be TypeCheck(Type == Type)
         */
        [HarmonyPatch(typeof(FVRFireArmAttachmentSensor), "OnTriggerEnter")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PatchAttachmentTypeCheckTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs).MatchForward(false,
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "Type"),
                new CodeMatch(i => i.opcode == OpCodes.Bne_Un || i.opcode == OpCodes.Bne_Un_S))
            .Repeat(m =>
            {
                m.Advance(1)
                .SetOpcodeAndAdvance(OpCodes.Brfalse)
                .Advance(-1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ceq, null))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemoveAttachmentChecksPlugin), "TypeCheck")));
            })
            .InstructionEnumeration();
        }

        /*
         * Functionality patches
         * Adds functionality that technically isn't needed, but makes things easier to use and/or abuse
         */
        [HarmonyPatch(typeof(FVRFireArmAttachment), "UpdateSnappingBasedOnDistance")]
        [HarmonyPatch(typeof(Suppressor), "UpdateSnappingBasedOnDistance")]
        [HarmonyPrefix]
        public static bool FVRFireArmAttachment_UpdateSnappingBasedOnDistance(FVRFireArmAttachment __instance)
        {
            if (GM.Options.ControlOptions.UseEasyMagLoading && __instance.m_hand.OtherHand.CurrentInteractable != null && __instance.m_hand.OtherHand.CurrentInteractable is FVRFireArm)
            {
                FVRFireArm fvrfireArm = __instance.m_hand.OtherHand.CurrentInteractable as FVRFireArm;
                float handToMuzzle = Vector3.Distance(__instance.m_hand.OtherHand.transform.position, fvrfireArm.CurrentMuzzle.position) + 0.15f;
                float distance = Vector3.Distance(__instance.transform.position, fvrfireArm.transform.position);
                __instance.SetAllCollidersToLayer(false, distance <= handToMuzzle ? "NoCol" : "Default");
            }
            return true;
        }

        [HarmonyPatch(typeof(FVRFireArmAttachment), nameof(FVRFireArmAttachment.EndInteraction))]
        [HarmonyPrefix]
        public static bool FVRFireArmAttachment_EndInteraction(FVRFireArmAttachment __instance)
        {
            __instance.SetAllCollidersToLayer(false, "Default");
            return true;
        }

        [HarmonyPatch(typeof(FVRFireArmAttachmentInterface), nameof(FVRFireArmAttachmentInterface.HasAttachmentsOnIt))]
        [HarmonyPrefix]
        public static bool FVRFireArmAttachmentInterface_HasAttachmentsOnIt(FVRFireArmAttachment __instance, ref bool __result)
        {
            __result = false;
            return !_removeAttachmentsAtAnyTime.Value;
        }

        [HarmonyPatch(typeof(FVRFireArmAttachment), "AttachToMount")]
        [HarmonyPatch(typeof(FVRFireArmAttachment), "GetRotTarget")]
        [HarmonyPrefix]
        public static void FVRFireArmAttachment_SetBiDirectional(ref bool ___IsBiDirectional)
        {
            ___IsBiDirectional = true;
        }

        [HarmonyPatch(typeof(FVRFireArmAttachmentMount), "Awake")]
        [HarmonyPostfix]
        public static void FVRFireArmAttachmentMount_RemoveMaxAttachmentLimit(ref int ___m_maxAttachments)
        {
            ___m_maxAttachments = int.MaxValue;
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