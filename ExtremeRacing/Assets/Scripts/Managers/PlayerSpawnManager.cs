using UnityEngine;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.Managers
{
	public class PlayerSpawnManager : MonoBehaviour
	{
		public GameObject vehiclePrefab;
		public Transform spawnPoint;
		public UI.HUDController hud;
		public UI.MinimapController minimap;

		private void Start()
		{
			DoSpawn();
		}

		public void DoSpawn()
		{
			if (vehiclePrefab == null)
			{
				vehiclePrefab = Resources.Load<GameObject>("PlayerVehicle");
			}
			if (vehiclePrefab == null) return;
			Transform sp = spawnPoint != null ? spawnPoint : GameObject.Find("SpawnPoint")?.transform;
			if (sp == null) sp = transform;
			var go = Instantiate(vehiclePrefab, sp.position, sp.rotation);
			var vc = go.GetComponent<VehicleController>();
			if (vc != null)
			{
				// Assign spec by saved selection if available
				var id = Gameplay.ProgressionSystem.Instance != null ? Gameplay.ProgressionSystem.Instance.GetSelectedVehicleId() : null;
				if (!string.IsNullOrEmpty(id))
				{
					// Could load from Addressables/Resources if needed
				}
			}
			if (hud) hud.playerVehicle = vc;
			if (minimap) minimap.target = go.transform;
		}
	}
}