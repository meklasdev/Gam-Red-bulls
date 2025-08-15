using UnityEngine;

/// <summary>
/// Prosty kontroler BMX/roweru: napęd na pedałowanie, balans, skręt kierownicą.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BicycleController : MonoBehaviour
{
	[SerializeField] private float pedalForce = 200f;
	[SerializeField] private float brakeForce = 300f;
	[SerializeField] private float steerSpeed = 2.5f;
	[SerializeField] private float maxLeanAngle = 25f;
	[SerializeField] private float selfBalanceTorque = 8f;

	private Rigidbody _rb;
	private float _steerInput;
	private float _throttleInput;
	private float _brakeInput;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_rb.centerOfMass = new Vector3(0f, -0.3f, 0f);
	}

	private void OnEnable()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnInputUpdated += OnInput;
		}
	}

	private void OnDisable()
	{
		if (InputManager.Instance != null)
		{
			InputManager.Instance.OnInputUpdated -= OnInput;
		}
	}

	private void FixedUpdate()
	{
		ApplyDrive();
		ApplySteerAndLean();
		ApplySelfBalance();
	}

	private void OnInput(InputManager.InputState s)
	{
		_throttleInput = s.throttle;
		_brakeInput = s.brake + (s.handbrake ? 1f : 0f);
		_steerInput = s.steer;
	}

	private void ApplyDrive()
	{
		Vector3 forward = transform.forward;
		float force = pedalForce * _throttleInput - brakeForce * _brakeInput;
		_rb.AddForce(forward * force * Time.fixedDeltaTime, ForceMode.Acceleration);
	}

	private void ApplySteerAndLean()
	{
		float yaw = _steerInput * steerSpeed;
		Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
		_rb.MoveRotation(_rb.rotation * rot);

		// Lean (pochylenie) względem skrętu
		float targetLean = -_steerInput * maxLeanAngle;
		Quaternion lean = Quaternion.AngleAxis(targetLean, transform.forward);
		_rb.MoveRotation(lean * _rb.rotation);
	}

	private void ApplySelfBalance()
	{
		// Stabilizacja pochylania do pionu gdy brak skrętu
		Vector3 right = transform.right;
		float tilt = Vector3.SignedAngle(right, Vector3.ProjectOnPlane(right, Vector3.up), transform.forward);
		float correction = -tilt * selfBalanceTorque;
		_rb.AddTorque(transform.forward * correction * Time.fixedDeltaTime, ForceMode.Acceleration);
	}
}