using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner skrzynek z łupem. Umieszcza skrzynki w punktach w scenie.
/// </summary>
public class LootSpawner : MonoBehaviour
{
	[System.Serializable]
	public class LootPoint
	{
		public Transform point;
		public GameObject lootPrefab;
	}

	[SerializeField] private List<LootPoint> points = new List<LootPoint>();
	[SerializeField] private bool spawnOnStart = true;

	private readonly List<GameObject> _spawned = new List<GameObject>();
	private MissionSystem _missionSystem;

	private void Awake()
	{
		_missionSystem = FindObjectOfType<MissionSystem>();
	}

	private void Start()
	{
		if (spawnOnStart) SpawnAll();
	}

	public void SpawnAll()
	{
		ClearAll();
		foreach (var p in points)
		{
			if (p.point == null || p.lootPrefab == null) continue;
			var go = Instantiate(p.lootPrefab, p.point.position, p.point.rotation, transform);
			var pickup = go.GetComponent<LootPickup>();
			if (pickup == null) pickup = go.AddComponent<LootPickup>();
			pickup.OnCollected += HandleCollected;
			_spawned.Add(go);
		}
	}

	public void ClearAll()
	{
		foreach (var go in _spawned) if (go != null) Destroy(go);
		_spawned.Clear();
	}

	private void HandleCollected(LootPickup pickup)
	{
		_missionSystem?.ReportCrateCollected();
	}
}

/// <summary>
/// Prosty komponent skrzynki z eventem zbioru.
/// </summary>
public class LootPickup : MonoBehaviour
{
	public delegate void CollectedHandler(LootPickup pickup);
	public event CollectedHandler OnCollected;

	private void OnTriggerEnter(Collider other)
	{
		OnCollected?.Invoke(this);
		Destroy(gameObject);
	}
}