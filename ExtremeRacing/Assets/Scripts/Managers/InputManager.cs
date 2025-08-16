using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ExtremeRacing.Infrastructure;

namespace ExtremeRacing.Managers
{
	public class InputManager : Singleton<InputManager>
	{
		public float Steer { get; private set; }
		public float Throttle { get; private set; }
		public float Brake { get; private set; }
		public bool Handbrake { get; private set; }
		public bool Boost { get; private set; }

#if ENABLE_INPUT_SYSTEM
		private InputAction _steerAction;
		private InputAction _throttleAction;
		private InputAction _brakeAction;
		private InputAction _handbrakeAction;
		private InputAction _boostAction;
#endif

		private bool _hasTouchOverride;
		private float _ovSteer, _ovThrottle, _ovBrake; 
		private bool _ovHandbrake, _ovBoost;

		protected override void Awake()
		{
			base.Awake();
#if ENABLE_INPUT_SYSTEM
			_steerAction = new InputAction("Steer", binding: "<Gamepad>/leftStick/x");
			_throttleAction = new InputAction("Throttle", binding: "<Gamepad>/rightTrigger");
			_brakeAction = new InputAction("Brake", binding: "<Gamepad>/leftTrigger");
			_handbrakeAction = new InputAction("Handbrake", binding: "<Gamepad>/buttonSouth");
			_boostAction = new InputAction("Boost", binding: "<Gamepad>/buttonNorth");

			_steerAction.AddBinding("<Keyboard>/a");
			_steerAction.AddBinding("<Keyboard>/d");
			_throttleAction.AddBinding("<Keyboard>/w");
			_brakeAction.AddBinding("<Keyboard>/s");
			_handbrakeAction.AddBinding("<Keyboard>/space");
			_boostAction.AddBinding("<Keyboard>/leftShift");

			_steerAction.Enable();
			_throttleAction.Enable();
			_brakeAction.Enable();
			_handbrakeAction.Enable();
			_boostAction.Enable();
#endif
		}

		private void Update()
		{
#if ENABLE_INPUT_SYSTEM
			float steer = _steerAction.ReadValue<float>();
			float throttle = _throttleAction.ReadValue<float>();
			float brake = _brakeAction.ReadValue<float>();
			bool handbrake = _handbrakeAction.ReadValue<float>() > 0.5f;
			bool boost = _boostAction.ReadValue<float>() > 0.5f;
#else
			float steer = Input.GetAxis("Horizontal");
			float throttle = Mathf.Clamp01(Input.GetAxis("Vertical"));
			float brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
			bool handbrake = Input.GetKey(KeyCode.Space);
			bool boost = Input.GetKey(KeyCode.LeftShift);
#endif
			if (_hasTouchOverride)
			{
				steer = _ovSteer;
				throttle = _ovThrottle;
				brake = _ovBrake;
				handbrake = _ovHandbrake;
				boost = _ovBoost;
			}

			Steer = steer;
			Throttle = throttle;
			Brake = brake;
			Handbrake = handbrake;
			Boost = boost;
		}

		public void SetTouchInput(float steer, float throttle, float brake, bool handbrake, bool boost)
		{
			_hasTouchOverride = true;
			_ovSteer = Mathf.Clamp(steer, -1f, 1f);
			_ovThrottle = Mathf.Clamp01(throttle);
			_ovBrake = Mathf.Clamp01(brake);
			_ovHandbrake = handbrake;
			_ovBoost = boost;
		}

		public void ClearTouchOverride()
		{
			_hasTouchOverride = false;
		}
	}
}