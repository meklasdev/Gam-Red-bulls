using UnityEngine;

/// <summary>
/// Zużycie opon: wpływa na przyczepność, można naprawić w pit-stopie.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TyreWearSystem : MonoBehaviour
{
	[SerializeField] private float wear; // 0..1, 1 = zużyte
	[SerializeField] private float wearPerMeter = 0.0001f;

	private Rigidbody _rb;
	private WeatherManager _weather;
	public float GripMultiplier { get; private set; } = 1f;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_weather = FindObjectOfType<WeatherManager>();
	}

	private void FixedUpdate()
	{
		wear = Mathf.Clamp01(wear + _rb.velocity.magnitude * wearPerMeter);
		float weatherMul = _weather != null ? _weather.TractionMultiplier : 1f;
		GripMultiplier = Mathf.Lerp(1f, 0.7f, wear) * weatherMul;
	}

	public void Repair(float amount)
	{
		wear = Mathf.Clamp01(wear - Mathf.Max(0f, amount));
	}
}