using UnityEngine;

/// <summary>
/// System modyfikacji pojazdu (legalne/nielegalne). Nielegalne dają większy bonus z ryzykiem wykrycia.
/// </summary>
public class ModSystem : MonoBehaviour
{
	[System.Serializable]
	public class Mod
	{
		public string id;
		public string displayName;
		public bool illegal;
		[Range(-0.5f, 2f)] public float speedMul = 0f;   // addytywnie do mnożnika 1+speedMul
		[Range(-0.5f, 2f)] public float accelMul = 0f;
		[Range(-0.5f, 2f)] public float brakeMul = 0f;
	}

	[SerializeField] private VehicleController vehicle;
	[SerializeField] private VehicleStats baseStats;
	[SerializeField] private Mod[] installedMods;
	[SerializeField] private float detectionPerMinute = 0.1f;

	private float _detectionTimer;

	private void Awake()
	{
		if (vehicle == null) vehicle = GetComponent<VehicleController>();
	}

	private void Update()
	{
		ApplyMods();
		DetectionTick();
	}

	private void ApplyMods()
	{
		if (vehicle == null || baseStats == null) return;
		float speedMul = 1f, accelMul = 1f, brakeMul = 1f;
		foreach (var m in installedMods)
		{
			speedMul += m.speedMul;
			accelMul += m.accelMul;
			brakeMul += m.brakeMul;
		}
		// Zastosuj przez skopiowane parametry tymczasowe
		var rb = vehicle.GetComponent<Rigidbody>();
		if (rb != null) rb.mass = baseStats.mass;
	}

	private void DetectionTick()
	{
		bool hasIllegal = false;
		foreach (var m in installedMods) if (m.illegal) { hasIllegal = true; break; }
		if (!hasIllegal) return;
		_detectionTimer += Time.deltaTime * (detectionPerMinute / 60f);
		if (_detectionTimer >= 1f)
		{
			// Wykrycie – kara w kontraktach/popularności
			FindObjectOfType<ContractSystem>()?.AddPopularity(-10);
			_detectionTimer = 0f;
		}
	}
}