using Unity.Netcode;
using UnityEngine;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.Multiplayer
{
	[RequireComponent(typeof(VehicleController))]
	public class NetworkVehicleSync : NetworkBehaviour
	{
		private VehicleController _vehicle;
		private NetworkVariable<Vector3> _netPos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
		private NetworkVariable<Quaternion> _netRot = new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

		public override void OnNetworkSpawn()
		{
			_vehicle = GetComponent<VehicleController>();
		}

		private void Update()
		{
			if (IsOwner)
			{
				_netPos.Value = transform.position;
				_netRot.Value = transform.rotation;
			}
			else
			{
				transform.SetPositionAndRotation(Vector3.Lerp(transform.position, _netPos.Value, 0.2f), Quaternion.Slerp(transform.rotation, _netRot.Value, 0.2f));
			}
		}
	}
}