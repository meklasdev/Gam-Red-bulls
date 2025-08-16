#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace ExtremeRacing.Editor
{
	public static class SceneAutoPopulate
	{
		[MenuItem("ExtremeRacing/Scenes/Populate All Regions")]
		public static void PopulateAll()
		{
			string[] names = {
				"Region_GorskiSzczyt",
				"Region_PustynnyKanion",
				"Region_MiastoNocy",
				"Region_PortWyscigowy",
				"Region_TorMistrzow"
			};
			foreach (var n in names) Populate(n);
		}

		[MenuItem("ExtremeRacing/Scenes/Populate Current Region")]
		public static void PopulateCurrent()
		{
			var scene = EditorSceneManager.GetActiveScene();
			if (scene.IsValid()) Populate(Path.GetFileNameWithoutExtension(scene.path));
		}

		private static void Populate(string sceneName)
		{
			string path = $"Assets/Scenes/{sceneName}.unity";
			if (!File.Exists(path)) return;
			var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
			if (scene.IsValid())
			{
				if (GameObject.FindObjectOfType<UnityEngine.Terrain>() == null)
				{
					var terrainGO = new GameObject("Terrain_Placeholder");
					var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
					plane.name = "Ground";
					plane.transform.localScale = new Vector3(50, 1, 50);
				}

				var spawn = GameObject.Find("SpawnPoint") ?? new GameObject("SpawnPoint");
				spawn.transform.position = new Vector3(0, 1, 0);

				string systemsPath = "Assets/Prefabs/GameSystems.prefab";
				var systems = AssetDatabase.LoadAssetAtPath<GameObject>(systemsPath);
				if (systems != null && GameObject.Find("GameSystems") == null)
				{
					PrefabUtility.InstantiatePrefab(systems);
				}

				EditorSceneManager.SaveScene(scene);
			}
		}
	}
}
#endif