using UnityEngine;

#if UNITY_NETCODE
using Unity.Netcode;
#endif

/// <summary>
/// Synchronizacja pojazdu w sieci.
/// </summary>
public class NetworkVehicle : MonoBehaviour
{
	#if UNITY_NETCODE
	private NetworkObject _netObj;
	private NetworkTransform _netTransform;
	#endif

	private void Awake()
	{
		#if UNITY_NETCODE
		_netObj = GetComponent<NetworkObject>();
		_netTransform = GetComponent<NetworkTransform>();
		#endif
	}
}