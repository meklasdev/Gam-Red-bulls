using UnityEngine;

namespace ExtremeRacing.Managers
{
	[System.Serializable]
	public class SaveData
	{
		public int currency = 0;
		public string selectedVehicleId = "Supercar_Basic";
		public string[] unlockedVehicles = new string[] { "Bike_Basic", "Motocross_Basic", "Supercar_Basic" };
		public string[] unlockedRegions = new string[] { GameManager.SceneGorski, GameManager.ScenePustynny };
	}

	public static class SaveSystem
	{
		private const string Key = "ExtremeRacing_Save";
		private static SaveData _cache;

		public static SaveData Data
		{
			get
			{
				if (_cache == null)
				{
					Load();
				}
				return _cache;
			}
		}

		public static void Load()
		{
			if (PlayerPrefs.HasKey(Key))
			{
				var json = PlayerPrefs.GetString(Key);
				_cache = JsonUtility.FromJson<SaveData>(json);
			}
			else
			{
				_cache = new SaveData();
				Save();
			}
		}

		public static void Save()
		{
			var json = JsonUtility.ToJson(_cache);
			PlayerPrefs.SetString(Key, json);
			PlayerPrefs.Save();
		}
	}
}