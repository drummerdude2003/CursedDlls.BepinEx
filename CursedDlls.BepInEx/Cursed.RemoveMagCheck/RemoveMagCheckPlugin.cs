using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using FistVR;
using HarmonyLib;

namespace RemoveMagCheck
{
    [BepInPlugin("dll.cursed.removemagcheck", "Remove magazine check", "1.0")]
    public class RemoveMagCheckPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(RemoveMagCheckPlugin));
        }

        [HarmonyPatch(typeof(FVRFireArmMagazine), "FVRFixedUpdate")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> FVRFixedUpdateTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Ldfld,
                        AccessTools.Field(typeof(FVRFireArm), nameof(FVRFireArm.MagazineType))),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld,
                        AccessTools.Field(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.MagazineType))))
                .SetAndAdvance(OpCodes.Ldc_I4_0, null)
                .SetAndAdvance(OpCodes.Ldc_I4_0, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null).InstructionEnumeration();
        }

        [HarmonyPatch(typeof(FVRFireArmReloadTriggerMag), "OnTriggerEnter")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> OnTriggerEnterTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_2),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld,
                        AccessTools.Field(typeof(FVRFireArmReloadTriggerMag),
                            nameof(FVRFireArmReloadTriggerMag.Magazine))),
                    new CodeMatch(OpCodes.Ldfld,
                        AccessTools.Field(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.MagazineType))))
                .SetAndAdvance(OpCodes.Ldc_I4_0, null)
                .SetAndAdvance(OpCodes.Ldc_I4_0, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null).InstructionEnumeration();
        }
    }
}