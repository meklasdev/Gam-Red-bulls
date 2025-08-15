using UnityEngine;

/// <summary>
/// Kontroler pojazdu. Obsługuje pojazdy z WheelColliderami oraz prosty fallback na Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
	[Header("Konfiguracja")]
	[SerializeField] private VehicleStats stats;
	[SerializeField] private bool useWheelColliders = true;

	[Header("Koła (opcjonalne)")]
	[SerializeField] private WheelCollider[] driveWheels;
	[SerializeField] private WheelCollider[] steerWheels;

	[Header("Wizualizacja kół (opcjonalne)")]
	[SerializeField] private Transform[] wheelVisuals;

	private Rigidbody _rigidbody;
	private float _currentSteerAngle;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
		if (stats != null) _rigidbody.mass = stats.mass;
	}

	private void OnEnable()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnInputUpdated += HandleInput;
		}
	}

	private void OnDisable()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnInputUpdated -= HandleInput;
		}
	}

	private void FixedUpdate()
	{
		ApplyDownforce();
		UpdateWheelVisuals();
		LimitMaxSpeed();
	}

	private void HandleInput(InputManager.InputState state)
	{
		if (useWheelColliders && driveWheels != null && driveWheels.Length > 0)
		{
			ApplyWheelColliderMovement(state);
		}
		else
		{
			ApplyRigidbodyMovement(state);
		}
	}

	private void ApplyWheelColliderMovement(InputManager.InputState state)
	{
		float motor = (stats != null ? stats.wheelTorque : 250f) * state.throttle;
		float brakeTorque = (stats != null ? stats.wheelBrakeTorque : 500f) * (state.brake + (state.handbrake ? 1f : 0f));

		foreach (var wc in driveWheels)
		{
			if (wc == null) continue;
			wc.motorTorque = motor;
			wc.brakeTorque = brakeTorque;
		}

		_currentSteerAngle = Mathf.MoveTowards(_currentSteerAngle,
			(state.steer) * (stats != null ? stats.maxSteerAngle : 25f),
			(stats != null ? stats.steerSpeed : 5f) * Time.fixedDeltaTime * 100f);

		foreach (var wc in steerWheels)
		{
			if (wc == null) continue;
			wc.steerAngle = _currentSteerAngle;
		}
	}

	private void ApplyRigidbodyMovement(InputManager.InputState state)
	{
		Vector3 forward = transform.forward;
		float targetSpeed = (stats != null ? stats.maxSpeedKmh : 150f) / 3.6f;
		float currentSpeed = Vector3.Dot(_rigidbody.velocity, forward);
		float accel = (stats != null ? stats.acceleration : 10f);
		float brake = (stats != null ? stats.brakeForce : 15f);

		float desiredChange = (state.throttle * accel - state.brake * brake) * Time.fixedDeltaTime;
		float newSpeed = Mathf.Clamp(currentSpeed + desiredChange, -targetSpeed, targetSpeed);
		float delta = newSpeed - currentSpeed;
		_rigidbody.AddForce(forward * delta * _rigidbody.mass, ForceMode.Impulse);

		// Prostą zmianę kierunku realizujemy poprzez rotację
		float steerAmount = (stats != null ? stats.maxSteerAngle : 25f) * Mathf.Deg2Rad * state.steer;
		Quaternion deltaRot = Quaternion.Euler(0f, steerAmount, 0f);
		_rigidbody.MoveRotation(_rigidbody.rotation * deltaRot);
	}

	private void ApplyDownforce()
	{
		if (stats == null || stats.downforce <= 0f) return;
		float speed = _rigidbody.velocity.magnitude;
		_rigidbody.AddForce(-transform.up * stats.downforce * speed);
	}

	private void UpdateWheelVisuals()
	{
		if (wheelVisuals == null || driveWheels == null) return;
		int count = Mathf.Min(wheelVisuals.Length, driveWheels.Length);
		for (int i = 0; i < count; i++)
		{
			if (wheelVisuals[i] == null || driveWheels[i] == null) continue;
			Vector3 pos; Quaternion rot;
			driveWheels[i].GetWorldPose(out pos, out rot);
			wheelVisuals[i].SetPositionAndRotation(pos, rot);
		}
	}

	private void LimitMaxSpeed()
	{
		if (stats == null) return;
		float maxSpeed = stats.maxSpeedKmh / 3.6f;
		if (_rigidbody.velocity.magnitude > maxSpeed)
		{
			_rigidbody.velocity = _rigidbody.velocity.normalized * maxSpeed;
		}
	}
}