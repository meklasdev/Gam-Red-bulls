using UnityEngine;

namespace ExtremeRacing.Gameplay
{
	public class CheckpointGate : MonoBehaviour
	{
		public LapManager manager;
		public int index;

		private void OnTriggerEnter(Collider other)
		{
			if (manager == null) return;
			manager.OnGatePassed(index, other);
		}
	}
}