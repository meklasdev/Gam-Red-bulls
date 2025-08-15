using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tryb przetrwania: proceduralne segmenty trasy i przeszkody, ograniczone paliwo.
/// </summary>
public class SurvivalModeManager : MonoBehaviour
{
	[SerializeField] private Transform player;
	[SerializeField] private GameObject[] segmentPrefabs;
	[SerializeField] private GameObject[] obstaclePrefabs;
	[SerializeField] private int maxSegments = 10;
	[SerializeField] private float segmentLength = 100f;
	[SerializeField] private float obstacleChance = 0.4f;

	private readonly Queue<GameObject> _spawned = new Queue<GameObject>();
	private float _nextZ;

	private void Start()
	{
		for (int i = 0; i < maxSegments; i++) SpawnSegment();
	}

	private void Update()
	{
		if (player == null || _spawned.Count == 0) return;
		var first = _spawned.Peek();
		if (player.position.z - first.transform.position.z > segmentLength)
		{
			Destroy(_spawned.Dequeue());
			SpawnSegment();
		}
	}

	private void SpawnSegment()
	{
		if (segmentPrefabs == null || segmentPrefabs.Length == 0) return;
		var prefab = segmentPrefabs[Random.Range(0, segmentPrefabs.Length)];
		var seg = Instantiate(prefab, new Vector3(0, 0, _nextZ), Quaternion.identity);
		_spawned.Enqueue(seg);
		_nextZ += segmentLength;

		// Obstacles
		if (obstaclePrefabs != null && obstaclePrefabs.Length > 0 && Random.value < obstacleChance)
		{
			int count = Random.Range(1, 4);
			for (int i = 0; i < count; i++)
			{
				var o = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
				Vector3 pos = new Vector3(Random.Range(-6f, 6f), 0f, _nextZ - Random.Range(10f, segmentLength - 10f));
				Instantiate(o, pos, Quaternion.identity, seg.transform);
			}
		}
	}
}