using UnityEngine;
using UnityEngine.UI;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Input
{
	public class TouchControlsOverlay : MonoBehaviour
	{
		public Slider steerSlider;
		public Slider throttleSlider;
		public Slider brakeSlider;
		public Toggle handbrakeToggle;
		public Toggle boostToggle;

		private void Update()
		{
			if (InputManager.Instance == null) return;
			float steer = steerSlider ? Mathf.Lerp(-1f, 1f, steerSlider.value) : 0f;
			float throttle = throttleSlider ? throttleSlider.value : 0f;
			float brake = brakeSlider ? brakeSlider.value : 0f;
			bool hb = handbrakeToggle && handbrakeToggle.isOn;
			bool boost = boostToggle && boostToggle.isOn;
			InputManager.Instance.SetTouchInput(steer, throttle, brake, hb, boost);
		}
	}
}