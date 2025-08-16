using UnityEngine;

namespace ExtremeRacing.Vehicles
{
	public enum VehicleType
	{
		Bike,
		Motocross,
		Motorcycle,
		Supercar,
		Rally,
		F1,
		Gokart
	}

	[CreateAssetMenu(menuName = "ExtremeRacing/Vehicle Spec", fileName = "VehicleSpec")]
	public class VehicleSpec : ScriptableObject
	{
		public string vehicleId = "vehicle";
		public VehicleType vehicleType = VehicleType.Supercar;
		[Header("Dynamics")]
		public float maxSpeedKmh = 240f;
		public float acceleration = 15f;
		public float grip = 1.0f;
		public float brakePower = 20f;
		public float steerAngle = 30f;
	}
}