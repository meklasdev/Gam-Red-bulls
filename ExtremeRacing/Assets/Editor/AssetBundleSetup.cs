#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ExtremeRacing.Editor
{
	public static class AssetBundleSetup
	{
		[MenuItem("ExtremeRacing/Build/AssetBundles")]
		public static void BuildAssetBundles()
		{
			string dir = Path.Combine(Application.dataPath, "StreamingAssets/AssetBundles");
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			BuildPipeline.BuildAssetBundles("Assets/StreamingAssets/AssetBundles", BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
			Debug.Log("AssetBundles built to Assets/StreamingAssets/AssetBundles");
		}
	}
}
#endif