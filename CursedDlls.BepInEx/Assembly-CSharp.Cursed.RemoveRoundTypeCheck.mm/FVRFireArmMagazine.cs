using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FistVR
{
	public class patch_FVRFireArmMagazine : FVRFireArmMagazine
	{
		public void AddRound(FireArmRoundClass rClass, bool makeSound, bool updateDisplay)
		{
			if (this.m_numRounds < this.m_capacity)
			{
				this.m_timeSinceRoundInserted = 0f;
				patch_FVRLoadedRound fvrloadedRound = new patch_FVRLoadedRound();
				fvrloadedRound.LR_Class = rClass;
				fvrloadedRound.LR_Type = this.RoundType;
				fvrloadedRound.LR_Mesh = AM.GetRoundMesh(this.RoundType, rClass);
				fvrloadedRound.LR_Material = AM.GetRoundMaterial(this.RoundType, rClass);
				fvrloadedRound.LR_ObjectWrapper = AM.GetRoundSelfPrefab(this.RoundType, rClass);
				this.LoadedRounds[this.m_numRounds] = fvrloadedRound;
				this.m_numRounds++;
				if (makeSound)
				{
					if (this.FireArm != null)
					{
						if (this.m_numRounds >= this.m_capacity)
						{
							this.FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineInsertRound, 0.8f);
						}
						else
						{
							this.FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineInsertRound, 1f);
						}
					}
					else if (this.UsesOverrideInOut)
					{
						SM.PlayGenericSound(this.ProfileOverride.MagazineInsertRound, base.transform.position);
					}
					else
					{
						SM.PlayGenericSound(this.Profile.MagazineInsertRound, base.transform.position);
					}
				}
			}
			if (updateDisplay)
			{
				this.UpdateBulletDisplay();
			}
		}

		public void AddRound(FVRFireArmRound round, bool makeSound, bool updateDisplay, bool animate)
		{
			if (this.m_numRounds < this.m_capacity)
			{
				this.m_timeSinceRoundInserted = 0f;
				patch_FVRLoadedRound fvrloadedRound = new patch_FVRLoadedRound();
				fvrloadedRound.LR_Class = round.RoundClass;
				fvrloadedRound.LR_Type = round.RoundType;
				fvrloadedRound.LR_Mesh = AM.GetRoundMesh(round.RoundType, round.RoundClass);
				fvrloadedRound.LR_Material = AM.GetRoundMaterial(round.RoundType, round.RoundClass);
				fvrloadedRound.LR_ObjectWrapper = AM.GetRoundSelfPrefab(round.RoundType, round.RoundClass);
				this.LoadedRounds[this.m_numRounds] = fvrloadedRound;
				this.m_numRounds++;
				if (makeSound)
				{
					if (this.FireArm != null)
					{
						if (this.m_numRounds >= this.m_capacity)
						{
							this.FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineInsertRound, 0.8f);
						}
						else
						{
							this.FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineInsertRound, 1f);
						}
					}
					else if (this.m_numRounds >= this.m_capacity)
					{
						SM.PlayCoreSoundOverrides(FVRPooledAudioType.Generic, this.Profile.MagazineInsertRound, base.transform.position, this.Profile.MagazineInsertRound.VolumeRange * 1f, this.Profile.MagazineInsertRound.PitchRange * 0.8f);
					}
					else
					{
						SM.PlayCoreSound(FVRPooledAudioType.Generic, this.Profile.MagazineInsertRound, base.transform.position);
					}
				}
			}
			if (updateDisplay)
			{
				this.UpdateBulletDisplay();
			}
			if (animate && this.m_canAnimate)
			{
				this.DisplayBullets[0].transform.position = round.transform.position;
				this.DisplayBullets[0].transform.rotation = round.transform.rotation;
				this.m_roundInsertionStartPos = round.transform.position;
				this.m_roundInsertionStartRot = round.transform.rotation;
				this.m_roundInsertionLerp = 0f;
			}
		}
	}
}
