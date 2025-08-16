using Unity.Netcode;
using UnityEngine;

namespace ExtremeRacing.Multiplayer
{
	public class RuntimeNetworkBootstrap : MonoBehaviour
	{
		private void Awake()
		{
			if (NetworkManager.Singleton == null) return;
			var networkPrefabs = Resources.LoadAll<GameObject>("NetworkPrefabs");
			foreach (var prefab in networkPrefabs)
			{
				if (prefab.GetComponent<NetworkObject>() == null) continue;
				try
				{
					NetworkManager.Singleton.AddNetworkPrefab(prefab);
				}
				catch
				{
					// ignore duplicates
				}
			}
		}
	}
}