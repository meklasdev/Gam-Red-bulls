using UnityEngine;

namespace ExtremeRacing.Vehicles
{
	[RequireComponent(typeof(Rigidbody))]
	public class NitroBoost : MonoBehaviour
	{
		public float boostForce = 5000f;
		public float boostDuration = 3f;
		public float cooldown = 5f;
		private float _timer;
		private bool _active;
		private Rigidbody _rb;

		private void Awake()
		{
			_rb = GetComponent<Rigidbody>();
		}

		private void Update()
		{
			if (_active)
			{
				_timer -= Time.deltaTime;
				if (_timer <= 0f)
				{
					_active = false;
					_timer = cooldown;
				}
			}
			else if (_timer > 0f)
			{
				_timer -= Time.deltaTime;
			}
		}

		public void Trigger()
		{
			if (_active || _timer > 0f) return;
			_active = true;
			_timer = boostDuration;
		}

		private void FixedUpdate()
		{
			if (_active)
			{
				_rb.AddForce(transform.forward * boostForce * Time.fixedDeltaTime, ForceMode.Acceleration);
			}
		}
	}
}