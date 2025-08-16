#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ExtremeRacing.Editor
{
	public static class MobileSettingsConfigurator
	{
		[MenuItem("ExtremeRacing/Configure/Mobile Player Settings")]
		public static void Configure()
		{
			PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
			PlayerSettings.allowedAutorotateToLandscapeLeft = true;
			PlayerSettings.allowedAutorotateToLandscapeRight = true;
			PlayerSettings.allowedAutorotateToPortrait = false;
			PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

			// Android
			PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.Android8_0Oreo;
			PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
			PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);

			// iOS
			PlayerSettings.iOS.targetOSVersionString = "13.0";
			PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, true);

			// Quality for mobile
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;

			Debug.Log("Mobile Player Settings configured (Android min 8.0, iOS 13.0, 60 FPS)");
		}
	}
}
#endif