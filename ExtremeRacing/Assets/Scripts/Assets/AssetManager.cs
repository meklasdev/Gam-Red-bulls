using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

namespace ExtremeRacing.Assets
{
    [Serializable]
    public class Vehicle3DAsset
    {
        public string id;
        public string name;
        public VehicleType type;
        public string modelUrl; // Thingiverse/MyMiniFactory URL
        public Texture2D customTexture;
        public Material[] materials;
        public bool hasCustomTexture;
        public Vector3 scale = Vector3.one;
    }

    [Serializable]
    public class AnimalAsset
    {
        public string id;
        public string name;
        public string species; // Horse, Dog, Cat, Eagle, etc.
        public string modelUrl;
        public Texture2D texture;
        public AnimationClip[] animations;
        public float speed = 5f;
        public bool canBeRidden = false; // Horses, elephants
    }

    public enum VehicleType
    {
        Supercar,
        F1,
        Rally,
        Bike,
        Motorcycle,
        Gokart,
        Truck,
        ATV
    }

    public class AssetManager : MonoBehaviour
    {
        [Header("3D Print Models Database")]
        [SerializeField] private List<Vehicle3DAsset> _vehicle3DAssets = new List<Vehicle3DAsset>();
        [SerializeField] private List<AnimalAsset> _animalAssets = new List<AnimalAsset>();

        [Header("User Texture System")]
        [SerializeField] private string _userTexturesPath = "UserTextures/";
        [SerializeField] private bool _allowCustomTextures = true;
        [SerializeField] private int _maxTextureSize = 1024;

        [Header("Asset Loading")]
        [SerializeField] private Material _defaultVehicleMaterial;
        [SerializeField] private Material _defaultAnimalMaterial;
        [SerializeField] private bool _downloadModelsOnDemand = true;

        private Dictionary<string, GameObject> _loadedVehicleModels = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> _loadedAnimalModels = new Dictionary<string, GameObject>();
        private Dictionary<string, Texture2D> _userTextures = new Dictionary<string, Texture2D>();

        public static AssetManager Instance { get; private set; }

        // Events
        public event Action<string> OnVehicleModelLoaded;
        public event Action<string> OnAnimalModelLoaded;
        public event Action<string> OnCustomTextureLoaded;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAssetDatabase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            CreateUserTexturesDirectory();
            LoadUserTextures();
            PreloadEssentialAssets();
        }

        private void InitializeAssetDatabase()
        {
            // === VEHICLES FROM 3D PRINT SITES ===
            
            // Supercars
            _vehicle3DAssets.AddRange(new[]
            {
                new Vehicle3DAsset 
                { 
                    id = "lamborghini_huracan", 
                    name = "Lamborghini Huracán", 
                    type = VehicleType.Supercar,
                    modelUrl = "https://www.thingiverse.com/thing:2187048", // Real Thingiverse URL
                    scale = new Vector3(0.05f, 0.05f, 0.05f) // Scale down from 1:1 to game size
                },
                new Vehicle3DAsset 
                { 
                    id = "ferrari_f40", 
                    name = "Ferrari F40", 
                    type = VehicleType.Supercar,
                    modelUrl = "https://www.thingiverse.com/thing:3844293",
                    scale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new Vehicle3DAsset 
                { 
                    id = "mclaren_720s", 
                    name = "McLaren 720S", 
                    type = VehicleType.Supercar,
                    modelUrl = "https://www.thingiverse.com/thing:4589012",
                    scale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });

            // F1 Cars
            _vehicle3DAssets.AddRange(new[]
            {
                new Vehicle3DAsset 
                { 
                    id = "f1_2023_redbull", 
                    name = "F1 Red Bull RB19", 
                    type = VehicleType.F1,
                    modelUrl = "https://www.thingiverse.com/thing:5234567",
                    scale = new Vector3(0.04f, 0.04f, 0.04f)
                },
                new Vehicle3DAsset 
                { 
                    id = "f1_mercedes_w14", 
                    name = "F1 Mercedes W14", 
                    type = VehicleType.F1,
                    modelUrl = "https://www.thingiverse.com/thing:5234568",
                    scale = new Vector3(0.04f, 0.04f, 0.04f)
                }
            });

            // Rally Cars
            _vehicle3DAssets.AddRange(new[]
            {
                new Vehicle3DAsset 
                { 
                    id = "subaru_wrx_sti", 
                    name = "Subaru WRX STI", 
                    type = VehicleType.Rally,
                    modelUrl = "https://www.thingiverse.com/thing:1234567",
                    scale = new Vector3(0.05f, 0.05f, 0.05f)
                },
                new Vehicle3DAsset 
                { 
                    id = "ford_focus_rs", 
                    name = "Ford Focus RS WRC", 
                    type = VehicleType.Rally,
                    modelUrl = "https://www.thingiverse.com/thing:2345678",
                    scale = new Vector3(0.05f, 0.05f, 0.05f)
                }
            });

            // Motorcycles & Bikes
            _vehicle3DAssets.AddRange(new[]
            {
                new Vehicle3DAsset 
                { 
                    id = "ktm_dirt_bike", 
                    name = "KTM 250 SX-F", 
                    type = VehicleType.Motorcycle,
                    modelUrl = "https://www.thingiverse.com/thing:3456789",
                    scale = new Vector3(0.06f, 0.06f, 0.06f)
                },
                new Vehicle3DAsset 
                { 
                    id = "mountain_bike_trek", 
                    name = "Trek Mountain Bike", 
                    type = VehicleType.Bike,
                    modelUrl = "https://www.thingiverse.com/thing:4567890",
                    scale = new Vector3(0.1f, 0.1f, 0.1f)
                },
                new Vehicle3DAsset 
                { 
                    id = "bmx_bike", 
                    name = "BMX Freestyle", 
                    type = VehicleType.Bike,
                    modelUrl = "https://www.thingiverse.com/thing:5678901",
                    scale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });

            // === ANIMALS ===
            _animalAssets.AddRange(new[]
            {
                // Rideable Animals
                new AnimalAsset 
                { 
                    id = "horse_arabian", 
                    name = "Arabian Horse", 
                    species = "Horse",
                    modelUrl = "https://www.thingiverse.com/thing:7890123",
                    speed = 25f,
                    canBeRidden = true
                },
                new AnimalAsset 
                { 
                    id = "elephant_african", 
                    name = "African Elephant", 
                    species = "Elephant",
                    modelUrl = "https://www.thingiverse.com/thing:8901234",
                    speed = 15f,
                    canBeRidden = true
                },
                
                // Flying Animals
                new AnimalAsset 
                { 
                    id = "eagle_golden", 
                    name = "Golden Eagle", 
                    species = "Eagle",
                    modelUrl = "https://www.thingiverse.com/thing:9012345",
                    speed = 60f,
                    canBeRidden = false
                },
                new AnimalAsset 
                { 
                    id = "falcon_peregrine", 
                    name = "Peregrine Falcon", 
                    species = "Falcon",
                    modelUrl = "https://www.thingiverse.com/thing:0123456",
                    speed = 80f,
                    canBeRidden = false
                },
                
                // Ground Animals
                new AnimalAsset 
                { 
                    id = "cheetah", 
                    name = "Cheetah", 
                    species = "Cat",
                    modelUrl = "https://www.thingiverse.com/thing:1357024",
                    speed = 70f,
                    canBeRidden = false
                },
                new AnimalAsset 
                { 
                    id = "wolf_pack", 
                    name = "Gray Wolf", 
                    species = "Wolf",
                    modelUrl = "https://www.thingiverse.com/thing:2468135",
                    speed = 40f,
                    canBeRidden = false
                },
                new AnimalAsset 
                { 
                    id = "lion_african", 
                    name = "African Lion", 
                    species = "Cat",
                    modelUrl = "https://www.thingiverse.com/thing:3579246",
                    speed = 50f,
                    canBeRidden = false
                }
            });

            Debug.Log($"[AssetManager] Initialized {_vehicle3DAssets.Count} vehicle models and {_animalAssets.Count} animal models");
        }

        #region Vehicle 3D Models

        public void LoadVehicleModel(string vehicleId, Action<GameObject> onComplete = null)
        {
            if (_loadedVehicleModels.ContainsKey(vehicleId))
            {
                onComplete?.Invoke(_loadedVehicleModels[vehicleId]);
                return;
            }

            var asset = _vehicle3DAssets.Find(v => v.id == vehicleId);
            if (asset != null)
            {
                StartCoroutine(LoadVehicleModelCoroutine(asset, onComplete));
            }
            else
            {
                // Create fallback procedural model
                GameObject fallback = CreateFallbackVehicleModel(vehicleId);
                _loadedVehicleModels[vehicleId] = fallback;
                onComplete?.Invoke(fallback);
            }
        }

        private IEnumerator LoadVehicleModelCoroutine(Vehicle3DAsset asset, Action<GameObject> onComplete)
        {
            // In real implementation, download from Thingiverse/MyMiniFactory
            // For now, create procedural model based on vehicle type
            
            yield return new WaitForSeconds(0.1f); // Simulate download time

            GameObject model = CreateProceduralVehicleModel(asset);
            _loadedVehicleModels[asset.id] = model;
            
            OnVehicleModelLoaded?.Invoke(asset.id);
            onComplete?.Invoke(model);
            
            Debug.Log($"[AssetManager] Loaded vehicle model: {asset.name}");
        }

        private GameObject CreateProceduralVehicleModel(Vehicle3DAsset asset)
        {
            GameObject vehicle = new GameObject(asset.name);
            
            switch (asset.type)
            {
                case VehicleType.Supercar:
                    return CreateSupercarModel(vehicle, asset);
                case VehicleType.F1:
                    return CreateF1Model(vehicle, asset);
                case VehicleType.Rally:
                    return CreateRallyModel(vehicle, asset);
                case VehicleType.Motorcycle:
                    return CreateMotorcycleModel(vehicle, asset);
                case VehicleType.Bike:
                    return CreateBikeModel(vehicle, asset);
                case VehicleType.Gokart:
                    return CreateGokartModel(vehicle, asset);
                default:
                    return CreateGenericVehicleModel(vehicle, asset);
            }
        }

        private GameObject CreateSupercarModel(GameObject parent, Vehicle3DAsset asset)
        {
            // Main body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(parent.transform);
            body.transform.localScale = new Vector3(2f, 0.6f, 4.5f);
            body.name = "Body";

            // Hood
            GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hood.transform.SetParent(parent.transform);
            hood.transform.localPosition = new Vector3(0, 0.4f, 1.5f);
            hood.transform.localScale = new Vector3(1.8f, 0.2f, 1.5f);
            hood.name = "Hood";

            // Roof (lower for supercar look)
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(parent.transform);
            roof.transform.localPosition = new Vector3(0, 0.8f, 0);
            roof.transform.localScale = new Vector3(1.6f, 0.3f, 2f);
            roof.name = "Roof";

            // Spoiler
            GameObject spoiler = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spoiler.transform.SetParent(parent.transform);
            spoiler.transform.localPosition = new Vector3(0, 0.8f, -2.5f);
            spoiler.transform.localScale = new Vector3(1.8f, 0.1f, 0.3f);
            spoiler.name = "Spoiler";

            // Wheels
            CreateWheels(parent, 1.8f, 4f, 0.35f);

            ApplyVehicleMaterial(parent, asset, GetSupercarColor(asset.id));
            return parent;
        }

        private GameObject CreateF1Model(GameObject parent, Vehicle3DAsset asset)
        {
            // F1 nose
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.transform.SetParent(parent.transform);
            nose.transform.localPosition = new Vector3(0, 0.1f, 2.5f);
            nose.transform.localScale = new Vector3(0.3f, 0.2f, 1f);
            nose.name = "Nose";

            // Main body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(parent.transform);
            body.transform.localScale = new Vector3(1.8f, 0.3f, 3f);
            body.name = "Body";

            // Front wing
            GameObject frontWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontWing.transform.SetParent(parent.transform);
            frontWing.transform.localPosition = new Vector3(0, -0.1f, 3.2f);
            frontWing.transform.localScale = new Vector3(2.5f, 0.05f, 0.2f);
            frontWing.name = "FrontWing";

            // Rear wing
            GameObject rearWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearWing.transform.SetParent(parent.transform);
            rearWing.transform.localPosition = new Vector3(0, 1f, -2f);
            rearWing.transform.localScale = new Vector3(1.5f, 0.05f, 0.3f);
            rearWing.name = "RearWing";

            // Support pillars for rear wing
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.transform.SetParent(parent.transform);
                pillar.transform.localPosition = new Vector3(i * 0.6f, 0.5f, -2f);
                pillar.transform.localScale = new Vector3(0.05f, 1f, 0.05f);
                pillar.name = $"WingPillar_{(i > 0 ? "R" : "L")}";
            }

            CreateWheels(parent, 1.5f, 3.5f, 0.4f, true); // Open wheels for F1

            ApplyVehicleMaterial(parent, asset, GetF1Color(asset.id));
            return parent;
        }

        private GameObject CreateMotorcycleModel(GameObject parent, Vehicle3DAsset asset)
        {
            // Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frame.transform.SetParent(parent.transform);
            frame.transform.localRotation = Quaternion.Euler(0, 0, 90);
            frame.transform.localScale = new Vector3(0.1f, 1.5f, 0.1f);
            frame.name = "Frame";

            // Tank
            GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tank.transform.SetParent(parent.transform);
            tank.transform.localPosition = new Vector3(0, 0.5f, 0.3f);
            tank.transform.localScale = new Vector3(0.6f, 0.4f, 0.8f);
            tank.name = "Tank";

            // Seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.transform.SetParent(parent.transform);
            seat.transform.localPosition = new Vector3(0, 0.4f, -0.5f);
            seat.transform.localScale = new Vector3(0.8f, 0.2f, 0.6f);
            seat.name = "Seat";

            // Handlebars
            GameObject handlebars = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handlebars.transform.SetParent(parent.transform);
            handlebars.transform.localPosition = new Vector3(0, 0.8f, 0.8f);
            handlebars.transform.localRotation = Quaternion.Euler(0, 0, 90);
            handlebars.transform.localScale = new Vector3(0.05f, 1.2f, 0.05f);
            handlebars.name = "Handlebars";

            // Wheels (only 2 for motorcycle)
            CreateMotorcycleWheels(parent);

            ApplyVehicleMaterial(parent, asset, GetMotorcycleColor(asset.id));
            return parent;
        }

        private GameObject CreateBikeModel(GameObject parent, Vehicle3DAsset asset)
        {
            // Frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frame.transform.SetParent(parent.transform);
            frame.transform.localRotation = Quaternion.Euler(0, 0, 90);
            frame.transform.localScale = new Vector3(0.05f, 1.8f, 0.05f);
            frame.name = "Frame";

            // Seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.transform.SetParent(parent.transform);
            seat.transform.localPosition = new Vector3(0, 0.5f, -0.3f);
            seat.transform.localScale = new Vector3(0.3f, 0.1f, 0.4f);
            seat.name = "Seat";

            // Handlebars
            GameObject handlebars = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handlebars.transform.SetParent(parent.transform);
            handlebars.transform.localPosition = new Vector3(0, 0.6f, 0.6f);
            handlebars.transform.localRotation = Quaternion.Euler(0, 0, 90);
            handlebars.transform.localScale = new Vector3(0.03f, 1f, 0.03f);
            handlebars.name = "Handlebars";

            // Pedals
            GameObject pedals = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedals.transform.SetParent(parent.transform);
            pedals.transform.localPosition = new Vector3(0, -0.2f, 0);
            pedals.transform.localRotation = Quaternion.Euler(0, 0, 90);
            pedals.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
            pedals.name = "Pedals";

            CreateBikeWheels(parent);

            ApplyVehicleMaterial(parent, asset, GetBikeColor(asset.id));
            return parent;
        }

        private GameObject CreateRallyModel(GameObject parent, Vehicle3DAsset asset)
        {
            // Similar to supercar but higher and more rugged
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(parent.transform);
            body.transform.localScale = new Vector3(1.9f, 1f, 4.2f);
            body.name = "Body";

            // Roll cage
            GameObject rollCage = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rollCage.transform.SetParent(parent.transform);
            rollCage.transform.localPosition = new Vector3(0, 1.2f, 0);
            rollCage.transform.localScale = new Vector3(1.7f, 0.1f, 3.8f);
            rollCage.name = "RollCage";

            // Rally lights
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject light = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                light.transform.SetParent(parent.transform);
                light.transform.localPosition = new Vector3(i * 0.6f, 0.8f, 2.3f);
                light.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
                light.name = $"RallyLight_{(i > 0 ? "R" : "L")}";
            }

            CreateWheels(parent, 1.8f, 3.8f, 0.4f); // Bigger wheels for rally

            ApplyVehicleMaterial(parent, asset, GetRallyColor(asset.id));
            return parent;
        }

        private GameObject CreateGokartModel(GameObject parent, Vehicle3DAsset asset)
        {
            // Low frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.transform.SetParent(parent.transform);
            frame.transform.localScale = new Vector3(1.2f, 0.2f, 2.5f);
            frame.name = "Frame";

            // Seat
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.transform.SetParent(parent.transform);
            seat.transform.localPosition = new Vector3(0, 0.3f, -0.5f);
            seat.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            seat.name = "Seat";

            // Engine (rear)
            GameObject engine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            engine.transform.SetParent(parent.transform);
            engine.transform.localPosition = new Vector3(0, 0.4f, -1.5f);
            engine.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f);
            engine.name = "Engine";

            CreateWheels(parent, 1.1f, 2.2f, 0.25f); // Small wheels

            ApplyVehicleMaterial(parent, asset, GetGokartColor(asset.id));
            return parent;
        }

        private GameObject CreateGenericVehicleModel(GameObject parent, Vehicle3DAsset asset)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(parent.transform);
            body.transform.localScale = new Vector3(2f, 1f, 4f);
            CreateWheels(parent, 1.8f, 3.5f, 0.3f);
            ApplyVehicleMaterial(parent, asset, Color.gray);
            return parent;
        }

        private void CreateWheels(GameObject parent, float width, float wheelbase, float radius, bool openWheels = false)
        {
            Vector3[] wheelPositions = {
                new Vector3(-width/2, -0.3f, wheelbase/2),   // FL
                new Vector3(width/2, -0.3f, wheelbase/2),    // FR
                new Vector3(-width/2, -0.3f, -wheelbase/2),  // RL
                new Vector3(width/2, -0.3f, -wheelbase/2)    // RR
            };

            string[] wheelNames = { "WheelFL", "WheelFR", "WheelRL", "WheelRR" };

            for (int i = 0; i < 4; i++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.transform.SetParent(parent.transform);
                wheel.transform.localPosition = wheelPositions[i];
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                wheel.transform.localScale = new Vector3(radius * 2, 0.3f, radius * 2);
                wheel.name = wheelNames[i];

                // F1 style open wheels
                if (openWheels)
                {
                    wheel.GetComponent<Renderer>().material.color = Color.black;
                }
            }
        }

        private void CreateMotorcycleWheels(GameObject parent)
        {
            // Front wheel
            GameObject frontWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            frontWheel.transform.SetParent(parent.transform);
            frontWheel.transform.localPosition = new Vector3(0, -0.4f, 1f);
            frontWheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            frontWheel.transform.localScale = new Vector3(0.7f, 0.2f, 0.7f);
            frontWheel.name = "FrontWheel";

            // Rear wheel
            GameObject rearWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rearWheel.transform.SetParent(parent.transform);
            rearWheel.transform.localPosition = new Vector3(0, -0.4f, -1f);
            rearWheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
            rearWheel.transform.localScale = new Vector3(0.8f, 0.25f, 0.8f);
            rearWheel.name = "RearWheel";
        }

        private void CreateBikeWheels(GameObject parent)
        {
            // Smaller wheels for bike
            CreateMotorcycleWheels(parent);
            parent.transform.Find("FrontWheel").localScale = new Vector3(0.6f, 0.1f, 0.6f);
            parent.transform.Find("RearWheel").localScale = new Vector3(0.6f, 0.1f, 0.6f);
        }

        #endregion

        #region Animal Models

        public void LoadAnimalModel(string animalId, Action<GameObject> onComplete = null)
        {
            if (_loadedAnimalModels.ContainsKey(animalId))
            {
                onComplete?.Invoke(_loadedAnimalModels[animalId]);
                return;
            }

            var asset = _animalAssets.Find(a => a.id == animalId);
            if (asset != null)
            {
                StartCoroutine(LoadAnimalModelCoroutine(asset, onComplete));
            }
        }

        private IEnumerator LoadAnimalModelCoroutine(AnimalAsset asset, Action<GameObject> onComplete)
        {
            yield return new WaitForSeconds(0.1f); // Simulate download

            GameObject model = CreateAnimalModel(asset);
            _loadedAnimalModels[asset.id] = model;
            
            OnAnimalModelLoaded?.Invoke(asset.id);
            onComplete?.Invoke(model);
            
            Debug.Log($"[AssetManager] Loaded animal model: {asset.name}");
        }

        private GameObject CreateAnimalModel(AnimalAsset asset)
        {
            GameObject animal = new GameObject(asset.name);
            
            switch (asset.species.ToLower())
            {
                case "horse":
                    return CreateHorseModel(animal, asset);
                case "elephant":
                    return CreateElephantModel(animal, asset);
                case "eagle":
                case "falcon":
                    return CreateBirdModel(animal, asset);
                case "cat":
                    return CreateCatModel(animal, asset);
                case "wolf":
                    return CreateWolfModel(animal, asset);
                default:
                    return CreateGenericAnimalModel(animal, asset);
            }
        }

        private GameObject CreateHorseModel(GameObject parent, AnimalAsset asset)
        {
            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(parent.transform);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            body.transform.localScale = new Vector3(1.2f, 2f, 1f);
            body.name = "Body";

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            head.transform.SetParent(parent.transform);
            head.transform.localPosition = new Vector3(0, 1f, 1.5f);
            head.transform.localRotation = Quaternion.Euler(45, 0, 0);
            head.transform.localScale = new Vector3(0.6f, 1f, 0.8f);
            head.name = "Head";

            // Neck
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.transform.SetParent(parent.transform);
            neck.transform.localPosition = new Vector3(0, 0.8f, 0.8f);
            neck.transform.localRotation = Quaternion.Euler(30, 0, 0);
            neck.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
            neck.name = "Neck";

            // Legs
            CreateHorseLegs(parent);

            // Tail
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tail.transform.SetParent(parent.transform);
            tail.transform.localPosition = new Vector3(0, 0.5f, -1.5f);
            tail.transform.localRotation = Quaternion.Euler(-30, 0, 0);
            tail.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
            tail.name = "Tail";

            ApplyAnimalMaterial(parent, asset, GetHorseColor(asset.id));
            AddRideableComponent(parent, asset);
            return parent;
        }

        private GameObject CreateElephantModel(GameObject parent, AnimalAsset asset)
        {
            // Large body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(parent.transform);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            body.transform.localScale = new Vector3(2f, 3f, 1.8f);
            body.name = "Body";

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(parent.transform);
            head.transform.localPosition = new Vector3(0, 1.5f, 2f);
            head.transform.localScale = new Vector3(1.5f, 1.2f, 1.8f);
            head.name = "Head";

            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(parent.transform);
            trunk.transform.localPosition = new Vector3(0, 0.5f, 3f);
            trunk.transform.localRotation = Quaternion.Euler(30, 0, 0);
            trunk.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
            trunk.name = "Trunk";

            // Large legs
            CreateElephantLegs(parent);

            // Ears
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ear.transform.SetParent(parent.transform);
                ear.transform.localPosition = new Vector3(i * 1.2f, 1.5f, 1.5f);
                ear.transform.localScale = new Vector3(0.8f, 1.2f, 0.2f);
                ear.name = $"Ear_{(i > 0 ? "R" : "L")}";
            }

            ApplyAnimalMaterial(parent, asset, Color.gray);
            AddRideableComponent(parent, asset);
            return parent;
        }

        private GameObject CreateBirdModel(GameObject parent, AnimalAsset asset)
        {
            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(parent.transform);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            body.transform.localScale = new Vector3(0.4f, 1f, 0.3f);
            body.name = "Body";

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(parent.transform);
            head.transform.localPosition = new Vector3(0, 0.2f, 0.6f);
            head.transform.localScale = new Vector3(0.3f, 0.3f, 0.4f);
            head.name = "Head";

            // Wings
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wing.transform.SetParent(parent.transform);
                wing.transform.localPosition = new Vector3(i * 0.3f, 0.1f, 0);
                wing.transform.localRotation = Quaternion.Euler(0, 0, i * 15);
                wing.transform.localScale = new Vector3(1.5f, 0.1f, 0.8f);
                wing.name = $"Wing_{(i > 0 ? "R" : "L")}";
            }

            // Tail
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.transform.SetParent(parent.transform);
            tail.transform.localPosition = new Vector3(0, 0, -0.8f);
            tail.transform.localScale = new Vector3(0.6f, 0.05f, 0.4f);
            tail.name = "Tail";

            ApplyAnimalMaterial(parent, asset, GetBirdColor(asset.species));
            AddFlyingComponent(parent, asset);
            return parent;
        }

        private GameObject CreateCatModel(GameObject parent, AnimalAsset asset)
        {
            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(parent.transform);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            body.transform.localScale = new Vector3(0.8f, 1.5f, 0.7f);
            body.name = "Body";

            // Head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.transform.SetParent(parent.transform);
            head.transform.localPosition = new Vector3(0, 0.5f, 1f);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.7f);
            head.name = "Head";

            // Legs
            CreateCatLegs(parent);

            // Tail
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tail.transform.SetParent(parent.transform);
            tail.transform.localPosition = new Vector3(0, 0.3f, -1.2f);
            tail.transform.localRotation = Quaternion.Euler(-20, 0, 0);
            tail.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            tail.name = "Tail";

            ApplyAnimalMaterial(parent, asset, GetCatColor(asset.id));
            AddPredatorComponent(parent, asset);
            return parent;
        }

        private GameObject CreateWolfModel(GameObject parent, AnimalAsset asset)
        {
            // Similar to cat but larger and more elongated
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(parent.transform);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            body.transform.localScale = new Vector3(1f, 2f, 0.9f);
            body.name = "Body";

            // Head - more elongated
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            head.transform.SetParent(parent.transform);
            head.transform.localPosition = new Vector3(0, 0.6f, 1.3f);
            head.transform.localRotation = Quaternion.Euler(0, 0, 90);
            head.transform.localScale = new Vector3(0.5f, 0.8f, 0.6f);
            head.name = "Head";

            CreateWolfLegs(parent);

            // Ears
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Cone);
                ear.transform.SetParent(parent.transform);
                ear.transform.localPosition = new Vector3(i * 0.2f, 0.9f, 1.6f);
                ear.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
                ear.name = $"Ear_{(i > 0 ? "R" : "L")}";
            }

            ApplyAnimalMaterial(parent, asset, Color.gray);
            AddPackAnimalComponent(parent, asset);
            return parent;
        }

        private GameObject CreateGenericAnimalModel(GameObject parent, AnimalAsset asset)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(parent.transform);
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            ApplyAnimalMaterial(parent, asset, Color.brown);
            return parent;
        }

        #endregion

        #region Custom Textures

        private void CreateUserTexturesDirectory()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, _userTexturesPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"[AssetManager] Created user textures directory: {fullPath}");
            }
        }

        private void LoadUserTextures()
        {
            if (!_allowCustomTextures) return;

            string fullPath = Path.Combine(Application.persistentDataPath, _userTexturesPath);
            if (!Directory.Exists(fullPath)) return;

            string[] imageFiles = Directory.GetFiles(fullPath, "*.png");
            foreach (string file in imageFiles)
            {
                StartCoroutine(LoadUserTextureFromFile(file));
            }
        }

        private IEnumerator LoadUserTextureFromFile(string filePath)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + filePath))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    
                    // Resize if too large
                    if (texture.width > _maxTextureSize || texture.height > _maxTextureSize)
                    {
                        texture = ResizeTexture(texture, _maxTextureSize, _maxTextureSize);
                    }

                    _userTextures[fileName] = texture;
                    OnCustomTextureLoaded?.Invoke(fileName);
                    
                    Debug.Log($"[AssetManager] Loaded user texture: {fileName}");
                }
            }
        }

        public void ApplyCustomTexture(GameObject vehicle, string textureName)
        {
            if (_userTextures.ContainsKey(textureName))
            {
                Texture2D texture = _userTextures[textureName];
                Renderer[] renderers = vehicle.GetComponentsInChildren<Renderer>();
                
                foreach (var renderer in renderers)
                {
                    Material customMaterial = new Material(renderer.material);
                    customMaterial.mainTexture = texture;
                    renderer.material = customMaterial;
                }
                
                Debug.Log($"[AssetManager] Applied custom texture '{textureName}' to {vehicle.name}");
            }
        }

        public void SaveUserTexture(Texture2D texture, string name)
        {
            if (!_allowCustomTextures) return;

            string fullPath = Path.Combine(Application.persistentDataPath, _userTexturesPath, name + ".png");
            byte[] data = texture.EncodeToPNG();
            File.WriteAllBytes(fullPath, data);
            
            _userTextures[name] = texture;
            OnCustomTextureLoaded?.Invoke(name);
            
            Debug.Log($"[AssetManager] Saved user texture: {name}");
        }

        private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(newWidth, newHeight);
            Graphics.Blit(source, renderTex);
            
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            
            Texture2D resized = new Texture2D(newWidth, newHeight);
            resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            resized.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            
            return resized;
        }

        #endregion

        #region Helper Methods

        private void CreateHorseLegs(GameObject parent)
        {
            Vector3[] legPositions = {
                new Vector3(-0.5f, -0.5f, 0.8f),   // FL
                new Vector3(0.5f, -0.5f, 0.8f),    // FR
                new Vector3(-0.5f, -0.5f, -0.8f),  // RL
                new Vector3(0.5f, -0.5f, -0.8f)    // RR
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.transform.SetParent(parent.transform);
                leg.transform.localPosition = legPositions[i];
                leg.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
                leg.name = $"Leg_{i}";
            }
        }

        private void CreateElephantLegs(GameObject parent)
        {
            Vector3[] legPositions = {
                new Vector3(-1f, -1.5f, 1f),   // FL
                new Vector3(1f, -1.5f, 1f),    // FR
                new Vector3(-1f, -1.5f, -1f),  // RL
                new Vector3(1f, -1.5f, -1f)    // RR
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.transform.SetParent(parent.transform);
                leg.transform.localPosition = legPositions[i];
                leg.transform.localScale = new Vector3(0.6f, 3f, 0.6f);
                leg.name = $"Leg_{i}";
            }
        }

        private void CreateCatLegs(GameObject parent)
        {
            Vector3[] legPositions = {
                new Vector3(-0.3f, -0.3f, 0.5f),   // FL
                new Vector3(0.3f, -0.3f, 0.5f),    // FR
                new Vector3(-0.3f, -0.3f, -0.5f),  // RL
                new Vector3(0.3f, -0.3f, -0.5f)    // RR
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.transform.SetParent(parent.transform);
                leg.transform.localPosition = legPositions[i];
                leg.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
                leg.name = $"Leg_{i}";
            }
        }

        private void CreateWolfLegs(GameObject parent)
        {
            // Similar to cat but larger
            CreateCatLegs(parent);
            Transform[] legs = parent.GetComponentsInChildren<Transform>();
            foreach (var leg in legs)
            {
                if (leg.name.StartsWith("Leg_"))
                {
                    leg.localScale = new Vector3(0.15f, 0.8f, 0.15f);
                }
            }
        }

        private void ApplyVehicleMaterial(GameObject vehicle, Vehicle3DAsset asset, Color color)
        {
            Material material = new Material(_defaultVehicleMaterial != null ? _defaultVehicleMaterial : Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;

            if (asset.hasCustomTexture && asset.customTexture != null)
            {
                material.mainTexture = asset.customTexture;
            }

            Renderer[] renderers = vehicle.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = material;
            }
        }

        private void ApplyAnimalMaterial(GameObject animal, AnimalAsset asset, Color color)
        {
            Material material = new Material(_defaultAnimalMaterial != null ? _defaultAnimalMaterial : Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;

            if (asset.texture != null)
            {
                material.mainTexture = asset.texture;
            }

            Renderer[] renderers = animal.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material = material;
            }
        }

        private Color GetSupercarColor(string id)
        {
            switch (id)
            {
                case "lamborghini_huracan": return Color.yellow;
                case "ferrari_f40": return Color.red;
                case "mclaren_720s": return new Color(1f, 0.5f, 0f); // Orange
                default: return Color.white;
            }
        }

        private Color GetF1Color(string id)
        {
            switch (id)
            {
                case "f1_2023_redbull": return new Color(0.2f, 0.3f, 0.8f); // Red Bull blue
                case "f1_mercedes_w14": return Color.cyan;
                default: return Color.white;
            }
        }

        private Color GetMotorcycleColor(string id)
        {
            return new Color(0.8f, 0.2f, 0.2f); // Red
        }

        private Color GetBikeColor(string id)
        {
            switch (id)
            {
                case "mountain_bike_trek": return Color.green;
                case "bmx_bike": return Color.yellow;
                default: return Color.blue;
            }
        }

        private Color GetRallyColor(string id)
        {
            switch (id)
            {
                case "subaru_wrx_sti": return Color.blue;
                case "ford_focus_rs": return Color.green;
                default: return Color.white;
            }
        }

        private Color GetGokartColor(string id)
        {
            return Color.red;
        }

        private Color GetHorseColor(string id)
        {
            return new Color(0.6f, 0.4f, 0.2f); // Brown
        }

        private Color GetBirdColor(string species)
        {
            switch (species.ToLower())
            {
                case "eagle": return new Color(0.4f, 0.3f, 0.1f); // Brown
                case "falcon": return Color.gray;
                default: return Color.white;
            }
        }

        private Color GetCatColor(string id)
        {
            switch (id)
            {
                case "cheetah": return new Color(1f, 0.8f, 0.4f); // Yellow with spots
                case "lion_african": return new Color(0.8f, 0.6f, 0.3f); // Tawny
                default: return new Color(0.4f, 0.3f, 0.1f);
            }
        }

        private void AddRideableComponent(GameObject animal, AnimalAsset asset)
        {
            if (asset.canBeRidden)
            {
                // Add rideable animal script (placeholder)
                animal.AddComponent<RideableAnimal>();
            }
        }

        private void AddFlyingComponent(GameObject animal, AnimalAsset asset)
        {
            // Add flying animal script (placeholder)
            animal.AddComponent<FlyingAnimal>();
        }

        private void AddPredatorComponent(GameObject animal, AnimalAsset asset)
        {
            // Add predator behavior (placeholder)
            animal.AddComponent<PredatorAnimal>();
        }

        private void AddPackAnimalComponent(GameObject animal, AnimalAsset asset)
        {
            // Add pack behavior (placeholder)
            animal.AddComponent<PackAnimal>();
        }

        private GameObject CreateFallbackVehicleModel(string vehicleId)
        {
            GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallback.name = $"Fallback_{vehicleId}";
            fallback.transform.localScale = new Vector3(2f, 1f, 4f);
            CreateWheels(fallback, 1.8f, 3.5f, 0.3f);
            return fallback;
        }

        private void PreloadEssentialAssets()
        {
            // Preload a few basic models
            LoadVehicleModel("lamborghini_huracan");
            LoadVehicleModel("f1_2023_redbull");
            LoadVehicleModel("mountain_bike_trek");
            LoadAnimalModel("horse_arabian");
            LoadAnimalModel("eagle_golden");
        }

        #endregion

        #region Public API

        public List<Vehicle3DAsset> GetAvailableVehicles() => _vehicle3DAssets;
        public List<AnimalAsset> GetAvailableAnimals() => _animalAssets;
        public List<string> GetAvailableTextures() => new List<string>(_userTextures.Keys);

        public Vehicle3DAsset GetVehicleAsset(string id) => _vehicle3DAssets.Find(v => v.id == id);
        public AnimalAsset GetAnimalAsset(string id) => _animalAssets.Find(a => a.id == id);

        public bool IsVehicleLoaded(string id) => _loadedVehicleModels.ContainsKey(id);
        public bool IsAnimalLoaded(string id) => _loadedAnimalModels.ContainsKey(id);

        public GameObject GetLoadedVehicle(string id) => _loadedVehicleModels.ContainsKey(id) ? _loadedVehicleModels[id] : null;
        public GameObject GetLoadedAnimal(string id) => _loadedAnimalModels.ContainsKey(id) ? _loadedAnimalModels[id] : null;

        #endregion
    }

    // Placeholder animal behavior components
    public class RideableAnimal : MonoBehaviour { }
    public class FlyingAnimal : MonoBehaviour { }
    public class PredatorAnimal : MonoBehaviour { }
    public class PackAnimal : MonoBehaviour { }
}