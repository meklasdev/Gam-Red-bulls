using UnityEngine;

#if UNITY_NETCODE
using Unity.Netcode;
#endif

/// <summary>
/// Rozruch sieci: uruchamia hosta/klienta w trybie testowym.
/// </summary>
public class NetworkBootstrap : MonoBehaviour
{
	[SerializeField] private bool autoStartAsHostInEditor = true;

	private void Start()
	{
		#if UNITY_NETCODE
		#if UNITY_EDITOR
		if (autoStartAsHostInEditor)
		{
			if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
			{
				NetworkManager.Singleton.StartHost();
			}
		}
		#endif
		#endif
	}
}