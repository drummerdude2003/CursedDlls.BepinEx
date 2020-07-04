using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using FistVR;
using HarmonyLib;

namespace Cursed.RemoveRoundTypeCheck
{
    [BepInPlugin("dll.cursed.removeroundtypecheck", "Remove RoundType checks", "1.0")]
    public class RemoveRoundTypeCheckPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(RemoveRoundTypeCheckPlugin));
        }

        [HarmonyPatch(typeof(FVRFireArmClip), nameof(FVRFireArmClip.UpdateInteraction))]
        [HarmonyPatch(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.UpdateInteraction))]
        [HarmonyPatch(typeof(FVRFireArmRound), nameof(FVRFireArmRound.UpdateInteraction))]
        [HarmonyPatch(typeof(FVRFireArmRound), "FVRFixedUpdate")]
        [HarmonyPatch(typeof(FVRFireArmRound), nameof(FVRFireArmRound.OnTriggerEnter))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateInteractionTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs).MatchForward(false,
                    new CodeMatch(i => i.IsLdloc() || i.IsLdarg()),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo) i.operand).Name == "RoundType"),
                    new CodeMatch(OpCodes.Bne_Un))
                .Repeat(m =>
                {
                    m.SetAndAdvance(OpCodes.Pop, null)
                        .SetAndAdvance(OpCodes.Nop, null)
                        .SetAndAdvance(OpCodes.Nop, null);
                })
                .InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FirearmSaver), nameof(FirearmSaver.TryToScanGun))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TryToScanGunTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs).MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo) i.operand).Name == "RoundType"),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo) i.operand).Name == "RoundType"),
                    new CodeMatch(OpCodes.Beq))
                .Repeat(m =>
                {
                    m.SetAndAdvance(OpCodes.Pop, null)
                        .SetAndAdvance(OpCodes.Nop, null)
                        .SetAndAdvance(OpCodes.Nop, null)
                        .SetOpcodeAndAdvance(OpCodes.Br);
                })
                .InstructionEnumeration();
        }

        [HarmonyPatch(typeof(Speedloader), nameof(Speedloader.UpdateInteraction))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SpeedLoaderUpdateInteractionTranspiler(
            IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs).MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo) i.operand).Name == "RoundType"),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Bne_Un))
                .Repeat(m =>
                {
                    m.SetAndAdvance(OpCodes.Pop, null)
                        .SetAndAdvance(OpCodes.Nop, null)
                        .SetAndAdvance(OpCodes.Nop, null)
                        .SetAndAdvance(OpCodes.Nop, null);
                })
                .InstructionEnumeration();
        }
    }
}