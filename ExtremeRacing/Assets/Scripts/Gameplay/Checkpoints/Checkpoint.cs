using UnityEngine;

namespace ExtremeRacing.Gameplay
{
	public class Checkpoint : MonoBehaviour
	{
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, 1f);
		}
	}
}