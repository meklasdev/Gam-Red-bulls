using UnityEngine;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.Vehicles
{
	public class VehicleSpawner : MonoBehaviour
	{
		public GameObject vehiclePrefab;
		public VehicleSpec spec;
		public Transform spawnPoint;
		public bool spawnOnStart = true;
		[HideInInspector] public GameObject spawned;

		private void Start()
		{
			if (spawnOnStart) Spawn();
		}

		public void Spawn()
		{
			if (vehiclePrefab == null) return;
			Transform sp = spawnPoint != null ? spawnPoint : transform;
			spawned = Instantiate(vehiclePrefab, sp.position, sp.rotation);
			var vc = spawned.GetComponent<VehicleController>();
			if (vc != null && spec != null) vc.spec = spec;
		}
	}
}