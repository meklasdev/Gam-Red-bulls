#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ExtremeRacing.Editor
{
	public static class BuildSettingsConfigurator
	{
		[MenuItem("ExtremeRacing/Configure/Build Settings (IL2CPP/ARM64)")]
		public static void Apply()
		{
			// Bundle IDs (placeholder)
			PlayerSettings.applicationIdentifier = "com.yourstudio.extremeracing";
			PlayerSettings.Android.bundleVersionCode = 1;
			PlayerSettings.iOS.buildNumber = "1";

			// Scripting backend and arch
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
			PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);

			// Cloud Diagnostics (enable Crash Reporting)
			PlayerSettings.enableCrashReportAPI = true;

			Debug.Log("Build settings configured: IL2CPP/ARM64, bundle IDs, Cloud Diagnostics.");
		}
	}
}
#endif