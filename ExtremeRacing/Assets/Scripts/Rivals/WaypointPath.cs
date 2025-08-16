using UnityEngine;

namespace ExtremeRacing.Rivals
{
	public class WaypointPath : MonoBehaviour
	{
		public Transform[] waypoints;
		public bool loop = true;

		public Transform GetNext(Transform current)
		{
			if (waypoints == null || waypoints.Length == 0) return null;
			if (current == null) return waypoints[0];
			for (int i = 0; i < waypoints.Length; i++)
			{
				if (waypoints[i] == current)
				{
					int next = i + 1;
					if (next >= waypoints.Length) next = loop ? 0 : waypoints.Length - 1;
					return waypoints[next];
				}
			}
			return waypoints[0];
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			if (waypoints == null) return;
			for (int i = 0; i < waypoints.Length; i++)
			{
				var a = waypoints[i];
				var b = waypoints[(i + 1) % waypoints.Length];
				if (a && b) Gizmos.DrawLine(a.position, b.position);
			}
		}
	}
}