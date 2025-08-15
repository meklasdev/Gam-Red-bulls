using UnityEngine;

/// <summary>
/// Strefa pit-stop – tankuje paliwo i naprawia opony.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PitStop : MonoBehaviour
{
	[SerializeField] private float refuelAmount = 999f;
	[SerializeField] private float repairAmount = 1f;

	private void Awake()
	{
		GetComponent<Collider>().isTrigger = true;
	}

	private void OnTriggerStay(Collider other)
	{
		var fuel = other.GetComponent<FuelSystem>();
		if (fuel != null) fuel.Refuel(refuelAmount);
		var tyres = other.GetComponent<TyreWearSystem>();
		if (tyres != null) tyres.Repair(repairAmount);
	}
}