using UnityEngine;

/// <summary>
/// Menedżer pogody: zarządza efektami, wpływem na fizykę (przyczepność) i ustawieniami środowiska.
/// </summary>
public class WeatherManager : MonoBehaviour
{
	public enum WeatherType { Clear, Rain, Snow, Sandstorm, Storm }

	[Header("Bieżąca pogoda")]
	[SerializeField] private WeatherType currentWeather = WeatherType.Clear;
	[Range(0f, 1f)] [SerializeField] private float intensity = 0.5f;

	[Header("Efekty opcjonalne")]
	[SerializeField] private ParticleSystem rainFx;
	[SerializeField] private ParticleSystem snowFx;
	[SerializeField] private ParticleSystem sandstormFx;
	[SerializeField] private AudioSource windAudio;

	public float TractionMultiplier { get; private set; } = 1f;

	public delegate void WeatherChangedHandler(WeatherType type, float intensity);
	public event WeatherChangedHandler OnWeatherChanged;

	public void SetWeather(WeatherType type, float newIntensity)
	{
		currentWeather = type;
		intensity = Mathf.Clamp01(newIntensity);
		ApplyCurrentWeatherImmediate();
	}

	public void ApplyCurrentWeatherImmediate()
	{
		UpdateTraction();
		UpdateVisuals();
		OnWeatherChanged?.Invoke(currentWeather, intensity);
	}

	private void UpdateTraction()
	{
		switch (currentWeather)
		{
			case WeatherType.Clear:
				TractionMultiplier = 1f;
				break;
			case WeatherType.Rain:
				TractionMultiplier = Mathf.Lerp(0.9f, 0.8f, intensity);
				break;
			case WeatherType.Snow:
				TractionMultiplier = Mathf.Lerp(0.85f, 0.75f, intensity);
				break;
			case WeatherType.Sandstorm:
				TractionMultiplier = Mathf.Lerp(0.9f, 0.7f, intensity);
				break;
			case WeatherType.Storm:
				TractionMultiplier = Mathf.Lerp(0.85f, 0.7f, intensity);
				break;
		}
	}

	private void UpdateVisuals()
	{
		if (rainFx != null)
		{
			if (currentWeather == WeatherType.Rain || currentWeather == WeatherType.Storm)
			{
				if (!rainFx.isPlaying) rainFx.Play();
			}
			else if (rainFx.isPlaying) rainFx.Stop();
		}

		if (snowFx != null)
		{
			if (currentWeather == WeatherType.Snow)
			{
				if (!snowFx.isPlaying) snowFx.Play();
			}
			else if (snowFx.isPlaying) snowFx.Stop();
		}

		if (sandstormFx != null)
		{
			if (currentWeather == WeatherType.Sandstorm)
			{
				if (!sandstormFx.isPlaying) sandstormFx.Play();
			}
			else if (sandstormFx.isPlaying) sandstormFx.Stop();
		}

		if (windAudio != null)
		{
			float targetVolume = (currentWeather == WeatherType.Sandstorm || currentWeather == WeatherType.Storm) ? Mathf.Lerp(0.2f, 0.8f, intensity) : 0.05f;
			windAudio.volume = targetVolume;
		}

		RenderSettings.fog = currentWeather != WeatherType.Clear;
		RenderSettings.fogDensity = Mathf.Lerp(0.002f, 0.02f, intensity);
	}
}