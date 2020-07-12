using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FistVR
{
	public class patch_FVRFireArmClip : FVRFireArmClip
	{
		public class FVRLoadedRound
		{
			public FireArmRoundType LR_Type;
			public FireArmRoundClass LR_Class;
			public Mesh LR_Mesh;
			public Material LR_Material;
			public FVRObject LR_ObjectWrapper;
			public GameObject LR_ProjectilePrefab;
		}

		public FVRLoadedRound[] LoadedRounds;

		public new GameObject DuplicateFromSpawnLock(FVRViveHand hand)
		{
			GameObject gameObject = base.DuplicateFromSpawnLock(hand);
			patch_FVRFireArmClip component = gameObject.GetComponent<patch_FVRFireArmClip>();
			for (int i = 0; i < Mathf.Min(this.LoadedRounds.Length, component.LoadedRounds.Length); i++)
			{
				if (this.LoadedRounds[i] != null && this.LoadedRounds[i].LR_Mesh != null)
				{
					component.LoadedRounds[i].LR_Class = this.LoadedRounds[i].LR_Class;
					component.LoadedRounds[i].LR_Type = this.LoadedRounds[i].LR_Type;
					component.LoadedRounds[i].LR_Mesh = this.LoadedRounds[i].LR_Mesh;
					component.LoadedRounds[i].LR_Material = this.LoadedRounds[i].LR_Material;
					component.LoadedRounds[i].LR_ObjectWrapper = this.LoadedRounds[i].LR_ObjectWrapper;
				}
			}
			component.m_numRounds = this.m_numRounds;
			component.UpdateBulletDisplay();
			return gameObject;
		}

		public float m_timeSinceRoundInserted;

		public void AddRound(FireArmRoundClass rClass, bool makeSound, bool updateDisplay)
		{
			if (this.m_numRounds < this.m_capacity)
			{
				this.m_timeSinceRoundInserted = 0f;
				FVRLoadedRound fvrloadedRound = new FVRLoadedRound();
				fvrloadedRound.LR_Class = rClass;
				fvrloadedRound.LR_Type = this.RoundType;
				fvrloadedRound.LR_Mesh = AM.GetRoundMesh(this.RoundType, rClass);
				fvrloadedRound.LR_Material = AM.GetRoundMaterial(this.RoundType, rClass);
				fvrloadedRound.LR_ObjectWrapper = AM.GetRoundSelfPrefab(this.RoundType, rClass);
				this.LoadedRounds[this.m_numRounds] = fvrloadedRound;
				this.m_numRounds++;
				if (makeSound)
				{
					SM.PlayGenericSound(this.InsertOntoClip, base.transform.position);
				}
			}
			if (updateDisplay)
			{
				this.UpdateBulletDisplay();
			}
		}

		public void AddRound(FVRFireArmRound round, bool makeSound, bool updateDisplay)
		{
			if (this.m_numRounds < this.m_capacity)
			{
				this.m_timeSinceRoundInserted = 0f;
				FVRLoadedRound fvrloadedRound = new FVRLoadedRound();
				fvrloadedRound.LR_Class = round.RoundClass;
				fvrloadedRound.LR_Type = round.RoundType;
				fvrloadedRound.LR_Mesh = AM.GetRoundMesh(round.RoundType, round.RoundClass);
				fvrloadedRound.LR_Material = AM.GetRoundMaterial(round.RoundType, round.RoundClass);
				fvrloadedRound.LR_ObjectWrapper = AM.GetRoundSelfPrefab(round.RoundType, round.RoundClass);
				this.LoadedRounds[this.m_numRounds] = fvrloadedRound;
				this.m_numRounds++;
				if (makeSound)
				{
					SM.PlayGenericSound(this.InsertOntoClip, base.transform.position);
				}
			}
			if (updateDisplay)
			{
				this.UpdateBulletDisplay();
			}
		}
	}
}
