using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using FistVR;
using HarmonyLib;
using UnityEngine;

namespace Cursed.RemoveAttachmentChecks
{
    [BepInPlugin("dll.cursed.removeattachmentchecks", "Remove attachment checks", "1.0")]
    public class RemoveAttachmentChecksPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(RemoveAttachmentChecksPlugin));
        }

        [HarmonyPatch(typeof(FVRFireArmAttachment), "AttachToMount")]
        [HarmonyPatch(typeof(FVRFireArmAttachment), "GetRotTarget")]
        [HarmonyPrefix]
        public static void SetBiDirectional(ref bool ___IsBiDirectional)
        {
            ___IsBiDirectional = true;
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

        [HarmonyPatch(typeof(FVRFireArmAttachmentMount), "Awake")]
        [HarmonyPostfix]
        public static void RemoveMaxAttachmentLimit(ref int ___m_maxAttachments)
        {
            ___m_maxAttachments = int.MaxValue;
        }

        [HarmonyPatch(typeof(FVRFireArmAttachmentSensor), "OnTriggerEnter")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RemoveAttachmentCheck(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(FVRFireArmAttachment), "Type")),
                    new CodeMatch(OpCodes.Bne_Un))
                .SetAndAdvance(OpCodes.Pop, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FVRFireArmAttachment), nameof(FVRFireArmAttachment.AttachToMount))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> RemoveAttachmentCollision(IEnumerable<CodeInstruction> instrs)
        {
            return instrs.Manipulator(i => i.Is(OpCodes.Ldstr, "Default"), i => i.operand = "NoCol");
        }
    }
}