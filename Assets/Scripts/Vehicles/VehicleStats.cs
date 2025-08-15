using UnityEngine;

/// <summary>
/// Parametry pojazdu przechowywane w ScriptableObject, aby łatwo tworzyć warianty.
/// </summary>
[CreateAssetMenu(menuName = "RedBull/Vehicle Stats", fileName = "VehicleStats")]
public class VehicleStats : ScriptableObject
{
	[Header("Napęd i prędkość")]
	public float maxSpeedKmh = 180f;
	public float acceleration = 12f;
	public float brakeForce = 20f;
	public float handbrakeForce = 40f;

	[Header("Sterowanie")]
	public float maxSteerAngle = 25f;
	public float steerSpeed = 5f;

	[Header("Fizyka")]
	public float mass = 1200f;
	public float downforce = 50f;

	[Header("Koła (dla WheelCollider)")]
	public float wheelTorque = 300f;
	public float wheelBrakeTorque = 600f;
}