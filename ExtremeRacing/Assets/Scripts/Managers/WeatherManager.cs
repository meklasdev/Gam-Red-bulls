using UnityEngine;

namespace ExtremeRacing.Managers
{
	public enum WeatherType
	{
		Clear,
		Rain,
		Sandstorm
	}

	public class WeatherManager : MonoBehaviour
	{
		[SerializeField] private WeatherType _current = WeatherType.Clear;
		[SerializeField] private ParticleSystem _rainFx;
		[SerializeField] private ParticleSystem _sandFx;
		[SerializeField] private Light _sunLight;

		public WeatherType Current => _current;

		public void SetWeather(WeatherType type)
		{
			_current = type;
			if (_rainFx != null) _rainFx.gameObject.SetActive(type == WeatherType.Rain);
			if (_sandFx != null) _sandFx.gameObject.SetActive(type == WeatherType.Sandstorm);
			if (_sunLight != null)
			{
				_sunLight.color = type == WeatherType.Sandstorm ? new Color(1f, 0.85f, 0.6f) : Color.white;
			}
		}
	}
}