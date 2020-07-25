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

[assembly: AssemblyVersion("1.2")]
namespace Cursed.RemoveRoundTypeCheck
{
    [BepInPlugin("dll.cursed.removeroundtypecheck", "CursedDlls - Remove RoundType Checks", "1.2")]
    public class RemoveRoundTypeCheckPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Logger { get; set; }

        private static ConfigEntry<bool> _typeChecksDisabled;
        private static ConfigEntry<bool> _unlimitedPalmAmount;

        private void Awake()
        {
            _typeChecksDisabled = Config.Bind("General", "TypeChecksDisabled", true,
                "Disables type checking on rounds. This lets you insert any round you want into any gun, magazine, clip, speedloader, or collection of palmed rounds.");
			_unlimitedPalmAmount = Config.Bind("General", "UnlimitedPalmAmount", true,
				"Removes the limit on palm amounts. This lets you palm as many rounds as you want to.");

			Logger = base.Logger;

			if (File.Exists($@"{Paths.BepInExRootPath}\monomod\CursedDlls\Assembly-CSharp.Cursed.RemoveRoundTypeCheck.mm.dll"))
                Harmony.CreateAndPatchAll(typeof(RemoveRoundTypeCheckPlugin));
            else
                Logger.LogError(@"This plugin requires the Assembly-CSharp.Cursed.RemoveRoundType.mm.dll MonoMod patch to function properly! Download and install it from https://github.com/drummerdude2003/CursedDlls.BepinEx/.");
		}

		public static bool TypeCheck(bool condition)
        {
            return condition || _typeChecksDisabled.Value;
        }

		public static bool PalmAmount(int proxies, int maxpalm)
		{
			return proxies < (_unlimitedPalmAmount.Value ? Int32.MaxValue : maxpalm);
		}

		/*
		 * Type patches
		 * Patch instructions that are simiilar to Type == Type to be TypeCheck(Type == Type)
		 */
		[HarmonyPatch(typeof(FVRFireArmClip), nameof(FVRFireArmClip.UpdateInteraction))]
        [HarmonyPatch(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.UpdateInteraction))]
        [HarmonyPatch(typeof(FVRFireArmRound), nameof(FVRFireArmRound.UpdateInteraction))]
        [HarmonyPatch(typeof(FVRFireArmRound), "FVRFixedUpdate")]
        [HarmonyPatch(typeof(FVRFireArmRound), nameof(FVRFireArmRound.OnTriggerEnter))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PatchRoundTypeChecksTranspiler(IEnumerable<CodeInstruction> instrs)
        {
            return new CodeMatcher(instrs).MatchForward(false,
                new CodeMatch(i => i.IsLdloc() || i.IsLdarg()),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "RoundType"),
                new CodeMatch(i => i.opcode == OpCodes.Bne_Un || i.opcode == OpCodes.Bne_Un_S))
            .Repeat(m =>
            {
                m.Advance(2)
                .SetOpcodeAndAdvance(OpCodes.Brfalse)
                .Advance(-1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ceq, null))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemoveRoundTypeCheckPlugin), "TypeCheck")));
            })
            .InstructionEnumeration();
        }

		[HarmonyPatch(typeof(Speedloader), nameof(Speedloader.OnTriggerEnter))]
		[HarmonyPatch(typeof(Speedloader), nameof(Speedloader.UpdateInteraction))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> PatchSpeedloaderRoundTypeChecksTranspiler(IEnumerable<CodeInstruction> instrs)
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
				.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemoveRoundTypeCheckPlugin), "TypeCheck")));
			})
			.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(FVRFireArmRound), "FVRFixedUpdate")]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> PatchSpeedloaderRoundLoadingTranspiler(IEnumerable<CodeInstruction> instrs)
		{
			return new CodeMatcher(instrs).MatchForward(false,
				new CodeMatch(i => i.opcode == OpCodes.Stfld),
				new CodeMatch(i => i.opcode == OpCodes.Ldarg_0),
				new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "m_hoverOverReloadTrigger"),
				new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "SpeedloaderChamber"))
			.Repeat(m =>
			{
				m.Advance(2)
				//this.m_hoverOverReloadTrigger.SpeedloaderChamber = this.RoundType
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FVRFireArmRound), "m_hoverOverReloadTrigger")))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FVRFireArmMagazineReloadTrigger), nameof(FVRFireArmMagazineReloadTrigger.SpeedloaderChamber))))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0, null))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FVRFireArmRound), "RoundType")))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(SpeedloaderChamber), "Type")))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0, null));
			})
			.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(FVRFirearmBeltDisplayData), nameof(FVRFirearmBeltDisplayData.PullPushBelt))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> PatchBeltDisplayDataTranspiler(IEnumerable<CodeInstruction> instrs)
		{
			return new CodeMatcher(instrs).MatchForward(false,
				new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "LR_Class"),
				new CodeMatch(i => i.opcode == OpCodes.Ldc_I4_0),
				new CodeMatch(i => i.opcode == OpCodes.Ldc_I4_0),
				new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddRound"))
			.Repeat(m =>
			{
				m.SetOperandAndAdvance(AccessTools.Field(typeof(FVRLoadedRound), "LR_Type"))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 5))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FVRLoadedRound), "LR_Class")))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AM), nameof(AM.GetRoundSelfPrefab))))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(AnvilAsset), nameof(AnvilAsset.GetGameObject))))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), nameof(GameObject.GetComponent), null, new Type[] { typeof(FVRFireArmRound) })))
				.Advance(2)
				.SetOperandAndAdvance(AccessTools.Method(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.AddRound), new Type[] { typeof(FVRFireArmRound), typeof(bool), typeof(bool) }));
			})
			.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(FVRFireArmMagazine), nameof(FVRFireArmMagazine.UpdateInteraction))]
		[HarmonyPatch(typeof(FVRFireArmClip), nameof(FVRFireArmClip.UpdateInteraction))]
		[HarmonyPatch(typeof(Speedloader), nameof(Speedloader.UpdateInteraction))]
		[HarmonyPatch(typeof(FVRFireArmRound), nameof(FVRFireArmRound.UpdateInteraction))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> MaxPalmedAmountOverride(IEnumerable<CodeInstruction> instrs)
		{
			return new CodeMatcher(instrs).MatchForward(false,
				new CodeMatch(i => i.opcode == OpCodes.Castclass && ((Type)i.operand) == typeof(FVRFireArmRound)),
				new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "MaxPalmedAmount"),
				new CodeMatch(i => i.opcode == OpCodes.Bge || i.opcode == OpCodes.Bge_S))
			.Repeat(m =>
			{
				m.Advance(2)
				.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemoveRoundTypeCheckPlugin), "PalmAmount")))
				.SetOpcodeAndAdvance(OpCodes.Brfalse);
			})
			.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(FVRFireArmRound), nameof(FVRFireArmRound.OnTriggerEnter))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> MaxPalmedAmountOnRoundTouch(IEnumerable<CodeInstruction> instrs)
		{
			return new CodeMatcher(instrs).MatchForward(false,
				new CodeMatch(i => i.opcode == OpCodes.Ldarg_0),
				new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "MaxPalmedAmount"),
				new CodeMatch(i => i.opcode == OpCodes.Bge || i.opcode == OpCodes.Bge_S))
			.Repeat(m =>
			{
				m.Advance(2)
				.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RemoveRoundTypeCheckPlugin), "PalmAmount")))
				.SetOpcodeAndAdvance(OpCodes.Brfalse);
			})
			.InstructionEnumeration();
		}

		/*
		 * Functionality patches
		 * Adds functionality that technically isn't needed, but makes things easier to use and/or abuse
		 */
		[HarmonyPatch(typeof(AmmoSpawnerV2), nameof(AmmoSpawnerV2.LoadIntoHeldObjects))]
		[HarmonyPrefix]
		public static bool AmmoSpawnerV2_LoadIntoHeldObjects(FireArmRoundType ___m_curAmmoType, FireArmRoundClass ___m_curAmmoClass)
		{
			FireArmRoundType curAmmoType = ___m_curAmmoType;
			FireArmRoundClass curAmmoClass = ___m_curAmmoClass;
			for (int i = 0; i < GM.CurrentMovementManager.Hands.Length; i++)
			{
				if (GM.CurrentMovementManager.Hands[i].CurrentInteractable != null && GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRPhysicalObject)
				{
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArmMagazine)
					{
						FVRFireArmMagazine fvrfireArmMagazine = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArmMagazine;
						if (TypeCheck(fvrfireArmMagazine.RoundType == curAmmoType))
						{
							fvrfireArmMagazine.m_numRounds = 0;
							for (int j = 0; j < fvrfireArmMagazine.LoadedRounds.Length; j++)
								fvrfireArmMagazine.AddRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>(), false, true);
							fvrfireArmMagazine.UpdateBulletDisplay();
						}
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArmClip)
					{
						FVRFireArmClip fvrfireArmClip = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArmClip;
						if (TypeCheck(fvrfireArmClip.RoundType == curAmmoType))
						{
							fvrfireArmClip.m_numRounds = 0;
							for (int j = 0; j < fvrfireArmClip.LoadedRounds.Length; j++)
								fvrfireArmClip.AddRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>(), false, true);
							fvrfireArmClip.UpdateBulletDisplay();
						}
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is Speedloader)
					{
						Speedloader speedloader = GM.CurrentMovementManager.Hands[i].CurrentInteractable as Speedloader;
						if (TypeCheck(speedloader.Chambers[0].Type == curAmmoType))
						{
							for (int j = 0; j < speedloader.Chambers.Count; j++)
							{
								speedloader.Chambers[j].Type = curAmmoType;
								speedloader.Chambers[j].Load(curAmmoClass);
							}
						}
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArm)
					{
						FVRFireArm fvrfireArm = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArm;
						if (TypeCheck(fvrfireArm.RoundType == curAmmoType) && fvrfireArm.Magazine != null)
						{
							fvrfireArm.Magazine.m_numRounds = 0;
							for (int j = 0; j < fvrfireArm.Magazine.LoadedRounds.Length; j++)
								fvrfireArm.Magazine.AddRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>(), false, true);
							fvrfireArm.Magazine.UpdateBulletDisplay();
						}
						if (TypeCheck(fvrfireArm.RoundType == curAmmoType) && fvrfireArm.Clip != null)
						{
							fvrfireArm.Clip.m_numRounds = 0;
							for (int j = 0; j < fvrfireArm.Clip.LoadedRounds.Length; j++)
								fvrfireArm.Clip.AddRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>(), false, true);
							fvrfireArm.Clip.UpdateBulletDisplay();
						}

						if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is BreakActionWeapon)
						{
							BreakActionWeapon baw = GM.CurrentMovementManager.Hands[i].CurrentInteractable as BreakActionWeapon;
							if (TypeCheck(baw.RoundType == curAmmoType))
								for (int j = 0; j < baw.Barrels.Length; j++)
									baw.Barrels[j].Chamber.SetRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>());
						}
						if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is Derringer)
						{
							Derringer derringer = GM.CurrentMovementManager.Hands[i].CurrentInteractable as Derringer;
							if (TypeCheck(derringer.RoundType == curAmmoType))
								for (int j = 0; j < derringer.Barrels.Count; j++)
									derringer.Barrels[j].Chamber.SetRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>());
						}
						else if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is SingleActionRevolver)
						{
							SingleActionRevolver saRevolver = GM.CurrentMovementManager.Hands[i].CurrentInteractable as SingleActionRevolver;
							if (TypeCheck(saRevolver.Cylinder.Chambers[0].RoundType == curAmmoType))
								for (int j = 0; j < saRevolver.Cylinder.Chambers.Length; j++)
									saRevolver.Cylinder.Chambers[j].SetRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>());
						}
						if (GM.CurrentMovementManager.Hands[i].CurrentInteractable.GetType().GetField("Chamber") != null) //handles most guns
						{
							FVRFireArmChamber Chamber = (FVRFireArmChamber)GM.CurrentMovementManager.Hands[i].CurrentInteractable.GetType().GetField("Chamber").GetValue(GM.CurrentMovementManager.Hands[i].CurrentInteractable);
							if (TypeCheck(fvrfireArm.RoundType == curAmmoType))
								Chamber.SetRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>());
						}
						if (GM.CurrentMovementManager.Hands[i].CurrentInteractable.GetType().GetField("Chambers") != null) //handles Revolver, LAPD2019, RevolvingShotgun
						{
							FVRFireArmChamber[] Chambers = (FVRFireArmChamber[])GM.CurrentMovementManager.Hands[i].CurrentInteractable.GetType().GetField("Chambers").GetValue(GM.CurrentMovementManager.Hands[i].CurrentInteractable);
							if (TypeCheck(Chambers[0].RoundType == curAmmoType))
								for (int j = 0; j < Chambers.Length; j++)
									Chambers[j].SetRound(AM.GetRoundSelfPrefab(curAmmoType, curAmmoClass).GetGameObject().GetComponent<FVRFireArmRound>());
						}
					}
				}
			}
			return false;
		}

		[HarmonyPatch(typeof(AmmoSpawnerV2), "CheckFillButton")]
		[HarmonyPrefix]
		public static bool AmmoSpawnerV2_CheckFillButton(AmmoSpawnerV2 __instance, FireArmRoundType ___m_curAmmoType, ref bool ___m_hasHeldType, ref FireArmRoundType ___heldType)
		{
			bool showFill = false;
			for (int i = 0; i < GM.CurrentMovementManager.Hands.Length; i++)
			{
				if (GM.CurrentMovementManager.Hands[i].CurrentInteractable != null && GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRPhysicalObject)
				{
					FireArmRoundType curAmmoType = ___m_curAmmoType;
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArmMagazine)
					{
						FVRFireArmMagazine fvrfireArmMagazine = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArmMagazine;
						___m_hasHeldType = true;
						___heldType = fvrfireArmMagazine.RoundType;
						if (TypeCheck(fvrfireArmMagazine.RoundType == curAmmoType))
							showFill = true;
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArmClip)
					{
						FVRFireArmClip fvrfireArmClip = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArmClip;
						___m_hasHeldType = true;
						___heldType = fvrfireArmClip.RoundType;
						if (TypeCheck(fvrfireArmClip.RoundType == curAmmoType))
							showFill = true;
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is Speedloader)
					{
						Speedloader speedloader = GM.CurrentMovementManager.Hands[i].CurrentInteractable as Speedloader;
						___m_hasHeldType = true;
						___heldType = speedloader.Chambers[0].Type;
						if (TypeCheck(speedloader.Chambers[0].Type == curAmmoType))
							showFill = true;
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArm)
					{
						FVRFireArm fvrfireArm = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArm;
						___m_hasHeldType = true;
						___heldType = fvrfireArm.RoundType;
						if (TypeCheck(fvrfireArm.RoundType == curAmmoType) && (fvrfireArm.Magazine != null || fvrfireArm.Clip != null))
							showFill = true;

						if (!showFill) //Excessive reflection is slow, so if we already found a mag or clip there's no point in checking for revolvers or break actions
						{
							if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is BreakActionWeapon)
							{
								BreakActionWeapon baw = GM.CurrentMovementManager.Hands[i].CurrentInteractable as BreakActionWeapon;
								if (TypeCheck(baw.RoundType == curAmmoType))
									showFill = true;
							}
							if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is Derringer)
							{
								Derringer derringer = GM.CurrentMovementManager.Hands[i].CurrentInteractable as Derringer;
								if (TypeCheck(derringer.RoundType == curAmmoType))
									showFill = true;
							}
							if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is SingleActionRevolver)
							{
								SingleActionRevolver saRevolver = GM.CurrentMovementManager.Hands[i].CurrentInteractable as SingleActionRevolver;
								if (TypeCheck(saRevolver.Cylinder.Chambers[0].RoundType == curAmmoType))
									showFill = true;
							}
							if (GM.CurrentMovementManager.Hands[i].CurrentInteractable.GetType().GetField("Chamber") != null) //handles most guns
							{
								FVRFireArmChamber Chamber = (FVRFireArmChamber)GM.CurrentMovementManager.Hands[i].CurrentInteractable.GetType().GetField("Chamber").GetValue(GM.CurrentMovementManager.Hands[i].CurrentInteractable);
								if (TypeCheck(fvrfireArm.RoundType == curAmmoType))
									showFill = true;
							}
							if (GM.CurrentMovementManager.Hands[i].CurrentInteractable.GetType().GetField("Chambers") != null) //handles Revolver, LAPD2019, RevolvingShotgun
							{
								FVRFireArmChamber[] Chambers = (FVRFireArmChamber[])GM.CurrentMovementManager.Hands[i].CurrentInteractable.GetType().GetField("Chambers").GetValue(GM.CurrentMovementManager.Hands[i].CurrentInteractable);
								if (TypeCheck(Chambers[0].RoundType == curAmmoType))
									showFill = true;
							}
						}
					}
				}
			}
			if (showFill && !__instance.BTNGO_Fill.activeSelf)
			{
				__instance.BTNGO_Fill.SetActive(true);
			}
			else if (!showFill && __instance.BTNGO_Fill.activeSelf)
			{
				__instance.BTNGO_Fill.SetActive(false);
			}
			if (___m_hasHeldType && !__instance.BTNGO_Select.activeSelf)
			{
				__instance.BTNGO_Select.SetActive(true);
			}
			else if (!___m_hasHeldType && __instance.BTNGO_Select.activeSelf)
			{
				__instance.BTNGO_Select.SetActive(false);
			}
			return false;
		}

		[HarmonyPatch(typeof(FVRFireArmClip), nameof(FVRFireArmClip.LoadOneRoundFromClipToMag))]
		[HarmonyPrefix]
		public static bool FVRFireArmClip_LoadOneRoundFromClipToMag(FVRFireArmClip __instance)
		{
			if (__instance.FireArm == null || __instance.FireArm.Magazine == null || __instance.FireArm.Magazine.IsFull() || !__instance.HasARound())
			{
				return false;
			}
			FVRFireArmRound rnd = __instance.RemoveRound(false).GetComponent<FVRFireArmRound>();
			SM.PlayGenericSound(__instance.LoadFromClipToMag, __instance.transform.position);
			__instance.FireArm.Magazine.AddRound(rnd, false, true);
			return false;
		}

		[HarmonyPatch(typeof(Speedloader), nameof(Speedloader.DuplicateFromSpawnLock))]
		[HarmonyPrefix]
		public static bool Speedloader_DuplicateFromSpawnLock(Speedloader __instance, ref GameObject __result, FVRViveHand hand)
		{
			//unlike MonoMod the base class can technically be retrived, but it's a PitA so again this is copied from FVRPhysicalObject.DuplicateFromSpawnLock
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.ObjectWrapper.GetGameObject(), __instance.Transform.position, __instance.Transform.rotation);
			FVRPhysicalObject fvrObj = gameObject.GetComponent<FVRPhysicalObject>();
			if (fvrObj is FVREntityProxy)
				(fvrObj as FVREntityProxy).Data.PrimeDataLists((fvrObj as FVREntityProxy).Flags);

			hand.ForceSetInteractable(fvrObj);
			fvrObj.SetQuickBeltSlot(null);
			fvrObj.BeginInteraction(hand);

			Speedloader component = gameObject.GetComponent<Speedloader>();
			for (int i = 0; i < __instance.Chambers.Count; i++)
			{
				component.Chambers[i].Type = __instance.Chambers[i].Type;

				if (__instance.Chambers[i].IsLoaded)
					component.Chambers[i].Load(__instance.Chambers[i].LoadedClass, false);
				else
					component.Chambers[i].Unload();
			}
			__result = gameObject;
			return false;
		}

		[HarmonyPatch(typeof(RevolverCylinder), nameof(RevolverCylinder.LoadFromSpeedLoader))]
		[HarmonyPrefix]
		public static bool RevolverCylinder_LoadFromSpeedLoader(RevolverCylinder __instance, Speedloader loader)
		{
			bool flag = false;
			for (int i = 0; i < loader.Chambers.Count; i++)
			{
				if (i < __instance.Revolver.Chambers.Length)
				{
					if (loader.Chambers[i].IsLoaded)
					{
						if (!__instance.Revolver.Chambers[i].IsFull)
						{
							__instance.Revolver.Chambers[i].RoundType = loader.Chambers[i].Type;
							__instance.Revolver.Chambers[i].Autochamber(loader.Chambers[i].Unload());
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				__instance.Revolver.PlayAudioEvent(FirearmAudioEventType.MagazineIn, 1f);
			}
			return false;
		}

		[HarmonyPatch(typeof(RevolvingShotgun), nameof(RevolvingShotgun.LoadCylinder))]
		[HarmonyPrefix]
		public static bool RevolvingShotgun_LoadCylinder(RevolvingShotgun __instance, Speedloader s, ref int ___m_curChamber, ref bool __result)
		{
			if (__instance.CylinderLoaded)
				return false;

			__instance.CylinderLoaded = true;
			__instance.ProxyCylinder.gameObject.SetActive(__instance.CylinderLoaded);
			__instance.PlayAudioEvent(FirearmAudioEventType.MagazineIn, 1f);
			___m_curChamber = 0;
			__instance.ProxyCylinder.localRotation = __instance.GetLocalRotationFromCylinder(___m_curChamber);
			for (int i = 0; i < __instance.Chambers.Length; i++)
			{
				if (s.Chambers[i].IsLoaded)
				{
					__instance.Chambers[i].RoundType = s.Chambers[i].Type;
					__instance.Chambers[i].Autochamber(s.Chambers[i].LoadedClass);
					__instance.Chambers[i].IsSpent = s.Chambers[i].IsSpent;
				}
				else
					__instance.Chambers[i].Unload();

				__instance.Chambers[i].UpdateProxyDisplay();
			}

			__result = true;
			return false;
		}

		[HarmonyPatch(typeof(RevolvingShotgun), nameof(RevolvingShotgun.EjectCylinder))]
		[HarmonyPrefix]
		public static bool RevolvingShotgun_EjectCylinder(RevolvingShotgun __instance, ref Speedloader __result)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.CylinderPrefab, __instance.CyclinderMountPoint.position, __instance.CyclinderMountPoint.rotation);
			Speedloader component = gameObject.GetComponent<Speedloader>();
			__instance.PlayAudioEvent(FirearmAudioEventType.MagazineOut, 1f);
			for (int i = 0; i < component.Chambers.Count; i++)
			{
				if (!__instance.Chambers[i].IsFull)
				{
					component.Chambers[i].Unload();
				}
				else if (__instance.Chambers[i].IsSpent)
				{
					component.Chambers[i].Type = __instance.Chambers[i].GetRound().RoundType;
					component.Chambers[i].LoadEmpty(__instance.Chambers[i].GetRound().RoundClass, false);
				}
				else
				{
					component.Chambers[i].Type = __instance.Chambers[i].GetRound().RoundType;
					component.Chambers[i].Load(__instance.Chambers[i].GetRound().RoundClass, false);
				}
				__instance.Chambers[i].UpdateProxyDisplay();
			}
			__instance.EjectDelay = 0.4f;
			__instance.CylinderLoaded = false;
			__instance.ProxyCylinder.gameObject.SetActive(__instance.CylinderLoaded);

			__result = component;
			return false;
		}

		[HarmonyPatch(typeof(SpeedloaderChamber), nameof(SpeedloaderChamber.LoadEmpty))]
		[HarmonyPrefix]
		public static bool SpeedloaderChamber_LoadEmpty(SpeedloaderChamber __instance, FireArmRoundClass rclass, bool playSound = false)
		{
			__instance.IsLoaded = true;
			__instance.IsSpent = true;
			__instance.LoadedClass = rclass;

			if (AM.GetRoundSelfPrefab(__instance.Type, __instance.LoadedClass).GetGameObject().GetComponent<FVRFireArmRound>().FiredRenderer != null)
            {
				__instance.Filter.mesh = AM.GetRoundSelfPrefab(__instance.Type, __instance.LoadedClass).GetGameObject().GetComponent<FVRFireArmRound>().FiredRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh;
				__instance.LoadedRenderer.material = AM.GetRoundMaterial(__instance.Type, __instance.LoadedClass);
				__instance.LoadedRenderer.enabled = true;
				if (playSound && __instance.SpeedLoader.ProfileOverride != null)
				{
					SM.PlayGenericSound(__instance.SpeedLoader.ProfileOverride.MagazineInsertRound, __instance.transform.position);
				}
			}
			else
            {
				__instance.IsLoaded = false;
				__instance.LoadedRenderer.enabled = false;
			}

			return false;
		}

		/*
		 * AM patches
		 * These keeps the game from falling apart if you make something stupid like a FMJ shotgun shell
		 * If you stumble across a .45 ACP FMJ out of nowhere, this is why
		 */
		[HarmonyPatch(typeof(AM), nameof(AM.getRoundMaterial))]
        [HarmonyPrefix]
        public static bool AM_getRoundMaterial(FireArmRoundType rType, FireArmRoundClass rClass, Dictionary<FireArmRoundType, Dictionary<FireArmRoundClass, FVRFireArmRoundDisplayData.DisplayDataClass>> ___TypeDic, ref Material __result)
        {
            try { __result = ___TypeDic[rType][rClass].Material; return false; }
            catch { __result = ___TypeDic[FireArmRoundType.a45_ACP][FireArmRoundClass.FMJ].Material; return false; }
        }

        [HarmonyPatch(typeof(AM), nameof(AM.getRoundMesh))]
        [HarmonyPrefix]
        public static bool AM_getRoundMesh(FireArmRoundType rType, FireArmRoundClass rClass, Dictionary<FireArmRoundType, Dictionary<FireArmRoundClass, FVRFireArmRoundDisplayData.DisplayDataClass>> ___TypeDic, ref Mesh __result)
        {
            try { __result = ___TypeDic[rType][rClass].Mesh; return false; }
            catch { __result = ___TypeDic[FireArmRoundType.a45_ACP][FireArmRoundClass.FMJ].Mesh; return false; }
        }

        [HarmonyPatch(typeof(AM), nameof(AM.getRoundSelfPrefab))]
        [HarmonyPrefix]
        public static bool AM_getRoundSelfPrefab(FireArmRoundType rType, FireArmRoundClass rClass, Dictionary<FireArmRoundType, Dictionary<FireArmRoundClass, FVRFireArmRoundDisplayData.DisplayDataClass>> ___TypeDic, ref FVRObject __result)
        {
            try { __result = ___TypeDic[rType][rClass].ObjectID; return false; }
            catch { __result = ___TypeDic[FireArmRoundType.a45_ACP][FireArmRoundClass.FMJ].ObjectID; return false; }
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