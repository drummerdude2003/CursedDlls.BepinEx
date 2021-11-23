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

[assembly: AssemblyVersion("1.4")]
namespace Cursed.RemoveAttachmentChecks
{
    [BepInPlugin("dll.cursed.removeattachmentchecks", "CursedDlls - Remove Attachment Checks", "1.4")]
    public class RemoveAttachmentChecksPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> _pluginEnabled;

        private static ConfigEntry<bool> _allAttachmentsAreScalable;
        private static ConfigEntry<bool> _easyAttachmentAttaching;
        private static ConfigEntry<bool> _removeAttachmentsAtAnyTime;
        private static ConfigEntry<bool> _enableBiDirectionalAttachments;
        private static ConfigEntry<bool> _typeChecksDisabled;

        private void Awake()
        {
            _pluginEnabled = Config.Bind("General", "PluginEnabled", false,
                "Enables RemoveAttachmentChecks. RemoveAttachmentChecks, as it says on the tin, removes a lot of checks related to attachments.");

            _allAttachmentsAreScalable = Config.Bind("General", "AllAttachmentsAreScalable", false,
                "Allows all attachments to be scalable, regardless of if it should be able to scale to its mount.");
            _removeAttachmentsAtAnyTime = Config.Bind("General", "RemoveAttachmentsAtAnyTime", false,
                "Allows the removal of attachments even when other attachments are on that attachment. Warning: becomes very janky when it comes to muzzle devices!");
            _easyAttachmentAttaching = Config.Bind("General", "EasyAttachmentAttaching", false,
                "Similar to easy magazine loading, but for attachments! You have the range between the muzzle point and hand radially, so there should be ample space to make stupid stuff.");
            _enableBiDirectionalAttachments = Config.Bind("General", "EnableBiDirectionalAttachments", false,
                "Enables attachments to be placed in any direction on rails. (For example, backwards muzzle devices)");
            _typeChecksDisabled = Config.Bind("General", "TypeChecksDisabled", false,
                "Disables type checking on rounds. This lets you insert any round you want into any gun, magazine, clip, speedloader, or collection of palmed rounds.");

            if (_pluginEnabled.Value)
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

        [HarmonyPatch(typeof(Revolver), "Awake")]
        [HarmonyPrefix]
        public static bool AddSuppressorAttachableToRevolver(Revolver __instance)
        {
            //tbh this really shouldn't even be done, i don't think any of the revolvers actually actually have supressed rounds. However, I don't care!
            __instance.AllowsSuppressor = true;
            return true;
        }

        
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
            if (_easyAttachmentAttaching.Value && __instance.m_hand.OtherHand.CurrentInteractable != null && __instance.m_hand.OtherHand.CurrentInteractable is FVRFireArm)
            {
                FVRFireArm fvrfireArm = __instance.m_hand.OtherHand.CurrentInteractable as FVRFireArm;
                float handToMuzzle = Vector3.Distance(__instance.m_hand.OtherHand.transform.position, fvrfireArm.CurrentMuzzle.position) + 0.25f;
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
        public static bool FVRFireArmAttachmentInterface_HasAttachmentsOnIt(FVRFireArmAttachmentInterface __instance, ref bool __result)
        {
            __result = false;
            return !_removeAttachmentsAtAnyTime.Value;
        }

        [HarmonyPatch(typeof(FVRFireArmAttachmentMount), nameof(FVRFireArmAttachmentMount.CanThisRescale))]
        [HarmonyPrefix]
        public static bool FVRFireArmAttachmentMount_CanThisRescale(FVRFireArmAttachmentMount __instance, ref bool __result)
        {
            __result = true;
            return !_allAttachmentsAreScalable.Value;
        }

        [HarmonyPatch(typeof(FVRFireArmAttachment), "AttachToMount")]
        [HarmonyPatch(typeof(FVRFireArmAttachment), "GetRotTarget")]
        [HarmonyPrefix]
        public static void FVRFireArmAttachment_SetBiDirectional(ref bool ___IsBiDirectional)
        {
            if (!___IsBiDirectional)
                ___IsBiDirectional = _enableBiDirectionalAttachments.Value;
        }

        [HarmonyPatch(typeof(FVRFireArmAttachmentMount), "Awake")]
        [HarmonyPostfix]
        public static void FVRFireArmAttachmentMount_RemoveMaxAttachmentLimit(ref int ___m_maxAttachments)
        {
            ___m_maxAttachments = int.MaxValue;
        }
    }
}