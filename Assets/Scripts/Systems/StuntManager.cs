using UnityEngine;

/// <summary>
/// Wykrywanie trików motocross (backflip/frontflip). Wymaga kontrolera motocykla/pojazdu z Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class StuntManager : MonoBehaviour
{
	[SerializeField] private float flipDetectAngle = 300f;
	[SerializeField] private float minAirTime = 0.3f;

	private Rigidbody _rb;
	private MissionSystem _missionSystem;
	private bool _inAir;
	private float _airTimer;
	private float _accumulatedPitch;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_missionSystem = FindObjectOfType<MissionSystem>();
	}

	private void FixedUpdate()
	{
		bool grounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);
		if (!grounded)
		{
			_inAir = true;
			_airTimer += Time.fixedDeltaTime;
			_accumulatedPitch += GetAngularPitchDelta();
		}
		else if (_inAir)
		{
			// Lądowanie – sprawdź triki
			if (_airTimer >= minAirTime)
			{
				if (_accumulatedPitch >= flipDetectAngle)
				{
					_missionSystem?.ReportTrickPerformed("backflip");
				}
				else if (_accumulatedPitch <= -flipDetectAngle)
				{
					_missionSystem?.ReportTrickPerformed("frontflip");
				}
			}
			ResetAir();
		}
	}

	private void ResetAir()
	{
		_inAir = false;
		_airTimer = 0f;
		_accumulatedPitch = 0f;
	}

	private float GetAngularPitchDelta()
	{
		// Użyj komponentu prędkości kątowej do przybliżenia zmiany kąta pitch
		return _rb.angularVelocity.x * Mathf.Rad2Deg * Time.fixedDeltaTime * 1.2f;
	}
}