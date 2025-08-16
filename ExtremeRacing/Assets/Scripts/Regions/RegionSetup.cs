using UnityEngine;
using ExtremeRacing.Rivals;
using ExtremeRacing.Gameplay;

namespace ExtremeRacing.Regions
{
	public class RegionSetup : MonoBehaviour
	{
		public RivalSpawner rivalSpawner;
		public LapManager lapManager;

		private void Start()
		{
			if (rivalSpawner) rivalSpawner.Spawn();
			if (lapManager) lapManager.Begin();
		}
	}
}