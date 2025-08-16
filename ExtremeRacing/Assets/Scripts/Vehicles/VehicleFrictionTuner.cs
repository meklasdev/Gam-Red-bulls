using UnityEngine;
using ExtremeRacing.PhysicsEx;

namespace ExtremeRacing.Vehicles
{
	public class VehicleFrictionTuner : MonoBehaviour
	{
		public WheelCollider[] wheels;
		public SurfaceGrip surfaceGrip;
		public WeatherGripController weatherGrip;
		[Range(0.5f, 2f)] public float baseStiffness = 1f;

		private void FixedUpdate()
		{
			float weatherMul = weatherGrip ? weatherGrip.GetWeatherGrip() : 1f;
			float surfMul = surfaceGrip ? surfaceGrip.gripMultiplier : 1f;
			float stiffness = baseStiffness * weatherMul * surfMul;
			foreach (var w in wheels)
			{
				if (w == null) continue;
				var f = w.forwardFriction;
				f.stiffness = stiffness;
				w.forwardFriction = f;
				var s = w.sidewaysFriction;
				s.stiffness = stiffness;
				w.sidewaysFriction = s;
			}
		}
	}
}