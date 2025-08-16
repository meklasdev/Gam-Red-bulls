using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRacing.Gameplay
{
	public class ProgressionSystem : MonoBehaviour
	{
		public static ProgressionSystem Instance { get; private set; }
		private HashSet<string> _vehicles;
		private HashSet<string> _regions;

		private void Awake()
		{
			Instance = this;
			_vehicles = new HashSet<string>(ExtremeRacing.Managers.SaveSystem.Data.unlockedVehicles);
			_regions = new HashSet<string>(ExtremeRacing.Managers.SaveSystem.Data.unlockedRegions);
		}

		public bool IsVehicleUnlocked(string id) => _vehicles.Contains(id);
		public bool IsRegionUnlocked(string id) => _regions.Contains(id);

		public void UnlockVehicle(string id)
		{
			if (_vehicles.Add(id))
			{
				var data = ExtremeRacing.Managers.SaveSystem.Data;
				data.unlockedVehicles = new List<string>(_vehicles).ToArray();
				ExtremeRacing.Managers.SaveSystem.Save();
			}
		}

		public void UnlockRegion(string id)
		{
			if (_regions.Add(id))
			{
				var data = ExtremeRacing.Managers.SaveSystem.Data;
				data.unlockedRegions = new List<string>(_regions).ToArray();
				ExtremeRacing.Managers.SaveSystem.Save();
			}
		}

		public void SelectVehicle(string id)
		{
			ExtremeRacing.Managers.SaveSystem.Data.selectedVehicleId = id;
			ExtremeRacing.Managers.SaveSystem.Save();
		}

		public string GetSelectedVehicleId() => ExtremeRacing.Managers.SaveSystem.Data.selectedVehicleId;
	}
}