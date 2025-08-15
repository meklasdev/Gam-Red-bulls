using UnityEngine;

/// <summary>
/// System paliwa: zużycie na czas i dystans, refuel w pit-stopie.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FuelSystem : MonoBehaviour
{
	[SerializeField] private float capacity = 60f; // litry
	[SerializeField] private float fuel = 60f;
	[SerializeField] private float consumptionPerSecond = 0.02f;
	[SerializeField] private float consumptionPerMeter = 0.0005f;

	private Rigidbody _rb;

	public float Fuel => fuel;
	public float Capacity => capacity;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		float delta = consumptionPerSecond * Time.fixedDeltaTime + _rb.velocity.magnitude * consumptionPerMeter;
		fuel = Mathf.Max(0f, fuel - delta);
	}

	public void Refuel(float amount)
	{
		fuel = Mathf.Clamp(fuel + Mathf.Max(0f, amount), 0f, capacity);
	}
}