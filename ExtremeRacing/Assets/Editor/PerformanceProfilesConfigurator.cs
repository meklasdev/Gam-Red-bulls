#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ExtremeRacing.Editor
{
	public static class PerformanceProfilesConfigurator
	{
		[MenuItem("ExtremeRacing/Configure/URP Mobile Quality (60 FPS)")]
		public static void Apply()
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;
			QualitySettings.shadowDistance = 50f;
			QualitySettings.lodBias = 1.2f;
			QualitySettings.particleRaycastBudget = 16;
			Debug.Log("Applied URP Mobile performance profile");
		}
	}
}
#endif