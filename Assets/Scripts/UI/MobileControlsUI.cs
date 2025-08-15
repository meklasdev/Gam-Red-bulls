using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Proste sterowanie ekranowe dla mobile. Podłącz pod przyciski (EventTrigger PointerDown/Up).
/// </summary>
public class MobileControlsUI : MonoBehaviour
{
	[SerializeField] private float steerValue = 1f;
	[SerializeField] private float throttleValue = 1f;
	[SerializeField] private float brakeValue = 1f;

	private bool _leftHeld, _rightHeld, _throttleHeld, _brakeHeld, _handbrakeHeld;

	private void Update()
	{
		if (InputManager.Instance == null) return;
		float steer = (_leftHeld ? -steerValue : 0f) + (_rightHeld ? steerValue : 0f);
		InputManager.Instance.SetSteer(Mathf.Clamp(steer, -1f, 1f));
		InputManager.Instance.SetThrottle(_throttleHeld ? throttleValue : 0f);
		InputManager.Instance.SetBrake(_brakeHeld ? brakeValue : 0f);
		InputManager.Instance.SetHandbrake(_handbrakeHeld);
	}

	public void OnLeftDown() { _leftHeld = true; }
	public void OnLeftUp() { _leftHeld = false; }
	public void OnRightDown() { _rightHeld = true; }
	public void OnRightUp() { _rightHeld = false; }
	public void OnThrottleDown() { _throttleHeld = true; }
	public void OnThrottleUp() { _throttleHeld = false; }
	public void OnBrakeDown() { _brakeHeld = true; }
	public void OnBrakeUp() { _brakeHeld = false; }
	public void OnHandbrakeDown() { _handbrakeHeld = true; }
	public void OnHandbrakeUp() { _handbrakeHeld = false; }
}