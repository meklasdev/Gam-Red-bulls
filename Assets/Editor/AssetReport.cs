#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetReport
{
	[MenuItem("Tools/Generate Asset List")]
	public static void Generate()
	{
		string[] guids = AssetDatabase.FindAssets("");
		using (var sw = new StreamWriter(Path.Combine(Application.dataPath, "../ASSETS_LIST.md")))
		{
			sw.WriteLine("# Lista zasobów");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				sw.WriteLine("- " + path);
			}
		}
		Debug.Log("Wygenerowano ASSETS_LIST.md");
	}
}
#endif