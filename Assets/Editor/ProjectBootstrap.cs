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

		// Prosty teren placeholder
		var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
		ground.name = "Ground";
		ground.transform.localScale = new Vector3(10, 1, 10);

		// Kamera
		var cam = new GameObject("Main Camera", typeof(Camera));
		cam.GetComponent<Camera>().transform.position = new Vector3(0, 20, -20);
		cam.GetComponent<Camera>().transform.LookAt(Vector3.zero);
		cam.tag = "MainCamera";

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
}
#endif