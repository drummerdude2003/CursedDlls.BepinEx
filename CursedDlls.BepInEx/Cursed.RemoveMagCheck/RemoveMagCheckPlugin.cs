using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FistVR;
using HarmonyLib;
using RUST.Steamworks;
using Steamworks;
using UnityEngine;

[assembly: AssemblyVersion("1.4")]
namespace RemoveMagCheck
{
    [BepInPlugin("dll.cursed.removemagcheck", "CursedDlls - Remove Magazine (and clip) Checks", "1.4")]
    public class RemoveMagCheckPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> _pluginEnabled;

        private static ConfigEntry<bool> _typeChecksDisabled;

        private void Awake()
        {
            _pluginEnabled = Config.Bind("General", "PluginEnabled", false,
                "Enables RemoveMagCheck. RemoveMagCheck, as it says on the tin, removes checks related to magazines, but also includes clips.");

            _typeChecksDisabled = Config.Bind("General", "TypeChecksDisabled", true,
                "Disables type checking on magazines and clips. This lets you insert any mag or clip of any type into any gun.");

            if (_pluginEnabled.Value)
                Harmony.CreateAndPatchAll(typeof(RemoveMagCheckPlugin));
        }

        public static bool TypeCheck(bool condition)
        {
            return condition || _typeChecksDisabled.Value;
        }

        /*
         * Type patches
         * Patch instructions that are simiilar to Type == Type to be TypeCheck(Type == Type)
         */
        [HarmonyPatch(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.FVRFixedUpdate))]
        [HarmonyPatch(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.UpdateInteraction))]
        [HarmonyPatch(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.Release))]
        [HarmonyPatch(typeof(FVRFireArmReloadTriggerMag), nameof(FVRFireArmReloadTriggerMag.OnTriggerEnter))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PatchMagazineTypeChecksTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs).MatchForward(false,
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "MagazineType"),
                new CodeMatch(i => i.opcode == OpCodes.Bne_Un || i.opcode == OpCodes.Bne_Un_S))
            .Repeat(m =>
            {
                m.Advance(1)
                .SetOpcodeAndAdvance(OpCodes.Brfalse)
                .Advance(-1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ceq, null))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemoveMagCheckPlugin), "TypeCheck")));
            })
            .InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FVRFireArmClipTriggerClip), nameof(FVRFireArmClipTriggerClip.OnTriggerEnter))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PatchClipTypeCheckTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs).MatchForward(false,
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "ClipType"),
                new CodeMatch(i => i.opcode == OpCodes.Bne_Un || i.opcode == OpCodes.Bne_Un_S))
            .Repeat(m =>
            {
                m.Advance(1)
                .SetOpcodeAndAdvance(OpCodes.Brfalse)
                .Advance(-1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ceq, null))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemoveMagCheckPlugin), "TypeCheck")));
            })
            .InstructionEnumeration();
        }
    }
}