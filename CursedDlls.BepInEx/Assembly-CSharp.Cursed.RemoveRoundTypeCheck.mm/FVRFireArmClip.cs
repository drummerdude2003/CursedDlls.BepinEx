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
			[System.NonSerialized]
			public FireArmRoundType LR_Type;
			public FireArmRoundClass LR_Class;
			public Mesh LR_Mesh;
			public Material LR_Material;
			public FVRObject LR_ObjectWrapper;
			public GameObject LR_ProjectilePrefab;
		}

		public FVRLoadedRound[] LoadedRounds;
	}
}
