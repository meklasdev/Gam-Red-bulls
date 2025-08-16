#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ExtremeRacing.Editor
{
	public static class TextureCompressionTool
	{
		[MenuItem("ExtremeRacing/Optimize/Set Texture Compression (ASTC/ETC2)")]
		public static void SetCompression()
		{
			string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				var importer = AssetImporter.GetAtPath(path) as TextureImporter;
				if (importer == null) continue;
				var android = importer.GetPlatformTextureSettings("Android");
				android.overridden = true;
				android.format = TextureImporterFormat.ASTC_6x6;
				importer.SetPlatformTextureSettings(android);
				var ios = importer.GetPlatformTextureSettings("iPhone");
				ios.overridden = true;
				ios.format = TextureImporterFormat.ASTC_6x6;
				importer.SetPlatformTextureSettings(ios);
				EditorUtility.SetDirty(importer);
			}
			AssetDatabase.SaveAssets();
			Debug.Log("Texture compression set to ASTC for Android/iOS.");
		}
	}
}
#endif