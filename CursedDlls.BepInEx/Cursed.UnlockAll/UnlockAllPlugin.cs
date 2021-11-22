﻿using System;
using System.Collections.Generic;
using System.Linq;
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
namespace Cursed.UnlockAll
{
    [BepInPlugin("dll.cursed.unlockall", "CursedDlls - Unlock All Items", "1.4")]
    public class UnlockAllPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> _overwriteRewardsTxt;

        private void Awake()
        {
            _overwriteRewardsTxt = Config.Bind("General", "OverwriteRewardsTxt", false,
                "Overwrites the contents of Rewards.txt with every unlocked object. Even if this is false, however, all reward items will show in the Item Spawner.");

            Harmony.CreateAndPatchAll(typeof(UnlockAllPlugin));
        }

        [HarmonyPatch(typeof(IM), "GenerateItemDBs")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GenerateItemDBsTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldstr, "ItemSpawnerIDs"),
                    new CodeMatch(OpCodes.Call))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_0),
                    Transpilers.EmitDelegate<Func<ItemSpawnerID[], FVRObject[], ItemSpawnerID[]>>(
                        (spawnerIds, fvrObjects) =>
                        {
                            var objects = new List<FVRObject>(fvrObjects);
                            objects.Reverse();
                            var extSpawnerIds = new List<ItemSpawnerID>(spawnerIds);
                            foreach (var itemSpawnerId in spawnerIds)
                            {
                                if (itemSpawnerId.MainObject != null)
                                    objects.Remove(itemSpawnerId.MainObject);
                                if (itemSpawnerId.SecondObject != null)
                                    objects.Remove(itemSpawnerId.SecondObject);
                            }

                            foreach (var fvrObject in objects)
                            {
                                var itemId = ScriptableObject.CreateInstance<ItemSpawnerID>();
                                itemId.DisplayName = fvrObject.DisplayName;
                                itemId.SubHeading = fvrObject.ItemID;
                                itemId.Category = ItemSpawnerID.EItemCategory.Misc;
                                itemId.SubCategory = ItemSpawnerID.ESubCategory.Backpack;
                                itemId.ItemID = fvrObject.ItemID;
                                itemId.MainObject = fvrObject;
                                itemId.Secondaries = new ItemSpawnerID[0];
                                itemId.UsesHugeSpawnPad = true;
                                extSpawnerIds.Add(itemId);
                            }

                            return extSpawnerIds.ToArray();
                        }))
                .InstructionEnumeration();
        }

        [HarmonyPatch(typeof(IM), "GenerateItemDBs")]
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
            if (__instance.Rewards.Contains(ID) && _overwriteRewardsTxt.Value)
            {
                __instance.Rewards.Add(ID);
                using (var writer = ES2Writer.Create("Rewards.txt"))
                {
                    __instance.SaveToFile(writer);
                }
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