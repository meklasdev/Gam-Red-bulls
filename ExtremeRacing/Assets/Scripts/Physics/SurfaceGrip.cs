using UnityEngine;

namespace ExtremeRacing.PhysicsEx
{
	public enum SurfaceType { Asphalt, Dirt, Sand, Grass }

	[CreateAssetMenu(menuName = "ExtremeRacing/Surface Grip", fileName = "SurfaceGrip")]
	public class SurfaceGrip : ScriptableObject
	{
		public SurfaceType type;
		[Range(0.2f, 2f)] public float gripMultiplier = 1f;
	}

	public class WeatherGripController : MonoBehaviour
	{
		public Managers.WeatherManager weather;
		[Range(0.2f, 2f)] public float rainMultiplier = 0.8f;
		[Range(0.2f, 2f)] public float sandstormMultiplier = 0.7f;

		public float GetWeatherGrip()
		{
			if (weather == null) return 1f;
			switch (weather.Current)
			{
				case Managers.WeatherType.Rain: return rainMultiplier;
				case Managers.WeatherType.Sandstorm: return sandstormMultiplier;
				default: return 1f;
			}
		}
	}
}