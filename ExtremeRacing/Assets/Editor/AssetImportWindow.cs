#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net;
using UnityEditor.PackageManager;

namespace ExtremeRacing.Editor
{
	public class AssetImportWindow : EditorWindow
	{
		private string _url = "";
		private string _packageName = "";
		private Vector2 _scroll;

		[MenuItem("ExtremeRacing/Assets/Import Assets (Window)")]
		public static void Open()
		{
			GetWindow<AssetImportWindow>(true, "Import Assets", true).minSize = new Vector2(480, 260);
		}

		private void OnGUI()
		{
			_scroll = EditorGUILayout.BeginScrollView(_scroll);
			GUILayout.Label("UnityPackage (.unitypackage)", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Import from file...", GUILayout.Width(180)))
			{
				ImportUnityPackageFromFile();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(8);

			GUILayout.Label("Download & Import from URL", EditorStyles.boldLabel);
			_url = EditorGUILayout.TextField("URL", _url);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Download & Import", GUILayout.Width(180)))
			{
				DownloadAndImport(_url);
			}
			EditorGUILayout.HelpBox("Supports .unitypackage, .fbx, .obj, .png, .jpg and other common assets.", MessageType.Info);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(12);
			GUILayout.Label("UPM Package (com.vendor.name)", EditorStyles.boldLabel);
			_packageName = EditorGUILayout.TextField("Package", _packageName);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Package", GUILayout.Width(180)))
			{
				AddUPMPackage(_packageName);
			}
			EditorGUILayout.HelpBox("Adds a Unity Package Manager dependency (e.g., com.unity.cinemachine)", MessageType.None);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();
		}

		private void ImportUnityPackageFromFile()
		{
			string path = EditorUtility.OpenFilePanel("Select UnityPackage", Application.dataPath, "unitypackage");
			if (string.IsNullOrEmpty(path)) return;
			AssetDatabase.ImportPackage(path, true);
		}

		private void DownloadAndImport(string url)
		{
			if (string.IsNullOrEmpty(url)) { EditorUtility.DisplayDialog("Import", "Please enter a URL.", "OK"); return; }
			string tempDir = Path.Combine(Path.GetTempPath(), "ExtremeRacing_Imports");
			if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
			string fileName = Path.GetFileName(new System.Uri(url).LocalPath);
			if (string.IsNullOrEmpty(fileName)) fileName = "download.bin";
			string tempPath = Path.Combine(tempDir, fileName);
			try
			{
				EditorUtility.DisplayProgressBar("Downloading", url, 0.2f);
				using (var wc = new WebClient())
				{
					wc.DownloadFile(url, tempPath);
				}
			}
			catch (System.Exception ex)
			{
				EditorUtility.ClearProgressBar();
				Debug.LogError($"Download failed: {ex.Message}");
				EditorUtility.DisplayDialog("Download failed", ex.Message, "OK");
				return;
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			string ext = Path.GetExtension(tempPath).ToLowerInvariant();
			if (ext == ".unitypackage")
			{
				AssetDatabase.ImportPackage(tempPath, true);
			}
			else
			{
				string targetDir = "Assets/Art/Imported";
				if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
				if (!AssetDatabase.IsValidFolder(targetDir)) AssetDatabase.CreateFolder("Assets/Art", "Imported");
				string targetPath = Path.Combine(targetDir, fileName).Replace('\\', '/');
				File.Copy(tempPath, targetPath, true);
				AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
				Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(targetPath);
				EditorGUIUtility.PingObject(Selection.activeObject);
				Debug.Log($"Imported asset to {targetPath}");
			}
		}

		private void AddUPMPackage(string packageId)
		{
			if (string.IsNullOrEmpty(packageId)) { EditorUtility.DisplayDialog("UPM", "Enter package name.", "OK"); return; }
			Client.Add(packageId);
			Debug.Log($"Adding UPM package: {packageId}");
		}
	}
}
#endif