#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ExtremeRacing.Editor
{
	public static class RegionAssetBundler
	{
		[MenuItem("ExtremeRacing/Build/Label Regions For AssetBundles")]
		public static void LabelRegions()
		{
			string scenesDir = "Assets/Scenes";
			foreach (var file in Directory.GetFiles(scenesDir, "*.unity"))
			{
				var importer = AssetImporter.GetAtPath(file);
				if (importer != null)
				{
					string name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
					if (name.StartsWith("region_"))
					{
						importer.SetAssetBundleNameAndVariant(name, "");
					}
				}
			}
			AssetDatabase.SaveAssets();
			Debug.Log("Region scenes labeled for AssetBundles");
		}
	}
}
#endif