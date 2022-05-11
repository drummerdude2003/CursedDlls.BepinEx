using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using HarmonyLib;
using UnityEngine;

[assembly: AssemblyVersion("1.5.1")]
namespace Cursed.UnlockAll
{
    [BepInPlugin("dll.cursed.unlockall", "CursedDlls - Unlock All Items", "1.5.1")]
    public class UnlockAllPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> _pluginEnabled;

        private static ConfigEntry<bool> _overwriteRewardsTxt;

        private void Awake()
        {
            _pluginEnabled = Config.Bind("General", "PluginEnabled", false,
                "Enables UnlockAll. UnlockAll treats every object in the spawner as unlocked, as well as adding every object in the game to the spawner.");

            _overwriteRewardsTxt = Config.Bind("General", "OverwriteRewardsTxt", false,
                "Overwrites the contents of Rewards.txt with every unlocked object. Even if this is false, however, all reward items will show in the Item Spawner.");

            if (_pluginEnabled.Value)
                Harmony.CreateAndPatchAll(typeof(UnlockAllPlugin));
        }

        public static ItemSpawnerID[] AddFVRObjects(FVRObject[] fvrObjects)
        {
            var objects = new List<FVRObject>(fvrObjects);
            objects.Reverse();
            var extSpawnerIds = new List<ItemSpawnerID>(Resources.LoadAll<ItemSpawnerID>("ItemSpawnerIDs"));
            foreach (var itemSpawnerId in extSpawnerIds)
            {
                if (itemSpawnerId.MainObject != null)
                    objects.Remove(itemSpawnerId.MainObject);
                if (itemSpawnerId.SecondObject != null)
                    objects.Remove(itemSpawnerId.SecondObject);
            }

            foreach (var fvrObject in objects)
            {
                if (fvrObject == null)
                    continue;

                // these are explicitly excluded for the new Item Spawner's scene saving
                // if they weren't, there's a chance it would save invalid ItemSpawnerIDs for entire scenes
                // (e.g. the item spawners in TnH, Rotweiners, MF2)
                if (fvrObject.Category == FVRObject.ObjectCategory.Uncategorized ||
                    fvrObject.Category == FVRObject.ObjectCategory.VFX ||
                    fvrObject.Category == FVRObject.ObjectCategory.SosigClothing)
                    continue;

                var itemId = ScriptableObject.CreateInstance<ItemSpawnerID>();
                itemId.DisplayName = fvrObject.DisplayName;
                itemId.SubHeading = fvrObject.ItemID;
                itemId.Category = ItemSpawnerID.EItemCategory.Misc;
                itemId.SubCategory = ItemSpawnerID.ESubCategory.None;
                itemId.ItemID = "zzz_" + fvrObject.ItemID + "_uncat";
                itemId.MainObject = fvrObject;
                itemId.Secondaries = new ItemSpawnerID[0];
                itemId.Secondaries_ByStringID = new List<string>();
                itemId.TutorialBlocks = new List<string>();
                itemId.ModTags = new List<string> { "UnlockAll - FVRObjects" };
                itemId.UsesHugeSpawnPad = true;
                extSpawnerIds.Add(itemId);
            }

            return extSpawnerIds.ToArray();
        }

        [HarmonyPatch(typeof(IM), nameof(IM.GenerateItemDBs))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GenerateItemDBsTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs).MatchForward(false,
                    new CodeMatch(OpCodes.Ldstr, "ItemSpawnerIDs"),
                    new CodeMatch(OpCodes.Call))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 2)) // I don't like this way
                .SetOperandAndAdvance(AccessTools.Method(typeof(UnlockAllPlugin), "AddFVRObjects"))
                .InstructionEnumeration();
        }

        [HarmonyPatch(typeof(IM), nameof(IM.GenerateItemDBs))]
        [HarmonyPrefix]
        public static bool AddUncategorizedSubCategory(IM __instance)
        {
            ItemSpawnerCategoryDefinitions.SubCategory subcat = new ItemSpawnerCategoryDefinitions.SubCategory
            {
                Subcat = ItemSpawnerID.ESubCategory.None,
                DisplayName = "UNCATEGORIZED",
                DoesDisplay_Sandbox = true,
                DoesDisplay_Unlocks = true,
                Sprite = null
            };

            List<ItemSpawnerCategoryDefinitions.SubCategory> subcats = __instance.CatDefs.Categories[6].Subcats.ToList();
            subcats.Add(subcat);
            __instance.CatDefs.Categories[6].Subcats = subcats.ToArray();

            return true;
        }

        [HarmonyPatch(typeof(RewardUnlocks), nameof(RewardUnlocks.IsRewardUnlocked), typeof(string))]
        [HarmonyPrefix]
        public static bool IsRewardUnlockedPrefix(RewardUnlocks __instance, ref bool __result, string ID)
        {
            if (!__instance.Rewards.Contains(ID) && _overwriteRewardsTxt.Value)
            {
                __instance.UnlockReward(ID);
                GM.Rewards.SaveToFile();
            }

            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(RewardUnlocks), nameof(RewardUnlocks.IsRewardUnlocked), typeof(ItemSpawnerID))]
        [HarmonyPrefix]
        public static bool IsRewardUnlockedPrefix(RewardUnlocks __instance, ref bool __result, ItemSpawnerID ID)
        {
            __result = __instance.IsRewardUnlocked(ID.ItemID);
            return false;
        }
    }
}