using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRacing.Gameplay
{
	public class LootSpawner : MonoBehaviour
	{
		public GameObject lootCratePrefab;
		public int count = 20;
		public Vector3 areaSize = new Vector3(500, 0, 500);
		public LayerMask groundMask;
		private readonly List<GameObject> _spawned = new List<GameObject>();

		public void Clear()
		{
			foreach (var go in _spawned)
			{
				if (go != null) Destroy(go);
			}
			_spawned.Clear();
		}

		public void Spawn()
		{
			if (lootCratePrefab == null) return;
			Clear();
			for (int i = 0; i < count; i++)
			{
				Vector3 pos = transform.position + new Vector3(Random.Range(-areaSize.x, areaSize.x), 100f, Random.Range(-areaSize.z, areaSize.z));
				if (Physics.Raycast(pos, Vector3.down, out var hit, 1000f, groundMask))
				{
					var go = Instantiate(lootCratePrefab, hit.point + Vector3.up * 0.5f, Quaternion.identity);
					_spawned.Add(go);
				}
			}
		}
	}
}