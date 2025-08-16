using UnityEngine;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Vehicles
{
	[RequireComponent(typeof(Rigidbody))]
	public class VehicleController : MonoBehaviour
	{
		public VehicleSpec spec;
		public VehicleType controlType = VehicleType.Supercar;

		[Header("Wheel Setup (cars/moto)")]
		public WheelCollider wheelFL;
		public WheelCollider wheelFR;
		public WheelCollider wheelRL;
		public WheelCollider wheelRR;

		[Header("Visuals (optional)")]
		public Transform visualFL;
		public Transform visualFR;
		public Transform visualRL;
		public Transform visualRR;

		[SerializeField] private float _currentSpeedKmh;
		[SerializeField] private float _motorTorque = 200f;
		[SerializeField] private float _downforce = 50f;

		private Rigidbody _rb;

		// External input override (for NPC or network)
		private bool _useExternalInput;
		private float _extSteer, _extThrottle, _extBrake;
		private bool _extHandbrake, _extBoost;

		private void Awake()
		{
			_rb = GetComponent<Rigidbody>();
			_rb.centerOfMass = new Vector3(0, -0.5f, 0);
		}

		private void FixedUpdate()
		{
			if (spec == null) return;

			_currentSpeedKmh = _rb.velocity.magnitude * 3.6f;
			_rb.AddForce(-transform.up * _downforce);

			float steer, throttle, brake; bool handbrake;
			if (_useExternalInput)
			{
				steer = _extSteer; throttle = _extThrottle; brake = _extBrake; handbrake = _extHandbrake;
			}
			else
			{
				steer = InputManager.Instance != null ? InputManager.Instance.Steer : 0f;
				throttle = InputManager.Instance != null ? InputManager.Instance.Throttle : 0f;
				brake = InputManager.Instance != null ? InputManager.Instance.Brake : 0f;
				handbrake = InputManager.Instance != null && InputManager.Instance.Handbrake;
			}

			if (controlType == VehicleType.Bike)
			{
				UpdateBike(throttle, steer, brake);
			}
			else
			{
				UpdateWheeled(throttle, steer, brake, handbrake);
			}
		}

		private void UpdateWheeled(float throttle, float steer, float brake, bool handbrake)
		{
			if (wheelFL == null || wheelFR == null || wheelRL == null || wheelRR == null) return;

			float speedFactor = Mathf.InverseLerp(0f, spec.maxSpeedKmh, _currentSpeedKmh);
			float steerLimit = Mathf.Lerp(spec.steerAngle, spec.steerAngle * 0.3f, speedFactor);

			wheelFL.steerAngle = steerLimit * steer;
			wheelFR.steerAngle = steerLimit * steer;

			float torque = _motorTorque * spec.acceleration * Mathf.Clamp01(1f - speedFactor) * throttle;
			wheelRL.motorTorque = torque;
			wheelRR.motorTorque = torque;

			float brakeTorque = (spec.brakePower * 1000f) * Mathf.Max(brake, handbrake ? 1f : 0f);
			wheelFL.brakeTorque = brakeTorque * 0.3f;
			wheelFR.brakeTorque = brakeTorque * 0.3f;
			wheelRL.brakeTorque = brakeTorque * 0.7f;
			wheelRR.brakeTorque = brakeTorque * 0.7f;

			UpdateWheelVisual(wheelFL, visualFL);
			UpdateWheelVisual(wheelFR, visualFR);
			UpdateWheelVisual(wheelRL, visualRL);
			UpdateWheelVisual(wheelRR, visualRR);
		}

		private void UpdateBike(float throttle, float steer, float brake)
		{
			float targetSpeed = spec.maxSpeedKmh * throttle;
			float targetVel = targetSpeed / 3.6f;
			Vector3 forward = transform.forward * spec.acceleration * 50f * throttle;
			_rb.AddForce(forward);

			Quaternion steerRot = Quaternion.AngleAxis(steer * spec.steerAngle * Time.fixedDeltaTime, Vector3.up);
			_rb.MoveRotation(_rb.rotation * steerRot);

			if (brake > 0.01f)
			{
				_rb.velocity = Vector3.MoveTowards(_rb.velocity, Vector3.zero, spec.brakePower * Time.fixedDeltaTime);
			}

			if (_rb.velocity.magnitude > targetVel)
			{
				_rb.velocity = _rb.velocity.normalized * targetVel;
			}
		}

		private void UpdateWheelVisual(WheelCollider col, Transform visual)
		{
			if (visual == null || col == null) return;
			col.GetWorldPose(out Vector3 pos, out Quaternion rot);
			visual.SetPositionAndRotation(pos, rot);
		}

		public float GetSpeedKmh()
		{
			return _currentSpeedKmh;
		}

		public void UseExternalInput(bool use)
		{
			_useExternalInput = use;
		}

		public void SetExternalInput(float steer, float throttle, float brake, bool handbrake, bool boost)
		{
			_extSteer = Mathf.Clamp(steer, -1f, 1f);
			_extThrottle = Mathf.Clamp01(throttle);
			_extBrake = Mathf.Clamp01(brake);
			_extHandbrake = handbrake;
			_extBoost = boost;
		}
	}
}