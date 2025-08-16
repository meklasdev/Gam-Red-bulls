using UnityEngine;
using UnityEngine.UI;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.UI
{
	public class GarageUI : MonoBehaviour
	{
		public Dropdown vehicleDropdown;
		public VehicleSpawner spawner;
		public VehicleSpec bikeSpec;
		public VehicleSpec motocrossSpec;
		public VehicleSpec supercarSpec;
		public VehicleSpec f1Spec;

		private void Start()
		{
			if (vehicleDropdown != null)
			{
				vehicleDropdown.options.Clear();
				vehicleDropdown.options.Add(new Dropdown.OptionData("Bike"));
				vehicleDropdown.options.Add(new Dropdown.OptionData("Motocross"));
				vehicleDropdown.options.Add(new Dropdown.OptionData("Supercar"));
				vehicleDropdown.options.Add(new Dropdown.OptionData("F1"));
				vehicleDropdown.onValueChanged.AddListener(OnVehicleChanged);
			}
		}

		private void OnVehicleChanged(int idx)
		{
			if (spawner == null) return;
			switch (idx)
			{
				case 0: spawner.spec = bikeSpec; break;
				case 1: spawner.spec = motocrossSpec; break;
				case 2: spawner.spec = supercarSpec; break;
				case 3: spawner.spec = f1Spec; break;
			}
			if (spawner.spawned != null) Destroy(spawner.spawned);
			spawner.Spawn();
		}
	}
}