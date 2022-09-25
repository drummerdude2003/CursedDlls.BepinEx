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

		// this is left here as it's very unlikely to change ever
		public void UpdateProxyRenderers()
		{
			if (this.ProxyRounds.Count > 0)
			{
				for (int i = 0; i < this.ProxyRounds.Count; i++)
				{
					this.ProxyRounds[i].Filter.mesh = AM.GetRoundMesh(this.ProxyRounds[i].Type, this.ProxyRounds[i].Class);
					this.ProxyRounds[i].Renderer.material = AM.GetRoundMaterial(this.ProxyRounds[i].Type, this.ProxyRounds[i].Class);
				}
			}
		}
	}
}
