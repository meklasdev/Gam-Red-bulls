using UnityEngine;

namespace ExtremeRacing.Gameplay
{
	public class ActivityManager : MonoBehaviour
	{
		public static ActivityManager Instance { get; private set; }
		public ActivityZone Current { get; private set; }
		public float Elapsed { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		private void Update()
		{
			if (Current != null)
			{
				Elapsed += Time.deltaTime;
			}
		}

		public void Begin(ActivityZone zone)
		{
			Current = zone;
			Elapsed = 0f;
		}

		public void Finish(bool success)
		{
			var zone = Current;
			Current = null;
			Elapsed = 0f;
			FindObjectOfType<ResultsUI>()?.Show(zone != null ? zone.type.ToString() : "Activity", success, (int)Random.Range(100, 500));
		}
	}
}