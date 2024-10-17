using System;
using UnityEngine;

namespace FistVR
{
	public class patch_FVRLoadedRound : FVRLoadedRound
	{
		[System.NonSerialized]
		public FireArmRoundType LR_Type;
	}
}