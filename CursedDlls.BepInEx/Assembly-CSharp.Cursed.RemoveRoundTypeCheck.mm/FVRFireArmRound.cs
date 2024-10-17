using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FistVR
{
	public class patch_FVRFireArmRound : FVRFireArmRound
	{
		public class ProxyRound
		{
			public GameObject GO;
			public MeshFilter Filter;
			public Renderer Renderer;
			public FireArmRoundType Type;
			public FireArmRoundClass Class;
			public FVRObject ObjectWrapper;
		}

		public List<ProxyRound> ProxyRounds;

		public void UpdateProxyRenderers()
		{
			if (this.ProxyRounds.Count > 0)
			{
				for (int i = 0; i < this.ProxyRounds.Count; i++)
				{
					FireArmRoundType rType = this.ProxyRounds[i].Type;
					FireArmRoundClass rClass = this.ProxyRounds[i].Class;

					// A problem occurs when a user has CursedDLLs installed, but RemoveRoundTypeCheck is not enabled
					// Overwriting this function means the game expects ProxyRound.Type to actually be populated - which would not be the case if the above is true.
					// We can do a basic check here to see if rType is unset, and if it is, let's just get it from the plain FVRFireArmRound.RoundType.
					// This should not cause any issue even for 22LR, as it will just get the expected round type.
					if (rType == 0)
						rType = this.RoundType;

					this.ProxyRounds[i].Filter.mesh = AM.GetRoundMesh(rType, rClass);
					this.ProxyRounds[i].Renderer.material = AM.GetRoundMaterial(rType, rClass);
				}
			}
		}
	}
}
