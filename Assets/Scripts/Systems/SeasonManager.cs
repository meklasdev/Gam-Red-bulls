using UnityEngine;

/// <summary>
/// Menedżer sezonów: ustawia presety pogody i czasu.
/// </summary>
public class SeasonManager : MonoBehaviour
{
	public enum Season { Wiosna, Lato, Jesien, Zima }

	[SerializeField] private Season currentSeason = Season.Lato;
	[SerializeField] private WeatherManager weatherManager;
	[SerializeField] private TimeOfDayManager timeOfDayManager;

	private void Awake()
	{
		if (weatherManager == null) weatherManager = FindObjectOfType<WeatherManager>();
		if (timeOfDayManager == null) timeOfDayManager = FindObjectOfType<TimeOfDayManager>();
		ApplySeason();
	}

	public void SetSeason(Season season)
	{
		currentSeason = season;
		ApplySeason();
	}

	public void ApplySeason()
	{
		switch (currentSeason)
		{
			case Season.Wiosna:
				weatherManager?.SetWeather(WeatherManager.WeatherType.Rain, 0.5f);
				if (timeOfDayManager != null) { timeOfDayManager.timeOfDay = 14f; timeOfDayManager.ApplySettingsImmediate(); }
				break;
			case Season.Lato:
				weatherManager?.SetWeather(WeatherManager.WeatherType.Clear, 0.1f);
				if (timeOfDayManager != null) { timeOfDayManager.timeOfDay = 21f; timeOfDayManager.ApplySettingsImmediate(); }
				break;
			case Season.Jesien:
				weatherManager?.SetWeather(WeatherManager.WeatherType.Sandstorm, 0.6f);
				if (timeOfDayManager != null) { timeOfDayManager.timeOfDay = 16f; timeOfDayManager.ApplySettingsImmediate(); }
				break;
			case Season.Zima:
				weatherManager?.SetWeather(WeatherManager.WeatherType.Snow, 0.7f);
				if (timeOfDayManager != null) { timeOfDayManager.timeOfDay = 10f; timeOfDayManager.ApplySettingsImmediate(); }
				break;
		}
	}
}