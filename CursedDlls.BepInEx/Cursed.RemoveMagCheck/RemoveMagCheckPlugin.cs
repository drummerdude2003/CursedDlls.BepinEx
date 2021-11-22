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
        public static ManualLogSource Logger { get; set; }

        private static ConfigEntry<bool> _typeChecksDisabled;

        private void Awake()
        {
            _typeChecksDisabled = Config.Bind("General", "TypeChecksDisabled", true,
                "Disables type checking on magazines and clips. This lets you insert any mag or clip of any type into any gun.");

            Logger = base.Logger;

            if (File.Exists($@"{Paths.BepInExRootPath}\monomod\CursedDlls\Assembly-CSharp.Cursed.RemoveRoundTypeCheck.mm.dll"))
                Harmony.CreateAndPatchAll(typeof(RemoveMagCheckPlugin));
            else
                Logger.LogError(@"This plugin requires the Assembly-CSharp.Cursed.RemoveRoundType.mm.dll MonoMod patch to function properly! Download and install it from https://github.com/drummerdude2003/CursedDlls.BepinEx/.");
        }

        public static bool TypeCheck(bool condition)
        {
            return condition || _typeChecksDisabled.Value;
        }

        /*
		 * Type patches
		 * Patch instructions that are simiilar to Type == Type to be TypeCheck(Type == Type)
		 */
        [HarmonyPatch(typeof(FVRFireArmMagazine), "FVRFixedUpdate")]
		[HarmonyPatch(typeof(FVRFireArmReloadTriggerMag), "OnTriggerEnter")]
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

		[HarmonyPatch(typeof(FVRFireArmClipTriggerClip), "OnTriggerEnter")]
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