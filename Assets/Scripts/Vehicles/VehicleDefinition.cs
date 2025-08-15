using UnityEngine;

/// <summary>
/// Definicja pojazdu do garażu i sklepu.
/// </summary>
[CreateAssetMenu(menuName = "RedBull/Vehicle Definition", fileName = "VehicleDefinition")]
public class VehicleDefinition : ScriptableObject
{
	public string vehicleId;
	public string displayName;
	public int cost;
	public VehicleStats stats;
	public GameObject prefab;
}