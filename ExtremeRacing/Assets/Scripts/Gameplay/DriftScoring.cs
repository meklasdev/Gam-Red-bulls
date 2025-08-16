using UnityEngine;

namespace ExtremeRacing.Gameplay
{
	public class DriftScoring : MonoBehaviour
	{
		[SerializeField] private float _comboTimeWindow = 2.0f;
		[SerializeField] private float _minDriftAngle = 15f;
		[SerializeField] private float _pointsPerMeter = 10f;

		private float _lastDriftTime;
		private float _currentPoints;
		private Vector3 _lastPosition;

		private void Start()
		{
			_lastPosition = transform.position;
		}

		private void Update()
		{
			Vector3 velocity = (transform.position - _lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
			_lastPosition = transform.position;
			float speed = velocity.magnitude;
			if (speed < 1f) return;

			float angle = Vector3.Angle(transform.forward, velocity.normalized);
			if (angle > _minDriftAngle)
			{
				_lastDriftTime = Time.time;
				_currentPoints += speed * Time.deltaTime * _pointsPerMeter;
			}
			else if (Time.time - _lastDriftTime > _comboTimeWindow)
			{
				if (_currentPoints > 0f)
				{
					Debug.Log($"Drift points: {Mathf.RoundToInt(_currentPoints)}");
					_currentPoints = 0f;
				}
			}
		}
	}
}