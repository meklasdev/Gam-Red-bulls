using UnityEngine;
using ExtremeRacing.NPC;

namespace ExtremeRacing.Rivals
{
	public class RivalSpawner : MonoBehaviour
	{
		public GameObject rivalPrefab;
		public WaypointPath path;
		public int count = 5;
		public float spacing = 5f;

		public void Spawn()
		{
			if (rivalPrefab == null || path == null || path.waypoints == null || path.waypoints.Length == 0) return;
			for (int i = 0; i < count; i++)
			{
				var wp = path.waypoints[Mathf.Min(i, path.waypoints.Length - 1)];
				var pos = wp.position - wp.forward * spacing * i;
				var rot = Quaternion.LookRotation(wp.forward);
				var go = Instantiate(rivalPrefab, pos, rot);
				var driver = go.GetComponent<NPCDriver>();
				if (driver) driver.waypoint = wp;
			}
		}
	}
}