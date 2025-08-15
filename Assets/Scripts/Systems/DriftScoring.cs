using UnityEngine;

/// <summary>
/// System punktacji driftu: mierzy kąt poślizgu, prędkość i dystans.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DriftScoring : MonoBehaviour
{
	[SerializeField] private float minSpeedKmh = 30f;
	[SerializeField] private float minDriftAngle = 15f;
	[SerializeField] private float pointsMultiplier = 1f;

	public delegate void DriftTickHandler(float currentPoints, float currentDistanceMeters);
	public event DriftTickHandler OnDriftTick;

	private Rigidbody _rb;
	private MissionSystem _missionSystem;
	private bool _isDrifting;
	private float _currentPoints;
	private float _currentDistanceMeters;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_missionSystem = FindObjectOfType<MissionSystem>();
	}

	private void FixedUpdate()
	{
		Vector3 velocity = _rb.velocity;
		float speedKmh = velocity.magnitude * 3.6f;
		if (speedKmh < minSpeedKmh)
		{
			EndDriftIfNeeded();
			return;
		}

		Vector3 forward = transform.forward;
		float angle = Vector3.Angle(velocity, forward);

		if (angle >= minDriftAngle)
		{
			_isDrifting = true;
			float pointsDelta = (angle - minDriftAngle) * velocity.magnitude * pointsMultiplier * Time.fixedDeltaTime;
			_currentPoints += Mathf.Max(0f, pointsDelta);
			_currentDistanceMeters += velocity.magnitude * Time.fixedDeltaTime;
			_missionSystem?.ReportDriftMeters(velocity.magnitude * Time.fixedDeltaTime);
			OnDriftTick?.Invoke(_currentPoints, _currentDistanceMeters);
		}
		else
		{
			EndDriftIfNeeded();
		}
	}

	private void EndDriftIfNeeded()
	{
		if (_isDrifting)
		{
			_isDrifting = false;
			_currentPoints = 0f;
			_currentDistanceMeters = 0f;
		}
	}
}