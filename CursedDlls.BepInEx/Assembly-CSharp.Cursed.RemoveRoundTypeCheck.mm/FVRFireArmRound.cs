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

		public void AddProxy(FireArmRoundClass roundClass, FVRObject prefabWrapper)
		{
			patch_FVRFireArmRound.ProxyRound proxyRound = new patch_FVRFireArmRound.ProxyRound();
			GameObject gameObject = new GameObject("Proxy");
			proxyRound.GO = gameObject;
			gameObject.transform.SetParent(base.transform);
			proxyRound.Filter = gameObject.AddComponent<MeshFilter>();
			proxyRound.Renderer = gameObject.AddComponent<MeshRenderer>();
			proxyRound.Class = roundClass;
			proxyRound.Type = prefabWrapper.GetGameObject().GetComponent<FVRFireArmRound>().RoundType;
			proxyRound.ObjectWrapper = prefabWrapper;
			this.ProxyRounds.Add(proxyRound);
		}

		public void PalmRound(FVRFireArmRound round, bool insertAtFront, bool updateDisplay, int addAtIndex = 0)
		{
			SM.PlayHandlingGrabSound(this.HandlingGrabSound, base.transform.position, false);
			patch_FVRFireArmRound.ProxyRound proxyRound = new patch_FVRFireArmRound.ProxyRound();
			GameObject gameObject = new GameObject("Proxy");
			proxyRound.GO = gameObject;
			gameObject.transform.SetParent(base.transform);
			proxyRound.Filter = gameObject.AddComponent<MeshFilter>();
			proxyRound.Renderer = gameObject.AddComponent<MeshRenderer>();
			proxyRound.Class = round.RoundClass;
			proxyRound.Type = round.RoundType;
			proxyRound.ObjectWrapper = round.ObjectWrapper;
			if (insertAtFront)
			{
				for (int i = this.ProxyRounds.Count - 1; i >= 1; i--)
				{
					this.ProxyRounds[i] = this.ProxyRounds[i - 1];
				}
				this.ProxyRounds[0] = proxyRound;
			}
			else
			{
				this.ProxyRounds.Add(proxyRound);
			}
			this.HoveredOverRound = null;
			UnityEngine.Object.Destroy(round.gameObject);
			if (updateDisplay)
			{
				this.UpdateProxyDisplay();
			}
		}

		public void UpdateProxyDisplay()
		{
			if (this.ProxyRounds.Count > 0)
			{
				if (base.IsHeld)
				{
					Vector3 position = base.transform.position;
					Vector3 a = -base.transform.forward * this.PalmingDimensions.z + -base.transform.up * this.PalmingDimensions.y;
					for (int i = 0; i < this.ProxyRounds.Count; i++)
					{
						this.ProxyRounds[i].GO.transform.position = position + a * (float)(i + 2);
						this.ProxyRounds[i].GO.transform.localRotation = Quaternion.identity;
					}
				}
				else
				{
					Vector3 position2 = base.transform.position;
					Vector3 a2 = -base.transform.up * this.PalmingDimensions.y;
					for (int j = 0; j < this.ProxyRounds.Count; j++)
					{
						this.ProxyRounds[j].GO.transform.position = position2 + a2 * (float)(j + 2);
						this.ProxyRounds[j].GO.transform.localRotation = Quaternion.identity;
					}
				}
				for (int k = 0; k < this.ProxyRounds.Count; k++)
				{
					this.ProxyRounds[k].Filter.mesh = AM.GetRoundMesh(this.ProxyRounds[k].Type, this.ProxyRounds[k].Class);
					this.ProxyRounds[k].Renderer.material = AM.GetRoundMaterial(this.ProxyRounds[k].Type, this.ProxyRounds[k].Class);
				}
			}
		}
	}
}
