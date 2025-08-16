using UnityEngine;
using ExtremeRacing.Procedural;

namespace ExtremeRacing.Gameplay
{
	public class SurvivalModeManager : MonoBehaviour
	{
		public ProceduralTrackGenerator generator;
		public float fuel = 100f;
		public float fuelConsumptionPerKm = 2f;
		private Vector3 _lastPos;
		private float _distanceKm;

		private void Start()
		{
			_lastPos = transform.position;
			if (generator != null)
			{
				generator.Generate();
			}
		}

		private void Update()
		{
			float meters = Vector3.Distance(transform.position, _lastPos);
			_lastPos = transform.position;
			_distanceKm += meters / 1000f;
			fuel -= (meters / 1000f) * fuelConsumptionPerKm;
			if (fuel <= 0f)
			{
				fuel = 0f;
				Debug.Log("Survival: Out of fuel!");
				enabled = false;
			}
		}
	}
}