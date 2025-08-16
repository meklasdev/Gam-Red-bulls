using UnityEngine;
using TMPro;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.UI
{
	public class GarageUI : MonoBehaviour
	{
		public TMP_Dropdown vehicleDropdown;
		public Vehicles.VehicleSpawner spawner;
		public VehicleSpec bikeSpec;
		public VehicleSpec motocrossSpec;
		public VehicleSpec supercarSpec;
		public VehicleSpec f1Spec;

		private void Start()
		{
			if (vehicleDropdown != null)
			{
				vehicleDropdown.options.Clear();
				vehicleDropdown.options.Add(new TMP_Dropdown.OptionData("Bike"));
				vehicleDropdown.options.Add(new TMP_Dropdown.OptionData("Motocross"));
				vehicleDropdown.options.Add(new TMP_Dropdown.OptionData("Supercar"));
				vehicleDropdown.options.Add(new TMP_Dropdown.OptionData("F1"));
				vehicleDropdown.onValueChanged.AddListener(OnVehicleChanged);
			}
		}

		private void OnVehicleChanged(int idx)
		{
			if (spawner == null) return;
			switch (idx)
			{
				case 0: spawner.spec = bikeSpec; Gameplay.ProgressionSystem.Instance.SelectVehicle(bikeSpec.vehicleId); break;
				case 1: spawner.spec = motocrossSpec; Gameplay.ProgressionSystem.Instance.SelectVehicle(motocrossSpec.vehicleId); break;
				case 2: spawner.spec = supercarSpec; Gameplay.ProgressionSystem.Instance.SelectVehicle(supercarSpec.vehicleId); break;
				case 3: spawner.spec = f1Spec; Gameplay.ProgressionSystem.Instance.SelectVehicle(f1Spec.vehicleId); break;
			}
			if (spawner.spawned != null) Destroy(spawner.spawned);
			spawner.Spawn();
		}
	}
}