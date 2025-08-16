using UnityEngine;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.NPC
{
	[RequireComponent(typeof(VehicleController))]
	public class NPCDriver : MonoBehaviour
	{
		public Transform waypoint;
		public float lookAhead = 20f;
		public float desiredSpeedKmh = 120f;
		private VehicleController _vehicle;

		private void Awake()
		{
			_vehicle = GetComponent<VehicleController>();
		}

		private void OnEnable()
		{
			if (_vehicle != null) _vehicle.UseExternalInput(true);
		}

		private void OnDisable()
		{
			if (_vehicle != null) _vehicle.UseExternalInput(false);
		}

		private void FixedUpdate()
		{
			if (_vehicle == null || waypoint == null || _vehicle.spec == null) return;
			Vector3 target = waypoint.position + waypoint.forward * lookAhead;
			Vector3 toTarget = (target - transform.position);
			float steerDir = Vector3.SignedAngle(transform.forward, toTarget.normalized, Vector3.up) / _vehicle.spec.steerAngle;
			steerDir = Mathf.Clamp(steerDir, -1f, 1f);
			float throttle = Mathf.Clamp01((desiredSpeedKmh - _vehicle.GetSpeedKmh()) / desiredSpeedKmh);
			_vehicle.SetExternalInput(steerDir, throttle, 0f, false, false);
		}
	}
}