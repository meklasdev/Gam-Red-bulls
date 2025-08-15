using UnityEngine;

/// <summary>
/// Prosty AI kierujący pojazdem po punktach kontrolnych (waypointach).
/// </summary>
[RequireComponent(typeof(VehicleController))]
public class AIController : MonoBehaviour
{
	[SerializeField] private Transform[] waypoints;
	[SerializeField] private float waypointRadius = 5f;
	[SerializeField] private float targetSpeedKmh = 140f;
	[SerializeField] private float steeringGain = 1.0f;

	private int _currentIndex;
	private VehicleController _vehicle;

	private void Awake()
	{
		_vehicle = GetComponent<VehicleController>();
	}

	private void FixedUpdate()
	{
		if (waypoints == null || waypoints.Length == 0) return;
		Transform target = waypoints[_currentIndex];
		Vector3 localTarget = transform.InverseTransformPoint(target.position);
		float steer = Mathf.Clamp(localTarget.x / localTarget.magnitude * steeringGain, -1f, 1f);

		float currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
		float throttle = currentSpeed < targetSpeedKmh ? 1f : 0f;
		float brake = currentSpeed > targetSpeedKmh + 10f ? 1f : 0f;

		// Wyślij bezpośrednio do kontrolera pojazdu przez InputManager
		var state = new InputManager.InputState
		{
			steer = steer,
			throttle = throttle,
			brake = brake,
			handbrake = false
		};
		// Bezpośrednie wywołanie ruchu (omijamy event dla deterministyczności AI)
		var method = typeof(VehicleController).GetMethod("HandleInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		if (method != null) method.Invoke(_vehicle, new object[] { state });

		if (Vector3.Distance(transform.position, target.position) <= waypointRadius)
		{
			_currentIndex = (_currentIndex + 1) % waypoints.Length;
		}
	}
}