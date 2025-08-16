using UnityEngine;

namespace ExtremeRacing.Managers
{
	public class TimeOfDayManager : MonoBehaviour
	{
		[Range(0f, 24f)] public float hour = 12f;
		public float dayLengthMinutes = 20f;
		public Light sun;
		public Gradient ambientColor;

		private void Update()
		{
			float deltaHours = (24f / (dayLengthMinutes * 60f)) * Time.deltaTime;
			hour = (hour + deltaHours) % 24f;

			if (sun != null)
			{
				float t = hour / 24f;
				sun.transform.rotation = Quaternion.Euler(new Vector3((t * 360f) - 90f, 170f, 0f));
				sun.intensity = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI));
			}

			RenderSettings.ambientLight = ambientColor.Evaluate(hour / 24f);
		}
	}
}