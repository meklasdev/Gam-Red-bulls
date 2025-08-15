using UnityEngine;

/// <summary>
/// Menedżer cyklu dnia/nocy. Obraca słońce i aktualizuje parametry oświetlenia.
/// </summary>
public class TimeOfDayManager : MonoBehaviour
{
	[Range(0f, 24f)] public float timeOfDay = 12f;
	[SerializeField] private bool autoCycle = true;
	[SerializeField] private float dayLengthMinutes = 10f;
	[SerializeField] private Light sunLight;
	[SerializeField] private Gradient sunColorByTime;
	[SerializeField] private AnimationCurve sunIntensityByTime = AnimationCurve.EaseInOut(0, 0, 24, 1);

	public delegate void TimeOfDayChangedHandler(float timeOfDay);
	public event TimeOfDayChangedHandler OnTimeOfDayChanged;

	private void Update()
	{
		if (autoCycle && dayLengthMinutes > 0f)
		{
			float daySeconds = dayLengthMinutes * 60f;
			timeOfDay = (timeOfDay + (24f / daySeconds) * Time.deltaTime) % 24f;
			ApplySettingsImmediate();
		}
	}

	public void ApplySettingsImmediate()
	{
		if (sunLight != null)
		{
			float t = timeOfDay / 24f;
			float angle = t * 360f - 90f;
			sunLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
			sunLight.color = sunColorByTime.Evaluate(t);
			sunLight.intensity = sunIntensityByTime.Evaluate(timeOfDay);
		}
		OnTimeOfDayChanged?.Invoke(timeOfDay);
	}
}