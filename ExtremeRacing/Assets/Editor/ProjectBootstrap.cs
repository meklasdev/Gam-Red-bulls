#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace ExtremeRacing.Editor
{
	[InitializeOnLoad]
	public static class ProjectBootstrap
	{
		static ProjectBootstrap()
		{
			EditorApplication.update += TryBootstrap;
		}

		private static bool _done;
		private static void TryBootstrap()
		{
			if (_done) return;
			_done = true;

			SetupFolders();
			SetupURP();
			CreateScenes();
			CreatePrefabsAndAssets();
			AddBuildScenes();
		}

		private static void SetupFolders()
		{
			string[] folders = {
				"Assets/Scenes",
				"Assets/Prefabs",
				"Assets/ScriptableObjects",
				"Assets/Art",
				"Assets/Addressables",
				"Assets/Resources/NetworkPrefabs"
			};
			foreach (var f in folders)
			{
				if (!AssetDatabase.IsValidFolder(f))
				{
					var parent = Path.GetDirectoryName(f).Replace('\\','/');
					var leaf = Path.GetFileName(f);
					AssetDatabase.CreateFolder(parent, leaf);
				}
			}
		}

		private static void SetupURP()
		{
			string assetPath = "Assets/URP_Asset.asset";
			string rendererPath = "Assets/URP_ForwardRenderer.asset";
			var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
			if (urp == null)
			{
				urp = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
				AssetDatabase.CreateAsset(urp, assetPath);
			}
			var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
			if (renderer == null)
			{
				renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
				AssetDatabase.CreateAsset(renderer, rendererPath);
			}
			SerializedObject so = new SerializedObject(urp);
			var prop = so.FindProperty("m_RendererDataList");
			if (prop != null)
			{
				prop.arraySize = 1;
				prop.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
				so.ApplyModifiedPropertiesWithoutUndo();
			}
			GraphicsSettingsUtil.SetURPAsset(urp);
		}

		private static void CreateScenes()
		{
			string[] sceneNames = {
				"MainMenu",
				"Region_GorskiSzczyt",
				"Region_PustynnyKanion",
				"Region_MiastoNocy",
				"Region_PortWyscigowy",
				"Region_TorMistrzow"
			};
			foreach (var name in sceneNames)
			{
				string path = $"Assets/Scenes/{name}.unity";
				if (!File.Exists(path))
				{
					var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
					var lightGO = new GameObject("Directional Light");
					var light = lightGO.AddComponent<Light>();
					light.type = LightType.Directional;
					light.intensity = 1.0f;
					var camGO = new GameObject("Main Camera");
					var cam = camGO.AddComponent<Camera>();
					cam.tag = "MainCamera";
					EditorSceneManager.SaveScene(scene, path);
				}
			}
		}

		private static void CreatePrefabsAndAssets()
		{
			// GameSystems prefab
			string systemsPath = "Assets/Prefabs/GameSystems.prefab";
			if (!File.Exists(systemsPath))
			{
				var root = new GameObject("GameSystems");
				root.AddComponent<ExtremeRacing.Managers.GameManager>();
				root.AddComponent<ExtremeRacing.Managers.InputManager>();
				root.AddComponent<ExtremeRacing.Managers.TimeOfDayManager>();
				root.AddComponent<ExtremeRacing.Managers.WeatherManager>();
				root.AddComponent<ExtremeRacing.Gameplay.MissionSystem>();
				root.AddComponent<ExtremeRacing.Gameplay.ContractSystem>();
				root.AddComponent<ExtremeRacing.Gameplay.DriftScoring>();
				root.AddComponent<ExtremeRacing.Gameplay.LootSpawner>();
				PrefabUtility.SaveAsPrefabAsset(root, systemsPath);
				Object.DestroyImmediate(root);
			}

			// NetworkManager prefab
			string nmPath = "Assets/Prefabs/NetworkManager.prefab";
			if (!File.Exists(nmPath))
			{
				var go = new GameObject("NetworkManager");
				var nm = go.AddComponent<NetworkManager>();
				go.AddComponent<ExtremeRacing.Multiplayer.RuntimeNetworkBootstrap>();
				var utp = go.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
				nm.NetworkConfig = new NetworkConfig { EnableSceneManagement = true };
				PrefabUtility.SaveAsPrefabAsset(go, nmPath);
				Object.DestroyImmediate(go);
			}

			// HUD prefab
			string hudPath = "Assets/Prefabs/HUD.prefab";
			if (!File.Exists(hudPath))
			{
				var canvasGO = new GameObject("HUD");
				var canvas = canvasGO.AddComponent<UnityEngine.Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
				canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
				var textGO = new GameObject("SpeedText");
				textGO.transform.SetParent(canvasGO.transform);
				var txt = textGO.AddComponent<UnityEngine.UI.Text>();
				txt.text = "0 km/h";
				txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
				txt.alignment = TextAnchor.LowerRight;
				txt.rectTransform.anchorMin = new Vector2(1,0);
				txt.rectTransform.anchorMax = new Vector2(1,0);
				txt.rectTransform.anchoredPosition = new Vector2(-40, 40);
				canvasGO.AddComponent<ExtremeRacing.UI.HUDController>().speedText = txt;
				PrefabUtility.SaveAsPrefabAsset(canvasGO, hudPath);
				Object.DestroyImmediate(canvasGO);
			}

			// Vehicle specs
			CreateVehicleSpec("Bike_Basic", ExtremeRacing.Vehicles.VehicleType.Bike, 60, 12, 1.2f, 10, 35);
			CreateVehicleSpec("Motocross_Basic", ExtremeRacing.Vehicles.VehicleType.Motocross, 120, 18, 1.1f, 14, 35);
			CreateVehicleSpec("Supercar_Basic", ExtremeRacing.Vehicles.VehicleType.Supercar, 320, 28, 1.6f, 24, 28);
			CreateVehicleSpec("F1_Basic", ExtremeRacing.Vehicles.VehicleType.F1, 360, 35, 1.8f, 30, 20);
		}

		private static void CreateVehicleSpec(string name, ExtremeRacing.Vehicles.VehicleType type, float maxSpeed, float accel, float grip, float brake, float steer)
		{
			string path = $"Assets/ScriptableObjects/{name}.asset";
			if (File.Exists(path)) return;
			var spec = ScriptableObject.CreateInstance<ExtremeRacing.Vehicles.VehicleSpec>();
			spec.vehicleId = name;
			spec.type = type;
			spec.maxSpeedKmh = maxSpeed;
			spec.acceleration = accel;
			spec.grip = grip;
			spec.brakePower = brake;
			spec.steerAngle = steer;
			AssetDatabase.CreateAsset(spec, path);
		}

		private static void AddBuildScenes()
		{
			var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
			string[] sceneNames = {
				"MainMenu",
				"Region_GorskiSzczyt",
				"Region_PustynnyKanion",
				"Region_MiastoNocy",
				"Region_PortWyscigowy",
				"Region_TorMistrzow"
			};
			foreach (var name in sceneNames)
			{
				string p = $"Assets/Scenes/{name}.unity";
				if (File.Exists(p)) list.Add(new EditorBuildSettingsScene(p, true));
			}
			EditorBuildSettings.scenes = list.ToArray();
		}
	}

	public static class GraphicsSettingsUtil
	{
		public static void SetURPAsset(UniversalRenderPipelineAsset asset)
		{
			UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset = asset;
			QualitySettings.renderPipeline = asset;
		}
	}
}
#endif