using BepInEx;
using FistVR;
using HarmonyLib;

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

        [HarmonyPatch(typeof(FVRFireArmAttachmentMount), "Awake")]
        [HarmonyPostfix]
        public static void RemoveMaxAttachmentLimit(ref int ___m_maxAttachments)
        {
            ___m_maxAttachments = int.MaxValue;
        }
    }
}