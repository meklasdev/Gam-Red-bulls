#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Bootstrap edytora: tworzy katalogi, sceny i proste menu jeśli nie istnieją.
/// </summary>
[InitializeOnLoad]
public static class ProjectBootstrap
{
	static ProjectBootstrap()
	{
		EditorApplication.update += TryBootstrap;
	}

	private static void TryBootstrap()
	{
		EditorApplication.update -= TryBootstrap;
		EnsureDirectories();
		CreateScenesIfMissing();
		CreatePlaceholderVehicleAssets();
		CreateQualityPresetsIfMissing();
	}

	private static void EnsureDirectories()
	{
		string[] dirs = new[]
		{
			"Assets/Scenes",
			"Assets/Prefabs",
			"Assets/Materials",
			"Assets/ExternalAssets/Models",
			"Assets/ExternalAssets/Textures",
			"Assets/ExternalAssets/Sounds"
		};
		foreach (var d in dirs)
		{
			if (!Directory.Exists(d)) Directory.CreateDirectory(d);
		}
	}

	private static void CreateScenesIfMissing()
	{
		CreateMainMenuIfMissing();
		CreateRegionIfMissing(SceneNames.RegionGorskiSzczyt);
		CreateRegionIfMissing(SceneNames.RegionPustynnyKanion);
		CreateRegionIfMissing(SceneNames.RegionMiastoNocy);
		CreateRegionIfMissing(SceneNames.RegionPortWyscigowy);
		CreateRegionIfMissing(SceneNames.RegionTorMistrzow);
	}

	private static void CreateMainMenuIfMissing()
	{
		string path = "Assets/Scenes/" + SceneNames.MainMenu + ".unity";
		if (File.Exists(path)) return;
		var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

		var go = new GameObject("GameSystems");
		go.AddComponent<GameManager>();
		go.AddComponent<SceneLoader>();
		go.AddComponent<MobileOptimizationSettings>();
		var opt = go.AddComponent<OptimizationManager>();

		// UI Canvas
		var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuUI));
		var canvas = canvasGo.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		var scaler = canvasGo.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);

		CreateButton(canvasGo.transform, "Pełny tryb", new Vector2(0, 200));
		CreateButton(canvasGo.transform, "Kariera", new Vector2(0, 120));
		CreateButton(canvasGo.transform, "Sandbox", new Vector2(0, 40));
		CreateButton(canvasGo.transform, "Górski Szczyt", new Vector2(0, -60), () => GameManager.Instance.StartGameInRegion(SceneNames.RegionGorskiSzczyt));
		CreateButton(canvasGo.transform, "Pustynny Kanion", new Vector2(0, -140), () => GameManager.Instance.StartGameInRegion(SceneNames.RegionPustynnyKanion));
		CreateButton(canvasGo.transform, "Miasto Nocy", new Vector2(0, -220), () => GameManager.Instance.StartGameInRegion(SceneNames.RegionMiastoNocy));
		CreateButton(canvasGo.transform, "Port Wyścigowy", new Vector2(0, -300), () => GameManager.Instance.StartGameInRegion(SceneNames.RegionPortWyscigowy));
		CreateButton(canvasGo.transform, "Tor Mistrzów", new Vector2(0, -380), () => GameManager.Instance.StartGameInRegion(SceneNames.RegionTorMistrzow));

		// Overlay FPS
		var overlayGo = new GameObject("PerformanceOverlay", typeof(RectTransform), typeof(Text), typeof(PerformanceOverlay));
		overlayGo.transform.SetParent(canvasGo.transform, false);
		var rect = overlayGo.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(0f, 1f);
		rect.anchoredPosition = new Vector2(10f, -10f); rect.sizeDelta = new Vector2(320f, 70f);
		var txt = overlayGo.GetComponent<Text>();
		txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		txt.color = Color.white; txt.alignment = TextAnchor.UpperLeft; txt.raycastTarget = false;
		overlayGo.GetComponent<PerformanceOverlay>().GetType().GetField("optimizationManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(overlayGo.GetComponent<PerformanceOverlay>(), opt);

		EditorSceneManager.SaveScene(scene, path);
	}

	private static void CreateRegionIfMissing(string sceneName)
	{
		string path = $"Assets/Scenes/{sceneName}.unity";
		if (File.Exists(path)) return;
		var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

		var systems = new GameObject("GameSystems");
		systems.AddComponent<GameManager>();
		systems.AddComponent<InputManager>();
		systems.AddComponent<MobileOptimizationSettings>();
		systems.AddComponent<WeatherManager>();
		systems.AddComponent<TimeOfDayManager>();
		systems.AddComponent<MissionSystem>();
		systems.AddComponent<ContractSystem>();
		var opt = systems.AddComponent<OptimizationManager>();

		// Prosty teren placeholder
		var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
		ground.name = "Ground";
		ground.transform.localScale = new Vector3(10, 1, 10);

		// Kamera
		var cam = new GameObject("Main Camera", typeof(Camera), typeof(AdaptiveCulling));
		cam.GetComponent<Camera>().transform.position = new Vector3(0, 20, -20);
		cam.GetComponent<Camera>().transform.LookAt(Vector3.zero);
		cam.tag = "MainCamera";

		// Overlay FPS
		var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		var overlayGo = new GameObject("PerformanceOverlay", typeof(RectTransform), typeof(Text), typeof(PerformanceOverlay));
		overlayGo.transform.SetParent(canvasGo.transform, false);
		var rect = overlayGo.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(0f, 1f);
		rect.anchoredPosition = new Vector2(10f, -10f); rect.sizeDelta = new Vector2(320f, 70f);
		var txt = overlayGo.GetComponent<Text>(); txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); txt.color = Color.white; txt.alignment = TextAnchor.UpperLeft; txt.raycastTarget = false;
		overlayGo.GetComponent<PerformanceOverlay>().GetType().GetField("optimizationManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(overlayGo.GetComponent<PerformanceOverlay>(), opt);

		// Pojazd gracza placeholder
		var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
		player.name = "PlayerVehicle";
		player.AddComponent<Rigidbody>();
		player.AddComponent<VehicleController>();
		player.AddComponent<DriftScoring>();
		player.AddComponent<StuntManager>();

		EditorSceneManager.SaveScene(scene, path);
	}

	private static Button CreateButton(Transform parent, string text, Vector2 anchoredPos, System.Action onClick = null)
	{
		var btnGo = new GameObject(text, typeof(RectTransform), typeof(Button), typeof(Image));
		btnGo.transform.SetParent(parent, false);
		var rect = btnGo.GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(360, 60);
		rect.anchoredPosition = anchoredPos;
		var img = btnGo.GetComponent<Image>();
		img.color = new Color(0.9f, 0.1f, 0.1f, 0.9f);
		var btn = btnGo.GetComponent<Button>();

		var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
		textGo.transform.SetParent(btnGo.transform, false);
		var txt = textGo.GetComponent<Text>();
		txt.text = text;
		txt.alignment = TextAnchor.MiddleCenter;
		txt.color = Color.white;
		txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		var textRect = textGo.GetComponent<RectTransform>();
		textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
		textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;

		if (onClick != null) btn.onClick.AddListener(() => onClick());
		return btn;
	}

	private static void CreatePlaceholderVehicleAssets()
	{
		var statsPath = "Assets/Prefabs/VehicleStats_Default.asset";
		if (!File.Exists(statsPath))
		{
			var stats = ScriptableObject.CreateInstance<VehicleStats>();
			stats.maxSpeedKmh = 200f;
			stats.acceleration = 14f;
			stats.brakeForce = 22f;
			AssetDatabase.CreateAsset(stats, statsPath);
		}
		var defPath = "Assets/Prefabs/VehicleDefinition_Default.asset";
		if (!File.Exists(defPath))
		{
			var def = ScriptableObject.CreateInstance<VehicleDefinition>();
			def.vehicleId = "default_car";
			def.displayName = "Domyślny Samochód";
			def.cost = 0;
			def.stats = AssetDatabase.LoadAssetAtPath<VehicleStats>(statsPath);
			AssetDatabase.CreateAsset(def, defPath);
		}
	}

	private static void CreateQualityPresetsIfMissing()
	{
		string baseDir = "Assets/Prefabs";
		if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
		CreatePreset("Low", 60, 0.8f, 0.7f, 2, 2, false, false, 0, baseDir);
		CreatePreset("Medium", 60, 1.0f, 1.0f, 0, 1, true, true, 2, baseDir);
		CreatePreset("High", 60, 1.0f, 1.5f, 0, 0, true, true, 4, baseDir);
	}

	private static void CreatePreset(string name, int fps, float scale, float lodBias, int maxLod, int texQ, bool aniso, bool shadows, int msaa, string dir)
	{
		string path = $"{dir}/Quality_{name}.asset";
		if (File.Exists(path)) return;
		var p = ScriptableObject.CreateInstance<QualityPreset>();
		p.displayName = name; p.targetFrameRate = fps; p.renderScale = scale; p.lodBias = lodBias; p.maximumLODLevel = maxLod; p.textureQuality = texQ; p.anisotropicEnable = aniso; p.shadowsEnabled = shadows; p.msaaSamples = msaa;
		AssetDatabase.CreateAsset(p, path);
	}
}
#endif