using UnityEngine;

/// <summary>
/// Menedżer wejścia gracza. Agreguje wejście z ekranu dotykowego oraz klawiatury (fallback w edytorze/PC)
/// i emituje zunifikowany stan wejścia dla pojazdów i innych systemów.
/// </summary>
public class InputManager : Singleton<InputManager>
{
	[System.Serializable]
	public struct InputState
	{
		public float throttle;   // 0..1
		public float brake;      // 0..1
		public float steer;      // -1..1 (lewo/prawo)
		public bool handbrake;
	}

	public delegate void InputUpdatedHandler(InputState state);
	public event InputUpdatedHandler OnInputUpdated;

	[SerializeField] private bool useKeyboardInEditor = true;
	[SerializeField] private float steerSensitivity = 1.5f;

	private InputState _currentState;

	private void Update()
	{
		ReadInputs();
		OnInputUpdated?.Invoke(_currentState);
	}

	/// <summary>
	/// Odczyt wejść z urządzenia. Na mobile oczekuje połączenia z UI (publiczne metody poniżej),
	/// w edytorze/PC fallback na klawiaturę.
	/// </summary>
	private void ReadInputs()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE
		if (useKeyboardInEditor)
		{
			_currentState.steer = Mathf.Clamp(Input.GetAxis("Horizontal") * steerSensitivity, -1f, 1f);
			_currentState.throttle = Mathf.Clamp01(Input.GetAxis("Vertical"));
			_currentState.brake = Mathf.Clamp01(-Input.GetAxis("Vertical"));
			_currentState.handbrake = Input.GetKey(KeyCode.Space);
			return;
		}
		#endif
		// Na urządzeniach mobilnych stan ustawiany jest przez UI-owe handlery
	}

	// Poniższe metody mogą być podpięte do przycisków UI (EventTrigger/OnClick/OnPointerDown/Up)
	public void SetSteer(float value)
	{
		_currentState.steer = Mathf.Clamp(value, -1f, 1f);
	}

	public void SetThrottle(float value)
	{
		_currentState.throttle = Mathf.Clamp01(value);
	}

	public void SetBrake(float value)
	{
		_currentState.brake = Mathf.Clamp01(value);
	}

	public void SetHandbrake(bool pressed)
	{
		_currentState.handbrake = pressed;
	}
}