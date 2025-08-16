using UnityEngine;
using UnityEditor;
using System.IO;
using ExtremeRacing.Vehicles;
using ExtremeRacing.Gameplay;

namespace ExtremeRacing.Editor
{
    public class PrefabGenerator : EditorWindow
    {
        private bool _generateVehicles = true;
        private bool _generateUI = true;
        private bool _generateEnvironment = true;
        private bool _generateGameplay = true;

        [MenuItem("Red Bull Racing/Generate All Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<PrefabGenerator>("Prefab Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Red Bull Racing - Prefab Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _generateVehicles = EditorGUILayout.Toggle("Generate Vehicles", _generateVehicles);
            _generateUI = EditorGUILayout.Toggle("Generate UI Prefabs", _generateUI);
            _generateEnvironment = EditorGUILayout.Toggle("Generate Environment", _generateEnvironment);
            _generateGameplay = EditorGUILayout.Toggle("Generate Gameplay Objects", _generateGameplay);

            GUILayout.Space(20);

            if (GUILayout.Button("Generate All Prefabs", GUILayout.Height(40)))
            {
                GenerateAllPrefabs();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Generate Individual Categories"))
            {
                ShowCategoryOptions();
            }
        }

        private void ShowCategoryOptions()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Vehicles"), false, () => GenerateVehiclePrefabs());
            menu.AddItem(new GUIContent("UI Elements"), false, () => GenerateUIPrefabs());
            menu.AddItem(new GUIContent("Environment"), false, () => GenerateEnvironmentPrefabs());
            menu.AddItem(new GUIContent("Gameplay Objects"), false, () => GenerateGameplayPrefabs());
            menu.ShowAsContext();
        }

        private void GenerateAllPrefabs()
        {
            EditorUtility.DisplayProgressBar("Generating Prefabs", "Starting...", 0f);

            try
            {
                CreatePrefabDirectories();

                if (_generateVehicles)
                {
                    GenerateVehiclePrefabs();
                    EditorUtility.DisplayProgressBar("Generating Prefabs", "Vehicles", 0.25f);
                }

                if (_generateUI)
                {
                    GenerateUIPrefabs();
                    EditorUtility.DisplayProgressBar("Generating Prefabs", "UI", 0.5f);
                }

                if (_generateEnvironment)
                {
                    GenerateEnvironmentPrefabs();
                    EditorUtility.DisplayProgressBar("Generating Prefabs", "Environment", 0.75f);
                }

                if (_generateGameplay)
                {
                    GenerateGameplayPrefabs();
                    EditorUtility.DisplayProgressBar("Generating Prefabs", "Gameplay", 1f);
                }

                Debug.Log("[PrefabGenerator] All prefabs generated successfully!");
                EditorUtility.DisplayDialog("Success", "All prefabs have been generated successfully!", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }

        private void CreatePrefabDirectories()
        {
            string[] directories = {
                "Assets/Prefabs",
                "Assets/Prefabs/Vehicles",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/Environment",
                "Assets/Prefabs/Gameplay"
            };

            foreach (string dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        #region Vehicle Prefabs

        private void GenerateVehiclePrefabs()
        {
            // Supercary
            CreateSupercar("Lamborghini_Prototype", Color.yellow);
            CreateSupercar("Ferrari_Prototype", Color.red);
            CreateSupercar("McLaren_Prototype", new Color(1f, 0.5f, 0f)); // Orange

            // F1 Cars
            CreateF1Car("F1_RedBull_Prototype", new Color(0.2f, 0.3f, 0.8f));
            CreateF1Car("F1_Mercedes_Prototype", Color.cyan);
            CreateF1Car("F1_Ferrari_Prototype", Color.red);

            // Rally Cars
            CreateRallyCar("Subaru_WRX_Prototype", Color.blue);
            CreateRallyCar("Ford_Focus_RS_Prototype", Color.green);

            // Motocykle
            CreateMotorcycle("Dirt_Bike_Prototype", Color.red);
            CreateMotorcycle("Sport_Bike_Prototype", Color.black);

            // Rowery
            CreateBike("Mountain_Bike_Prototype", Color.green);
            CreateBike("BMX_Prototype", Color.yellow);

            // Gokarty
            CreateGokart("Gokart_Prototype", Color.red);

            Debug.Log("[PrefabGenerator] Vehicle prefabs generated");
        }

        private void CreateSupercar(string name, Color color)
        {
            GameObject car = new GameObject(name);
            
            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(car.transform);
            body.transform.localScale = new Vector3(2f, 0.8f, 4.5f);
            body.GetComponent<Renderer>().material.color = color;
            DestroyImmediate(body.GetComponent<Collider>());

            // Add vehicle components
            var rigidbody = car.AddComponent<Rigidbody>();
            rigidbody.mass = 1500f;
            rigidbody.centerOfMass = new Vector3(0, -0.5f, 0);

            var controller = car.AddComponent<VehicleController>();
            
            // Create VehicleSpec
            var spec = CreateVehicleSpec($"{name}_Spec", VehicleType.Supercar, 300f, 25f, 8f, 1.2f);
            controller.spec = spec;
            controller.controlType = VehicleType.Supercar;

            // Wheels
            CreateWheels(car, controller, 1.8f, 4f);

            // Collider
            var collider = car.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 0.8f, 4.5f);

            SavePrefab(car, $"Assets/Prefabs/Vehicles/{name}.prefab");
        }

        private void CreateF1Car(string name, Color color)
        {
            GameObject car = new GameObject(name);
            
            // F1 Body - niższy i bardziej aerodynamiczny
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(car.transform);
            body.transform.localScale = new Vector3(1.8f, 0.5f, 5f);
            body.GetComponent<Renderer>().material.color = color;
            DestroyImmediate(body.GetComponent<Collider>());

            // Front wing
            GameObject frontWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontWing.transform.SetParent(car.transform);
            frontWing.transform.localPosition = new Vector3(0, -0.2f, 2.8f);
            frontWing.transform.localScale = new Vector3(2.2f, 0.1f, 0.3f);
            frontWing.GetComponent<Renderer>().material.color = Color.black;
            DestroyImmediate(frontWing.GetComponent<Collider>());

            // Rear wing
            GameObject rearWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearWing.transform.SetParent(car.transform);
            rearWing.transform.localPosition = new Vector3(0, 0.8f, -2.8f);
            rearWing.transform.localScale = new Vector3(1.5f, 0.1f, 0.3f);
            rearWing.GetComponent<Renderer>().material.color = Color.black;
            DestroyImmediate(rearWing.GetComponent<Collider>());

            var rigidbody = car.AddComponent<Rigidbody>();
            rigidbody.mass = 800f; // Lżejszy niż supercar
            rigidbody.centerOfMass = new Vector3(0, -0.4f, 0);

            var controller = car.AddComponent<VehicleController>();
            var spec = CreateVehicleSpec($"{name}_Spec", VehicleType.F1, 350f, 35f, 12f, 0.8f);
            controller.spec = spec;
            controller.controlType = VehicleType.F1;

            CreateWheels(car, controller, 1.6f, 4.5f);

            // F1 collider
            var collider = car.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.8f, 0.5f, 5f);

            SavePrefab(car, $"Assets/Prefabs/Vehicles/{name}.prefab");
        }

        private void CreateRallyCar(string name, Color color)
        {
            GameObject car = new GameObject(name);
            
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(car.transform);
            body.transform.localScale = new Vector3(1.8f, 1.2f, 4f);
            body.GetComponent<Renderer>().material.color = color;
            DestroyImmediate(body.GetComponent<Collider>());

            var rigidbody = car.AddComponent<Rigidbody>();
            rigidbody.mass = 1200f;
            rigidbody.centerOfMass = new Vector3(0, -0.3f, 0);

            var controller = car.AddComponent<VehicleController>();
            var spec = CreateVehicleSpec($"{name}_Spec", VehicleType.Rally, 220f, 20f, 6f, 1.5f);
            controller.spec = spec;
            controller.controlType = VehicleType.Rally;

            CreateWheels(car, controller, 1.7f, 3.5f);

            var collider = car.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.8f, 1.2f, 4f);

            SavePrefab(car, $"Assets/Prefabs/Vehicles/{name}.prefab");
        }

        private void CreateMotorcycle(string name, Color color)
        {
            GameObject bike = new GameObject(name);
            
            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(bike.transform);
            body.transform.localScale = new Vector3(0.5f, 0.3f, 2f);
            body.transform.rotation = Quaternion.Euler(90, 0, 0);
            body.GetComponent<Renderer>().material.color = color;
            DestroyImmediate(body.GetComponent<Collider>());

            var rigidbody = bike.AddComponent<Rigidbody>();
            rigidbody.mass = 200f;
            rigidbody.centerOfMass = new Vector3(0, -0.3f, 0);

            var controller = bike.AddComponent<VehicleController>();
            var spec = CreateVehicleSpec($"{name}_Spec", VehicleType.Motorcycle, 180f, 15f, 8f, 2f);
            controller.spec = spec;
            controller.controlType = VehicleType.Motorcycle;

            // Motorcycle wheels (tylko 2)
            CreateMotorcycleWheels(bike, controller);

            var collider = bike.AddComponent<CapsuleCollider>();
            collider.direction = 2; // Z-axis
            collider.height = 2f;
            collider.radius = 0.25f;

            SavePrefab(bike, $"Assets/Prefabs/Vehicles/{name}.prefab");
        }

        private void CreateBike(string name, Color color)
        {
            GameObject bike = new GameObject(name);
            
            // Simple bike frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.transform.SetParent(bike.transform);
            frame.transform.localScale = new Vector3(0.1f, 0.8f, 1.5f);
            frame.GetComponent<Renderer>().material.color = color;
            DestroyImmediate(frame.GetComponent<Collider>());

            var rigidbody = bike.AddComponent<Rigidbody>();
            rigidbody.mass = 15f;
            rigidbody.centerOfMass = new Vector3(0, -0.4f, 0);

            var controller = bike.AddComponent<VehicleController>();
            var spec = CreateVehicleSpec($"{name}_Spec", VehicleType.Bike, 45f, 8f, 3f, 3f);
            controller.spec = spec;
            controller.controlType = VehicleType.Bike;

            // Bike nie używa WheelCollider - używa prostszej fizyki

            var collider = bike.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.8f, 1.5f);

            SavePrefab(bike, $"Assets/Prefabs/Vehicles/{name}.prefab");
        }

        private void CreateGokart(string name, Color color)
        {
            GameObject kart = new GameObject(name);
            
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(kart.transform);
            body.transform.localScale = new Vector3(1.2f, 0.4f, 2f);
            body.GetComponent<Renderer>().material.color = color;
            DestroyImmediate(body.GetComponent<Collider>());

            var rigidbody = kart.AddComponent<Rigidbody>();
            rigidbody.mass = 150f;
            rigidbody.centerOfMass = new Vector3(0, -0.2f, 0);

            var controller = kart.AddComponent<VehicleController>();
            var spec = CreateVehicleSpec($"{name}_Spec", VehicleType.Gokart, 80f, 12f, 5f, 2.5f);
            controller.spec = spec;
            controller.controlType = VehicleType.Gokart;

            CreateWheels(kart, controller, 1f, 1.5f);

            var collider = kart.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.2f, 0.4f, 2f);

            SavePrefab(kart, $"Assets/Prefabs/Vehicles/{name}.prefab");
        }

        private VehicleSpec CreateVehicleSpec(string name, VehicleType type, float maxSpeed, float acceleration, float brake, float steer)
        {
            VehicleSpec spec = ScriptableObject.CreateInstance<VehicleSpec>();
            spec.vehicleType = type;
            spec.maxSpeedKmh = maxSpeed;
            spec.acceleration = acceleration;
            spec.brakePower = brake;
            spec.steerAngle = steer;

            string path = $"Assets/Prefabs/Vehicles/{name}.asset";
            AssetDatabase.CreateAsset(spec, path);
            return spec;
        }

        private void CreateWheels(GameObject vehicle, VehicleController controller, float width, float wheelbase)
        {
            // Front Left
            GameObject wheelFL = new GameObject("WheelFL");
            wheelFL.transform.SetParent(vehicle.transform);
            wheelFL.transform.localPosition = new Vector3(-width/2, -0.3f, wheelbase/2);
            var colliderFL = wheelFL.AddComponent<WheelCollider>();
            colliderFL.radius = 0.3f;
            controller.wheelFL = colliderFL;

            // Front Right
            GameObject wheelFR = new GameObject("WheelFR");
            wheelFR.transform.SetParent(vehicle.transform);
            wheelFR.transform.localPosition = new Vector3(width/2, -0.3f, wheelbase/2);
            var colliderFR = wheelFR.AddComponent<WheelCollider>();
            colliderFR.radius = 0.3f;
            controller.wheelFR = colliderFR;

            // Rear Left
            GameObject wheelRL = new GameObject("WheelRL");
            wheelRL.transform.SetParent(vehicle.transform);
            wheelRL.transform.localPosition = new Vector3(-width/2, -0.3f, -wheelbase/2);
            var colliderRL = wheelRL.AddComponent<WheelCollider>();
            colliderRL.radius = 0.3f;
            controller.wheelRL = colliderRL;

            // Rear Right
            GameObject wheelRR = new GameObject("WheelRR");
            wheelRR.transform.SetParent(vehicle.transform);
            wheelRR.transform.localPosition = new Vector3(width/2, -0.3f, -wheelbase/2);
            var colliderRR = wheelRR.AddComponent<WheelCollider>();
            colliderRR.radius = 0.3f;
            controller.wheelRR = colliderRR;

            // Visual wheels
            CreateWheelVisual(wheelFL, "VisualFL");
            CreateWheelVisual(wheelFR, "VisualFR");
            CreateWheelVisual(wheelRL, "VisualRL");
            CreateWheelVisual(wheelRR, "VisualRR");

            controller.visualFL = wheelFL.transform.GetChild(0);
            controller.visualFR = wheelFR.transform.GetChild(0);
            controller.visualRL = wheelRL.transform.GetChild(0);
            controller.visualRR = wheelRR.transform.GetChild(0);
        }

        private void CreateMotorcycleWheels(GameObject vehicle, VehicleController controller)
        {
            // Front Wheel
            GameObject wheelF = new GameObject("WheelFront");
            wheelF.transform.SetParent(vehicle.transform);
            wheelF.transform.localPosition = new Vector3(0, -0.3f, 1f);
            var colliderF = wheelF.AddComponent<WheelCollider>();
            colliderF.radius = 0.35f;
            controller.wheelFL = colliderF; // Używamy FL dla przedniego koła
            controller.wheelFR = colliderF; // I FR też

            // Rear Wheel
            GameObject wheelR = new GameObject("WheelRear");
            wheelR.transform.SetParent(vehicle.transform);
            wheelR.transform.localPosition = new Vector3(0, -0.3f, -1f);
            var colliderR = wheelR.AddComponent<WheelCollider>();
            colliderR.radius = 0.35f;
            controller.wheelRL = colliderR;
            controller.wheelRR = colliderR;

            CreateWheelVisual(wheelF, "VisualFront");
            CreateWheelVisual(wheelR, "VisualRear");

            controller.visualFL = wheelF.transform.GetChild(0);
            controller.visualRL = wheelR.transform.GetChild(0);
        }

        private void CreateWheelVisual(GameObject wheelParent, string name)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = name;
            visual.transform.SetParent(wheelParent.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0, 0, 90);
            visual.transform.localScale = new Vector3(0.6f, 0.2f, 0.6f);
            visual.GetComponent<Renderer>().material.color = Color.black;
            DestroyImmediate(visual.GetComponent<Collider>());
        }

        #endregion

        #region UI Prefabs

        private void GenerateUIPrefabs()
        {
            CreateHUDPrefab();
            CreateMainMenuPrefab();
            CreateGarageUIPrefab();
            CreateMissionSelectPrefab();
            CreateResultsUIPrefab();
            CreateLoadingScreenPrefab();

            Debug.Log("[PrefabGenerator] UI prefabs generated");
        }

        private void CreateHUDPrefab()
        {
            GameObject hud = new GameObject("HUD");
            
            Canvas canvas = hud.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            
            hud.AddComponent<UnityEngine.UI.CanvasScaler>();
            hud.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Speedometer
            CreateSpeedometer(hud);
            
            // Minimap
            CreateMinimap(hud);
            
            // Race info
            CreateRaceInfo(hud);

            SavePrefab(hud, "Assets/Prefabs/UI/HUD.prefab");
        }

        private void CreateSpeedometer(GameObject parent)
        {
            GameObject speedometer = new GameObject("Speedometer");
            speedometer.transform.SetParent(parent.transform);

            // Background circle
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(speedometer.transform);
            var bgImage = bg.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);
            
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.02f, 0.02f);
            bgRect.anchorMax = new Vector2(0.25f, 0.35f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Speed text
            GameObject speedText = new GameObject("SpeedText");
            speedText.transform.SetParent(speedometer.transform);
            var text = speedText.AddComponent<UnityEngine.UI.Text>();
            text.text = "0 KM/H";
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            RectTransform textRect = speedText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.02f, 0.02f);
            textRect.anchorMax = new Vector2(0.25f, 0.35f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        private void CreateMinimap(GameObject parent)
        {
            GameObject minimap = new GameObject("Minimap");
            minimap.transform.SetParent(parent.transform);

            var image = minimap.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            RectTransform rect = minimap.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.75f, 0.75f);
            rect.anchorMax = new Vector2(0.98f, 0.98f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Minimap label
            GameObject label = new GameObject("Label");
            label.transform.SetParent(minimap.transform);
            var labelText = label.AddComponent<UnityEngine.UI.Text>();
            labelText.text = "MAP";
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private void CreateRaceInfo(GameObject parent)
        {
            GameObject raceInfo = new GameObject("RaceInfo");
            raceInfo.transform.SetParent(parent.transform);

            // Lap counter
            GameObject lapText = new GameObject("LapText");
            lapText.transform.SetParent(raceInfo.transform);
            var text = lapText.AddComponent<UnityEngine.UI.Text>();
            text.text = "LAP 1/3";
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            RectTransform lapRect = lapText.GetComponent<RectTransform>();
            lapRect.anchorMin = new Vector2(0.4f, 0.85f);
            lapRect.anchorMax = new Vector2(0.6f, 0.95f);
            lapRect.offsetMin = Vector2.zero;
            lapRect.offsetMax = Vector2.zero;

            // Timer
            GameObject timerText = new GameObject("TimerText");
            timerText.transform.SetParent(raceInfo.transform);
            var timerTextComp = timerText.AddComponent<UnityEngine.UI.Text>();
            timerTextComp.text = "00:00.000";
            timerTextComp.fontSize = 18;
            timerTextComp.alignment = TextAnchor.MiddleCenter;
            timerTextComp.color = Color.yellow;

            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.4f, 0.75f);
            timerRect.anchorMax = new Vector2(0.6f, 0.85f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
        }

        private void CreateMainMenuPrefab()
        {
            // Ten prefab jest już tworzony w SceneGenerator, możemy go skopiować
            GameObject mainMenu = new GameObject("MainMenu");
            
            Canvas canvas = mainMenu.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Dodaj komponenty UI jak w SceneGenerator...
            
            SavePrefab(mainMenu, "Assets/Prefabs/UI/MainMenu.prefab");
        }

        private void CreateGarageUIPrefab()
        {
            GameObject garageUI = new GameObject("GarageUI");
            
            Canvas canvas = garageUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Vehicle display area
            GameObject vehiclePanel = CreateUIPanel(garageUI, "VehiclePanel", new Vector2(0.05f, 0.3f), new Vector2(0.45f, 0.95f));
            
            // Stats panel
            GameObject statsPanel = CreateUIPanel(garageUI, "StatsPanel", new Vector2(0.55f, 0.5f), new Vector2(0.95f, 0.95f));
            
            // Upgrade panel
            GameObject upgradePanel = CreateUIPanel(garageUI, "UpgradePanel", new Vector2(0.55f, 0.05f), new Vector2(0.95f, 0.45f));

            SavePrefab(garageUI, "Assets/Prefabs/UI/GarageUI.prefab");
        }

        private void CreateMissionSelectPrefab()
        {
            GameObject missionUI = new GameObject("MissionSelectUI");
            
            Canvas canvas = missionUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Mission list
            GameObject missionList = CreateUIPanel(missionUI, "MissionList", new Vector2(0.1f, 0.2f), new Vector2(0.6f, 0.8f));
            
            // Mission details
            GameObject missionDetails = CreateUIPanel(missionUI, "MissionDetails", new Vector2(0.65f, 0.2f), new Vector2(0.9f, 0.8f));

            SavePrefab(missionUI, "Assets/Prefabs/UI/MissionSelectUI.prefab");
        }

        private void CreateResultsUIPrefab()
        {
            GameObject resultsUI = new GameObject("ResultsUI");
            
            Canvas canvas = resultsUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Results panel
            GameObject resultsPanel = CreateUIPanel(resultsUI, "ResultsPanel", new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f));

            SavePrefab(resultsUI, "Assets/Prefabs/UI/ResultsUI.prefab");
        }

        private void CreateLoadingScreenPrefab()
        {
            GameObject loadingScreen = new GameObject("LoadingScreen");
            
            Canvas canvas = loadingScreen.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(loadingScreen.transform);
            var bgImage = bg.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = Color.black;

            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Loading text
            GameObject loadingText = new GameObject("LoadingText");
            loadingText.transform.SetParent(loadingScreen.transform);
            var text = loadingText.AddComponent<UnityEngine.UI.Text>();
            text.text = "LOADING...";
            text.fontSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            RectTransform textRect = loadingText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.3f, 0.4f);
            textRect.anchorMax = new Vector2(0.7f, 0.6f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            SavePrefab(loadingScreen, "Assets/Prefabs/UI/LoadingScreen.prefab");
        }

        private GameObject CreateUIPanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent.transform);
            
            var image = panel.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return panel;
        }

        #endregion

        #region Environment Prefabs

        private void GenerateEnvironmentPrefabs()
        {
            CreateJumpRamp();
            CreateCheckpoint();
            CreateLootCrate();
            CreateSpeedBoost();
            CreateObstacle();
            CreateBridge();

            Debug.Log("[PrefabGenerator] Environment prefabs generated");
        }

        private void CreateJumpRamp()
        {
            GameObject ramp = new GameObject("JumpRamp");
            
            // Ramp mesh
            GameObject rampMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampMesh.transform.SetParent(ramp.transform);
            rampMesh.transform.localScale = new Vector3(8f, 1f, 4f);
            rampMesh.transform.rotation = Quaternion.Euler(-15f, 0, 0);
            rampMesh.GetComponent<Renderer>().material.color = new Color(0.8f, 0.6f, 0.4f);

            // Trigger zone
            GameObject trigger = new GameObject("TriggerZone");
            trigger.transform.SetParent(ramp.transform);
            var triggerCollider = trigger.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(8f, 2f, 4f);

            SavePrefab(ramp, "Assets/Prefabs/Environment/JumpRamp.prefab");
        }

        private void CreateCheckpoint()
        {
            GameObject checkpoint = new GameObject("Checkpoint");
            
            // Left pole
            GameObject leftPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftPole.transform.SetParent(checkpoint.transform);
            leftPole.transform.localPosition = new Vector3(-3f, 2.5f, 0);
            leftPole.transform.localScale = new Vector3(0.3f, 2.5f, 0.3f);
            leftPole.GetComponent<Renderer>().material.color = Color.red;
            DestroyImmediate(leftPole.GetComponent<Collider>());

            // Right pole
            GameObject rightPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightPole.transform.SetParent(checkpoint.transform);
            rightPole.transform.localPosition = new Vector3(3f, 2.5f, 0);
            rightPole.transform.localScale = new Vector3(0.3f, 2.5f, 0.3f);
            rightPole.GetComponent<Renderer>().material.color = Color.red;
            DestroyImmediate(rightPole.GetComponent<Collider>());

            // Banner
            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            banner.transform.SetParent(checkpoint.transform);
            banner.transform.localPosition = new Vector3(0, 4f, 0);
            banner.transform.localScale = new Vector3(6f, 1f, 0.1f);
            banner.GetComponent<Renderer>().material.color = Color.yellow;
            DestroyImmediate(banner.GetComponent<Collider>());

            // Trigger
            GameObject trigger = new GameObject("CheckpointTrigger");
            trigger.transform.SetParent(checkpoint.transform);
            var triggerCollider = trigger.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(8f, 6f, 2f);

            SavePrefab(checkpoint, "Assets/Prefabs/Environment/Checkpoint.prefab");
        }

        private void CreateLootCrate()
        {
            GameObject crate = new GameObject("LootCrate");
            
            GameObject crateMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crateMesh.transform.SetParent(crate.transform);
            crateMesh.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            crateMesh.GetComponent<Renderer>().material.color = new Color(0.6f, 0.4f, 0.2f);

            var triggerCollider = crate.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(2f, 2f, 2f);

            // Add loot component
            crate.AddComponent<LootPickup>();

            SavePrefab(crate, "Assets/Prefabs/Environment/LootCrate.prefab");
        }

        private void CreateSpeedBoost()
        {
            GameObject boost = new GameObject("SpeedBoost");
            
            // Base
            GameObject baseMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseMesh.transform.SetParent(boost.transform);
            baseMesh.transform.localScale = new Vector3(3f, 0.2f, 3f);
            baseMesh.GetComponent<Renderer>().material.color = Color.blue;
            DestroyImmediate(baseMesh.GetComponent<Collider>());

            // Arrow indicators
            for (int i = 0; i < 3; i++)
            {
                GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arrow.transform.SetParent(boost.transform);
                arrow.transform.localPosition = new Vector3(0, 0.5f + i * 0.3f, 0);
                arrow.transform.localScale = new Vector3(0.5f, 0.1f, 1f);
                arrow.GetComponent<Renderer>().material.color = Color.cyan;
                DestroyImmediate(arrow.GetComponent<Collider>());
            }

            var triggerCollider = boost.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(3f, 2f, 3f);

            SavePrefab(boost, "Assets/Prefabs/Environment/SpeedBoost.prefab");
        }

        private void CreateObstacle()
        {
            GameObject obstacle = new GameObject("Obstacle");
            
            GameObject obstacleMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacleMesh.transform.SetParent(obstacle.transform);
            obstacleMesh.transform.localScale = new Vector3(2f, 2f, 2f);
            obstacleMesh.GetComponent<Renderer>().material.color = Color.gray;

            SavePrefab(obstacle, "Assets/Prefabs/Environment/Obstacle.prefab");
        }

        private void CreateBridge()
        {
            GameObject bridge = new GameObject("Bridge");
            
            // Bridge deck
            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.transform.SetParent(bridge.transform);
            deck.transform.localScale = new Vector3(20f, 1f, 4f);
            deck.GetComponent<Renderer>().material.color = new Color(0.4f, 0.3f, 0.2f);

            // Support pillars
            for (int i = -2; i <= 2; i++)
            {
                if (i == 0) continue; // Skip center
                
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.SetParent(bridge.transform);
                pillar.transform.localPosition = new Vector3(i * 5f, -5f, 0);
                pillar.transform.localScale = new Vector3(0.8f, 5f, 0.8f);
                pillar.GetComponent<Renderer>().material.color = Color.gray;
            }

            SavePrefab(bridge, "Assets/Prefabs/Environment/Bridge.prefab");
        }

        #endregion

        #region Gameplay Prefabs

        private void GenerateGameplayPrefabs()
        {
            CreateNPCDriver();
            CreateSpawnPoint();
            CreateWeatherSystem();
            CreateGameManager();

            Debug.Log("[PrefabGenerator] Gameplay prefabs generated");
        }

        private void CreateNPCDriver()
        {
            GameObject npc = new GameObject("NPCDriver");
            
            // Use existing car as base
            GameObject carBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            carBody.transform.SetParent(npc.transform);
            carBody.transform.localScale = new Vector3(2f, 0.8f, 4f);
            carBody.GetComponent<Renderer>().material.color = Color.white;
            DestroyImmediate(carBody.GetComponent<Collider>());

            var rigidbody = npc.AddComponent<Rigidbody>();
            rigidbody.mass = 1200f;

            var controller = npc.AddComponent<VehicleController>();
            var aiController = npc.AddComponent<ExtremeRacing.NPC.NPCDriver>();
            
            var spec = CreateVehicleSpec("NPC_Car_Spec", VehicleType.Supercar, 250f, 20f, 6f, 1.5f);
            controller.spec = spec;

            CreateWheels(npc, controller, 1.8f, 4f);

            var collider = npc.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 0.8f, 4f);

            SavePrefab(npc, "Assets/Prefabs/Gameplay/NPCDriver.prefab");
        }

        private void CreateSpawnPoint()
        {
            GameObject spawnPoint = new GameObject("SpawnPoint");
            
            // Visual marker
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.transform.SetParent(spawnPoint.transform);
            marker.transform.localScale = new Vector3(3f, 0.1f, 3f);
            marker.GetComponent<Renderer>().material.color = Color.green;
            DestroyImmediate(marker.GetComponent<Collider>());

            var collider = spawnPoint.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(4f, 2f, 4f);

            SavePrefab(spawnPoint, "Assets/Prefabs/Gameplay/SpawnPoint.prefab");
        }

        private void CreateWeatherSystem()
        {
            GameObject weatherSystem = new GameObject("WeatherSystem");
            
            weatherSystem.AddComponent<ExtremeRacing.Managers.WeatherManager>();
            weatherSystem.AddComponent<ExtremeRacing.Managers.TimeOfDayManager>();

            SavePrefab(weatherSystem, "Assets/Prefabs/Gameplay/WeatherSystem.prefab");
        }

        private void CreateGameManager()
        {
            GameObject gameManager = new GameObject("GameManager");
            
            gameManager.AddComponent<ExtremeRacing.Managers.GameManager>();
            gameManager.AddComponent<ExtremeRacing.Managers.InputManager>();
            gameManager.AddComponent<ExtremeRacing.Addressables.RegionStreamingManager>();

            SavePrefab(gameManager, "Assets/Prefabs/Gameplay/GameManager.prefab");
        }

        #endregion

        private void SavePrefab(GameObject obj, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);
            Debug.Log($"[PrefabGenerator] Prefab saved: {path}");
        }
    }
}