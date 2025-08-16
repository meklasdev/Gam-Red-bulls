using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace ExtremeRacing.Editor
{
    public class SceneGenerator : EditorWindow
    {
        private bool _generateTerrain = true;
        private bool _addBasicLighting = true;
        private bool _createSpawnPoints = true;
        private bool _addWeatherSystem = true;

        [MenuItem("Red Bull Racing/Generate All Scenes")]
        public static void ShowWindow()
        {
            GetWindow<SceneGenerator>("Scene Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Red Bull Racing - Scene Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _generateTerrain = EditorGUILayout.Toggle("Generate Terrain", _generateTerrain);
            _addBasicLighting = EditorGUILayout.Toggle("Add Basic Lighting", _addBasicLighting);
            _createSpawnPoints = EditorGUILayout.Toggle("Create Spawn Points", _createSpawnPoints);
            _addWeatherSystem = EditorGUILayout.Toggle("Add Weather System", _addWeatherSystem);

            GUILayout.Space(20);

            if (GUILayout.Button("Generate All Scenes", GUILayout.Height(40)))
            {
                GenerateAllScenes();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Generate Individual Scenes"))
            {
                ShowIndividualSceneOptions();
            }
        }

        private void ShowIndividualSceneOptions()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("MainMenu"), false, () => GenerateMainMenu());
            menu.AddItem(new GUIContent("Region_GorskiSzczyt"), false, () => GenerateGorskiSzczyt());
            menu.AddItem(new GUIContent("Region_PustynnyKanion"), false, () => GeneratePustynnyKanion());
            menu.AddItem(new GUIContent("Region_MiastoNocy"), false, () => GenerateMiastoNocy());
            menu.AddItem(new GUIContent("Region_PortWyscigowy"), false, () => GeneratePortWyscigowy());
            menu.AddItem(new GUIContent("Region_TorMistrzow"), false, () => GenerateTorMistrzow());
            menu.ShowAsContext();
        }

        private void GenerateAllScenes()
        {
            EditorUtility.DisplayProgressBar("Generating Scenes", "Starting...", 0f);

            try
            {
                CreateScenesDirectory();
                
                GenerateMainMenu();
                EditorUtility.DisplayProgressBar("Generating Scenes", "MainMenu", 1f/6f);

                GenerateGorskiSzczyt();
                EditorUtility.DisplayProgressBar("Generating Scenes", "Górski Szczyt", 2f/6f);

                GeneratePustynnyKanion();
                EditorUtility.DisplayProgressBar("Generating Scenes", "Pustynny Kanion", 3f/6f);

                GenerateMiastoNocy();
                EditorUtility.DisplayProgressBar("Generating Scenes", "Miasto Nocy", 4f/6f);

                GeneratePortWyscigowy();
                EditorUtility.DisplayProgressBar("Generating Scenes", "Port Wyścigowy", 5f/6f);

                GenerateTorMistrzow();
                EditorUtility.DisplayProgressBar("Generating Scenes", "Tor Mistrzów", 6f/6f);

                Debug.Log("[SceneGenerator] All scenes generated successfully!");
                EditorUtility.DisplayDialog("Success", "All scenes have been generated successfully!", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void CreateScenesDirectory()
        {
            string scenesPath = "Assets/Scenes";
            if (!Directory.Exists(scenesPath))
            {
                Directory.CreateDirectory(scenesPath);
                AssetDatabase.Refresh();
            }
        }

        private void GenerateMainMenu()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Camera dla menu
            GameObject camera = new GameObject("Main Camera");
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 1, -10);

            // Canvas UI
            CreateMainMenuUI();

            // Game Managers
            CreateGameManagers();

            // Background environment
            if (_addBasicLighting)
            {
                CreateBasicLighting();
            }

            SaveScene("Assets/Scenes/MainMenu.unity");
        }

        private void GenerateGorskiSzczyt()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera dla gracza
            GameObject camera = CreatePlayerCamera();

            // Terrain górski
            if (_generateTerrain)
            {
                CreateMountainTerrain();
            }

            // Spawn points
            if (_createSpawnPoints)
            {
                CreateMountainSpawnPoints();
            }

            // Weather system
            if (_addWeatherSystem)
            {
                CreateWeatherSystem("Mountain");
            }

            // Region-specific objects
            CreateMountainEnvironment();

            SaveScene("Assets/Scenes/Region_GorskiSzczyt.unity");
        }

        private void GeneratePustynnyKanion()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camera = CreatePlayerCamera();

            if (_generateTerrain)
            {
                CreateDesertTerrain();
            }

            if (_createSpawnPoints)
            {
                CreateDesertSpawnPoints();
            }

            if (_addWeatherSystem)
            {
                CreateWeatherSystem("Desert");
            }

            CreateDesertEnvironment();

            SaveScene("Assets/Scenes/Region_PustynnyKanion.unity");
        }

        private void GenerateMiastoNocy()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camera = CreatePlayerCamera();

            if (_generateTerrain)
            {
                CreateCityTerrain();
            }

            if (_createSpawnPoints)
            {
                CreateCitySpawnPoints();
            }

            if (_addWeatherSystem)
            {
                CreateWeatherSystem("City");
            }

            CreateCityEnvironment();

            SaveScene("Assets/Scenes/Region_MiastoNocy.unity");
        }

        private void GeneratePortWyscigowy()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camera = CreatePlayerCamera();

            if (_generateTerrain)
            {
                CreatePortTerrain();
            }

            if (_createSpawnPoints)
            {
                CreatePortSpawnPoints();
            }

            if (_addWeatherSystem)
            {
                CreateWeatherSystem("Port");
            }

            CreatePortEnvironment();

            SaveScene("Assets/Scenes/Region_PortWyscigowy.unity");
        }

        private void GenerateTorMistrzow()
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camera = CreatePlayerCamera();

            if (_generateTerrain)
            {
                CreateF1TrackTerrain();
            }

            if (_createSpawnPoints)
            {
                CreateF1SpawnPoints();
            }

            if (_addWeatherSystem)
            {
                CreateWeatherSystem("Track");
            }

            CreateF1Environment();

            SaveScene("Assets/Scenes/Region_TorMistrzow.unity");
        }

        private GameObject CreatePlayerCamera()
        {
            GameObject camera = new GameObject("Player Camera");
            camera.tag = "MainCamera";
            
            Camera cam = camera.AddComponent<Camera>();
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 2000f;
            
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 5, -10);
            
            return camera;
        }

        private void CreateMainMenuUI()
        {
            GameObject canvas = new GameObject("Canvas");
            Canvas canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Event System
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Title
            GameObject title = new GameObject("Title");
            title.transform.SetParent(canvas.transform);
            var titleText = title.AddComponent<UnityEngine.UI.Text>();
            titleText.text = "RED BULL EXTREME RACING";
            titleText.fontSize = 48;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.7f);
            titleRect.anchorMax = new Vector2(1, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // Buttons container
            CreateMenuButtons(canvas);
        }

        private void CreateMenuButtons(GameObject canvas)
        {
            string[] buttonNames = { "Kariera", "Pełny Tryb", "Sandbox", "Multiplayer", "Ustawienia", "Wyjście" };
            
            for (int i = 0; i < buttonNames.Length; i++)
            {
                GameObject button = new GameObject($"Button_{buttonNames[i]}");
                button.transform.SetParent(canvas.transform);

                var buttonComp = button.AddComponent<UnityEngine.UI.Button>();
                var buttonImage = button.AddComponent<UnityEngine.UI.Image>();
                buttonImage.color = new Color(0.2f, 0.6f, 1f, 0.8f);

                GameObject buttonText = new GameObject("Text");
                buttonText.transform.SetParent(button.transform);
                var text = buttonText.AddComponent<UnityEngine.UI.Text>();
                text.text = buttonNames[i];
                text.fontSize = 24;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.white;

                RectTransform buttonRect = button.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.3f, 0.6f - i * 0.08f);
                buttonRect.anchorMax = new Vector2(0.7f, 0.65f - i * 0.08f);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;

                RectTransform textRect = buttonText.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
        }

        private void CreateGameManagers()
        {
            GameObject managers = new GameObject("GameManagers");
            
            // Dodaj wszystkie potrzebne managery
            managers.AddComponent<ExtremeRacing.Managers.GameManager>();
            managers.AddComponent<ExtremeRacing.Managers.InputManager>();
            managers.AddComponent<ExtremeRacing.Addressables.RegionStreamingManager>();
        }

        private void CreateBasicLighting()
        {
            // Directional Light (słońce)
            GameObject sun = new GameObject("Directional Light");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = Color.white;
            sunLight.intensity = 1f;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);
        }

        private void CreateMountainTerrain()
        {
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = 513;
            terrainData.size = new Vector3(2000, 600, 2000);

            // Generuj górzysty teren
            float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                for (int y = 0; y < terrainData.heightmapResolution; y++)
                {
                    float xCoord = (float)x / terrainData.heightmapResolution * 5;
                    float yCoord = (float)y / terrainData.heightmapResolution * 5;
                    heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord) * 0.8f;
                }
            }
            terrainData.SetHeights(0, 0, heights);

            GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
            terrain.name = "Mountain Terrain";
        }

        private void CreateDesertTerrain()
        {
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = 513;
            terrainData.size = new Vector3(3000, 200, 3000);

            // Generuj pustynny teren z kanionami
            float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                for (int y = 0; y < terrainData.heightmapResolution; y++)
                {
                    float xCoord = (float)x / terrainData.heightmapResolution * 3;
                    float yCoord = (float)y / terrainData.heightmapResolution * 3;
                    
                    float noise1 = Mathf.PerlinNoise(xCoord, yCoord) * 0.3f;
                    float noise2 = Mathf.PerlinNoise(xCoord * 2, yCoord * 2) * 0.1f;
                    heights[x, y] = noise1 + noise2;
                }
            }
            terrainData.SetHeights(0, 0, heights);

            GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
            terrain.name = "Desert Terrain";
        }

        private void CreateCityTerrain()
        {
            // Płaski teren miejski
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = 257;
            terrainData.size = new Vector3(1500, 50, 1500);

            GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
            terrain.name = "City Terrain";
        }

        private void CreatePortTerrain()
        {
            // Teren portowy z niewielkimi wzniesieniami
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = 257;
            terrainData.size = new Vector3(1200, 30, 1200);

            GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
            terrain.name = "Port Terrain";
        }

        private void CreateF1TrackTerrain()
        {
            // Płaski teren dla toru F1
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = 257;
            terrainData.size = new Vector3(2000, 20, 2000);

            GameObject terrain = Terrain.CreateTerrainGameObject(terrainData);
            terrain.name = "F1 Track Terrain";
        }

        private void CreateMountainSpawnPoints()
        {
            CreateSpawnPoint("Downhill_Start", new Vector3(0, 550, 0));
            CreateSpawnPoint("Motocross_Start", new Vector3(200, 500, 100));
            CreateSpawnPoint("Endurance_Start", new Vector3(-150, 480, -200));
        }

        private void CreateDesertSpawnPoints()
        {
            CreateSpawnPoint("Rally_Start", new Vector3(100, 50, 0));
            CreateSpawnPoint("Drift_Arena", new Vector3(-200, 30, 300));
            CreateSpawnPoint("Canyon_Jump", new Vector3(0, 80, -400));
        }

        private void CreateCitySpawnPoints()
        {
            CreateSpawnPoint("Street_Start", new Vector3(0, 5, 0));
            CreateSpawnPoint("Drift_Contest", new Vector3(300, 5, 200));
            CreateSpawnPoint("Rooftop_Entry", new Vector3(-200, 25, -100));
        }

        private void CreatePortSpawnPoints()
        {
            CreateSpawnPoint("Gokart_Track", new Vector3(0, 5, 0));
            CreateSpawnPoint("Container_Rally", new Vector3(200, 5, 150));
            CreateSpawnPoint("Harbor_Sprint", new Vector3(-150, 5, -200));
        }

        private void CreateF1SpawnPoints()
        {
            CreateSpawnPoint("Grid_Position_1", new Vector3(0, 5, 0));
            CreateSpawnPoint("Grid_Position_2", new Vector3(4, 5, 0));
            CreateSpawnPoint("Pit_Lane", new Vector3(100, 5, 200));
        }

        private void CreateSpawnPoint(string name, Vector3 position)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{name}");
            spawnPoint.transform.position = position;
            spawnPoint.tag = "Respawn";

            // Dodaj komponent oznaczający spawn point
            var marker = spawnPoint.AddComponent<BoxCollider>();
            marker.isTrigger = true;
            marker.size = new Vector3(5, 2, 5);

            // Wizualny marker
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.transform.SetParent(spawnPoint.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(3, 0.1f, 3);
            visual.GetComponent<Renderer>().material.color = Color.green;
            DestroyImmediate(visual.GetComponent<Collider>());
        }

        private void CreateWeatherSystem(string type)
        {
            GameObject weatherSystem = new GameObject($"WeatherSystem_{type}");
            weatherSystem.AddComponent<ExtremeRacing.Managers.WeatherManager>();
            weatherSystem.AddComponent<ExtremeRacing.Managers.TimeOfDayManager>();
        }

        private void CreateMountainEnvironment()
        {
            // Tworzenie podstawowych obiektów środowiska górskiego
            CreateEnvironmentGroup("Trees", 20, new Vector3(1000, 0, 1000));
            CreateEnvironmentGroup("Rocks", 15, new Vector3(1000, 0, 1000));
            CreateEnvironmentGroup("Bridges", 3, new Vector3(500, 0, 500));
        }

        private void CreateDesertEnvironment()
        {
            CreateEnvironmentGroup("Cacti", 25, new Vector3(1500, 0, 1500));
            CreateEnvironmentGroup("Rocks", 20, new Vector3(1500, 0, 1500));
            CreateEnvironmentGroup("Oasis", 2, new Vector3(1000, 0, 1000));
        }

        private void CreateCityEnvironment()
        {
            CreateEnvironmentGroup("Buildings", 30, new Vector3(750, 0, 750));
            CreateEnvironmentGroup("Streets", 10, new Vector3(500, 0, 500));
            CreateEnvironmentGroup("Ramps", 8, new Vector3(400, 0, 400));
        }

        private void CreatePortEnvironment()
        {
            CreateEnvironmentGroup("Containers", 25, new Vector3(600, 0, 600));
            CreateEnvironmentGroup("Cranes", 5, new Vector3(400, 0, 400));
            CreateEnvironmentGroup("Ships", 3, new Vector3(300, 0, 300));
        }

        private void CreateF1Environment()
        {
            CreateEnvironmentGroup("Grandstands", 8, new Vector3(1000, 0, 1000));
            CreateEnvironmentGroup("Barriers", 50, new Vector3(800, 0, 800));
            CreateEnvironmentGroup("Pit_Buildings", 4, new Vector3(200, 0, 200));
        }

        private void CreateEnvironmentGroup(string groupName, int count, Vector3 area)
        {
            GameObject parent = new GameObject($"Environment_{groupName}");
            
            for (int i = 0; i < count; i++)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.name = $"{groupName}_{i:00}";
                obj.transform.SetParent(parent.transform);
                
                Vector3 randomPos = new Vector3(
                    Random.Range(-area.x/2, area.x/2),
                    Random.Range(0, area.y/2),
                    Random.Range(-area.z/2, area.z/2)
                );
                obj.transform.position = randomPos;
                
                // Randomizuj wielkość i rotację
                obj.transform.localScale = Vector3.one * Random.Range(0.5f, 2f);
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                
                // Ustaw kolor według typu
                Color color = GetEnvironmentColor(groupName);
                obj.GetComponent<Renderer>().material.color = color;
            }
        }

        private Color GetEnvironmentColor(string type)
        {
            switch (type)
            {
                case "Trees": return Color.green;
                case "Rocks": return Color.gray;
                case "Buildings": return Color.cyan;
                case "Containers": return Color.yellow;
                case "Cacti": return new Color(0, 0.8f, 0);
                case "Barriers": return Color.red;
                default: return Color.white;
            }
        }

        private void SaveScene(string path)
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
            Debug.Log($"[SceneGenerator] Scene saved: {path}");
        }
    }
}