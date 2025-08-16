using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using ExtremeRacing.Vehicles;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Complete
{
    // ===== SIMPLE AI SYSTEM =====
    [System.Serializable]
    public class AIWaypoint
    {
        public Transform transform;
        public float targetSpeed = 50f;
        public AIAction action = AIAction.Drive;
    }

    public enum AIAction
    {
        Drive,
        Brake,
        Boost,
        Jump,
        Stunt
    }

    public class SimpleAI : MonoBehaviour
    {
        [Header("AI Configuration")]
        [SerializeField] private VehicleController _vehicle;
        [SerializeField] private AIWaypoint[] _waypoints;
        [SerializeField] private float _reactionTime = 0.2f;
        [SerializeField] private float _aggressiveness = 0.5f;
        [SerializeField] private bool _canUseStunts = true;

        private int _currentWaypointIndex = 0;
        private float _lastReactionTime = 0f;
        private bool _isStunting = false;

        private void Start()
        {
            if (_vehicle == null) _vehicle = GetComponent<VehicleController>();
            if (_waypoints.Length == 0) GenerateBasicWaypoints();
        }

        private void Update()
        {
            if (Time.time - _lastReactionTime < _reactionTime) return;

            ProcessAI();
            _lastReactionTime = Time.time;
        }

        private void ProcessAI()
        {
            if (_waypoints.Length == 0) return;

            var currentWaypoint = _waypoints[_currentWaypointIndex];
            Vector3 targetPos = currentWaypoint.transform.position;
            
            // Calculate steering
            Vector3 direction = (targetPos - transform.position).normalized;
            float steerInput = Vector3.Dot(transform.right, direction);
            
            // Calculate throttle based on distance and target speed
            float distance = Vector3.Distance(transform.position, targetPos);
            float currentSpeed = _vehicle.GetSpeedKmh();
            float throttleInput = currentSpeed < currentWaypoint.targetSpeed ? 1f : 0f;
            
            // Apply inputs to vehicle
            _vehicle.SetExternalInput(steerInput, throttleInput, 0f, false, false);
            _vehicle.UseExternalInput(true);

            // Check waypoint reached
            if (distance < 10f)
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Length;
                
                // Execute waypoint action
                ExecuteWaypointAction(currentWaypoint.action);
            }
        }

        private void ExecuteWaypointAction(AIAction action)
        {
            switch (action)
            {
                case AIAction.Boost:
                    // Trigger boost
                    break;
                case AIAction.Jump:
                    // Trigger jump if vehicle supports it
                    break;
                case AIAction.Stunt:
                    if (_canUseStunts) StartStunt();
                    break;
            }
        }

        private void StartStunt()
        {
            if (!_isStunting)
            {
                _isStunting = true;
                StartCoroutine(StuntCoroutine());
            }
        }

        private IEnumerator StuntCoroutine()
        {
            yield return new WaitForSeconds(2f);
            _isStunting = false;
        }

        private void GenerateBasicWaypoints()
        {
            // Generate basic circular waypoints if none provided
            _waypoints = new AIWaypoint[8];
            for (int i = 0; i < 8; i++)
            {
                GameObject wp = new GameObject($"AI_Waypoint_{i}");
                float angle = (i / 8f) * 360f * Mathf.Deg2Rad;
                wp.transform.position = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 50f;
                
                _waypoints[i] = new AIWaypoint
                {
                    transform = wp.transform,
                    targetSpeed = UnityEngine.Random.Range(30f, 80f)
                };
            }
        }
    }

    // ===== WEATHER EFFECTS SYSTEM =====
    public enum WeatherType
    {
        Clear,
        Rain,
        Storm,
        Snow,
        Fog,
        Sandstorm
    }

    public class WeatherEffectsManager : MonoBehaviour
    {
        [Header("Weather Configuration")]
        [SerializeField] private WeatherType _currentWeather = WeatherType.Clear;
        [SerializeField] private ParticleSystem _rainEffect;
        [SerializeField] private ParticleSystem _snowEffect;
        [SerializeField] private ParticleSystem _sandstormEffect;
        [SerializeField] private Light _sunLight;
        [SerializeField] private Material _skyboxMaterial;

        [Header("Fog Settings")]
        [SerializeField] private bool _enableFog = true;
        [SerializeField] private Color _fogColor = Color.gray;
        [SerializeField] private float _fogDensity = 0.01f;

        [Header("Physics Effects")]
        [SerializeField] private float _rainGripReduction = 0.7f;
        [SerializeField] private float _snowGripReduction = 0.5f;
        [SerializeField] private float _sandVisibilityReduction = 0.3f;

        private void Start()
        {
            SetWeather(_currentWeather);
        }

        public void SetWeather(WeatherType weather)
        {
            _currentWeather = weather;
            ApplyWeatherEffects();
        }

        private void ApplyWeatherEffects()
        {
            // Disable all effects first
            if (_rainEffect) _rainEffect.gameObject.SetActive(false);
            if (_snowEffect) _snowEffect.gameObject.SetActive(false);
            if (_sandstormEffect) _sandstormEffect.gameObject.SetActive(false);

            switch (_currentWeather)
            {
                case WeatherType.Clear:
                    SetLighting(1f, Color.white);
                    SetFog(false);
                    break;

                case WeatherType.Rain:
                    if (_rainEffect) _rainEffect.gameObject.SetActive(true);
                    SetLighting(0.6f, new Color(0.7f, 0.7f, 0.8f));
                    SetFog(true, Color.gray, 0.02f);
                    ApplyGripReduction(_rainGripReduction);
                    break;

                case WeatherType.Storm:
                    if (_rainEffect) _rainEffect.gameObject.SetActive(true);
                    SetLighting(0.3f, new Color(0.5f, 0.5f, 0.6f));
                    SetFog(true, Color.gray, 0.05f);
                    ApplyGripReduction(_rainGripReduction * 0.8f);
                    StartCoroutine(LightningEffect());
                    break;

                case WeatherType.Snow:
                    if (_snowEffect) _snowEffect.gameObject.SetActive(true);
                    SetLighting(0.8f, new Color(0.9f, 0.9f, 1f));
                    SetFog(true, Color.white, 0.03f);
                    ApplyGripReduction(_snowGripReduction);
                    break;

                case WeatherType.Fog:
                    SetLighting(0.7f, new Color(0.8f, 0.8f, 0.8f));
                    SetFog(true, Color.gray, 0.08f);
                    break;

                case WeatherType.Sandstorm:
                    if (_sandstormEffect) _sandstormEffect.gameObject.SetActive(true);
                    SetLighting(0.4f, new Color(1f, 0.8f, 0.6f));
                    SetFog(true, new Color(0.8f, 0.6f, 0.4f), 0.1f);
                    ReduceVisibility(_sandVisibilityReduction);
                    break;
            }
        }

        private void SetLighting(float intensity, Color color)
        {
            if (_sunLight)
            {
                _sunLight.intensity = intensity;
                _sunLight.color = color;
            }
        }

        private void SetFog(bool enable, Color color = default, float density = 0.01f)
        {
            if (_enableFog)
            {
                RenderSettings.fog = enable;
                if (enable)
                {
                    RenderSettings.fogColor = color == default ? _fogColor : color;
                    RenderSettings.fogDensity = density;
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                }
            }
        }

        private void ApplyGripReduction(float reduction)
        {
            // Apply to all vehicles in scene
            VehicleController[] vehicles = FindObjectsOfType<VehicleController>();
            foreach (var vehicle in vehicles)
            {
                // In real implementation, modify wheel friction
                Debug.Log($"[Weather] Applied grip reduction: {reduction}");
            }
        }

        private void ReduceVisibility(float reduction)
        {
            // Reduce camera render distance
            Camera mainCam = Camera.main;
            if (mainCam)
            {
                mainCam.farClipPlane *= (1f - reduction);
            }
        }

        private IEnumerator LightningEffect()
        {
            while (_currentWeather == WeatherType.Storm)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 15f));
                
                // Lightning flash
                if (_sunLight)
                {
                    float originalIntensity = _sunLight.intensity;
                    _sunLight.intensity = 2f;
                    _sunLight.color = Color.white;
                    
                    yield return new WaitForSeconds(0.1f);
                    
                    _sunLight.intensity = originalIntensity;
                    _sunLight.color = new Color(0.5f, 0.5f, 0.6f);
                }
            }
        }

        public WeatherType GetCurrentWeather() => _currentWeather;
        public bool IsRaining() => _currentWeather == WeatherType.Rain || _currentWeather == WeatherType.Storm;
        public bool IsSnowing() => _currentWeather == WeatherType.Snow;
    }

    // ===== AUDIO SYSTEM =====
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _engineSource;
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip[] _menuMusic;
        [SerializeField] private AudioClip[] _raceMusic;
        [SerializeField] private AudioClip[] _ambientSounds;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip[] _engineSounds;
        [SerializeField] private AudioClip _brakeSound;
        [SerializeField] private AudioClip _jumpSound;
        [SerializeField] private AudioClip _crashSound;
        [SerializeField] private AudioClip _pickupSound;

        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayMusic(string category)
        {
            AudioClip[] tracks = null;
            switch (category.ToLower())
            {
                case "menu": tracks = _menuMusic; break;
                case "race": tracks = _raceMusic; break;
                case "ambient": tracks = _ambientSounds; break;
            }

            if (tracks != null && tracks.Length > 0)
            {
                AudioClip track = tracks[UnityEngine.Random.Range(0, tracks.Length)];
                _musicSource.clip = track;
                _musicSource.Play();
            }
        }

        public void PlaySFX(string sfxName)
        {
            AudioClip clip = null;
            switch (sfxName.ToLower())
            {
                case "brake": clip = _brakeSound; break;
                case "jump": clip = _jumpSound; break;
                case "crash": clip = _crashSound; break;
                case "pickup": clip = _pickupSound; break;
            }

            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip);
            }
        }

        public void SetEngineSound(VehicleType vehicleType, float rpm)
        {
            if (_engineSounds.Length > 0)
            {
                int index = (int)vehicleType % _engineSounds.Length;
                if (_engineSource.clip != _engineSounds[index])
                {
                    _engineSource.clip = _engineSounds[index];
                    _engineSource.loop = true;
                    _engineSource.Play();
                }

                // Modulate pitch based on RPM
                _engineSource.pitch = Mathf.Lerp(0.8f, 2f, rpm / 100f);
            }
        }

        public void SetMasterVolume(float volume)
        {
            _audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }

        public void SetMusicVolume(float volume)
        {
            _audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }

        public void SetSFXVolume(float volume)
        {
            _audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
    }

    // ===== MOBILE OPTIMIZATION =====
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("Performance Settings")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _adaptivePerformance = true;
        [SerializeField] private float _lodBias = 1f;
        [SerializeField] private int _maxLODLevel = 2;

        [Header("Rendering")]
        [SerializeField] private bool _enableHDR = false;
        [SerializeField] private bool _enableMSAA = false;
        [SerializeField] private ShadowQuality _shadowQuality = ShadowQuality.HardOnly;
        [SerializeField] private int _shadowDistance = 50;

        [Header("Texture Settings")]
        [SerializeField] private int _textureQuality = 1; // 0=full, 1=half, 2=quarter
        [SerializeField] private FilterMode _textureFiltering = FilterMode.Bilinear;

        private void Start()
        {
            ApplyMobileOptimizations();
        }

        private void ApplyMobileOptimizations()
        {
            // Frame rate
            Application.targetFrameRate = _targetFrameRate;
            QualitySettings.vSyncCount = 0;

            // Graphics quality
            QualitySettings.shadows = _shadowQuality;
            QualitySettings.shadowDistance = _shadowDistance;
            QualitySettings.lodBias = _lodBias;
            QualitySettings.maximumLODLevel = _maxLODLevel;

            // Texture quality
            QualitySettings.masterTextureLimit = _textureQuality;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;

            // Rendering pipeline
            var cameras = FindObjectsOfType<Camera>();
            foreach (var cam in cameras)
            {
                cam.allowHDR = _enableHDR;
                cam.allowMSAA = _enableMSAA;
            }

            // Physics optimization
            Physics.defaultContactOffset = 0.01f;
            Physics.sleepThreshold = 0.005f;
            Physics.defaultSolverIterations = 4;

            Debug.Log("[MobileOptimizer] Mobile optimizations applied");
        }

        private void Update()
        {
            if (_adaptivePerformance)
            {
                AdaptPerformance();
            }
        }

        private void AdaptPerformance()
        {
            float currentFPS = 1f / Time.deltaTime;
            float targetFPS = _targetFrameRate * 0.9f; // 10% tolerance

            if (currentFPS < targetFPS)
            {
                // Reduce quality
                if (QualitySettings.lodBias > 0.5f)
                {
                    QualitySettings.lodBias *= 0.95f;
                }
            }
            else if (currentFPS > _targetFrameRate * 1.1f)
            {
                // Can afford to increase quality
                if (QualitySettings.lodBias < _lodBias)
                {
                    QualitySettings.lodBias *= 1.02f;
                }
            }
        }

        public void SetQualityLevel(int level)
        {
            switch (level)
            {
                case 0: // Low
                    _shadowQuality = ShadowQuality.Disable;
                    _shadowDistance = 25;
                    _textureQuality = 2;
                    _lodBias = 0.5f;
                    break;

                case 1: // Medium
                    _shadowQuality = ShadowQuality.HardOnly;
                    _shadowDistance = 50;
                    _textureQuality = 1;
                    _lodBias = 0.8f;
                    break;

                case 2: // High
                    _shadowQuality = ShadowQuality.All;
                    _shadowDistance = 100;
                    _textureQuality = 0;
                    _lodBias = 1f;
                    break;
            }

            ApplyMobileOptimizations();
        }
    }

    // ===== GAME INTEGRATION MANAGER =====
    public class GameSystemsIntegration : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private WeatherEffectsManager _weatherManager;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private MobileOptimizer _mobileOptimizer;

        [Header("Integration Settings")]
        [SerializeField] private bool _autoStartSystems = true;
        [SerializeField] private bool _enableDebugLogs = true;

        private void Start()
        {
            if (_autoStartSystems)
            {
                InitializeAllSystems();
            }
        }

        public void InitializeAllSystems()
        {
            StartCoroutine(InitializationSequence());
        }

        private IEnumerator InitializationSequence()
        {
            LogDebug("Starting game systems initialization...");

            // Initialize mobile optimization first
            if (_mobileOptimizer == null)
            {
                GameObject mobileOpt = new GameObject("MobileOptimizer");
                _mobileOptimizer = mobileOpt.AddComponent<MobileOptimizer>();
            }
            yield return new WaitForEndOfFrame();

            // Initialize audio system
            if (_audioManager == null)
            {
                GameObject audioMgr = new GameObject("AudioManager");
                _audioManager = audioMgr.AddComponent<AudioManager>();
            }
            yield return new WaitForEndOfFrame();

            // Initialize weather system
            if (_weatherManager == null)
            {
                GameObject weatherMgr = new GameObject("WeatherManager");
                _weatherManager = weatherMgr.AddComponent<WeatherEffectsManager>();
            }
            yield return new WaitForEndOfFrame();

            // Start background music
            if (_audioManager)
            {
                _audioManager.PlayMusic("menu");
            }

            LogDebug("All game systems initialized successfully!");
        }

        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[GameSystems] {message}");
            }
        }

        // Public API for external access
        public void ChangeWeather(WeatherType weather)
        {
            if (_weatherManager)
                _weatherManager.SetWeather(weather);
        }

        public void PlayAudio(string category, string sfx = null)
        {
            if (_audioManager)
            {
                if (!string.IsNullOrEmpty(sfx))
                    _audioManager.PlaySFX(sfx);
                else
                    _audioManager.PlayMusic(category);
            }
        }

        public void SetQualityLevel(int level)
        {
            if (_mobileOptimizer)
                _mobileOptimizer.SetQualityLevel(level);
        }
    }

    // ===== PERFORMANCE MONITOR =====
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Monitoring")]
        [SerializeField] private bool _showFPS = true;
        [SerializeField] private bool _showMemory = true;
        [SerializeField] private float _updateInterval = 1f;

        private float _fps;
        private float _memory;
        private float _lastUpdateTime;

        private void Update()
        {
            if (Time.time - _lastUpdateTime >= _updateInterval)
            {
                _fps = 1f / Time.deltaTime;
                _memory = System.GC.GetTotalMemory(false) / 1024f / 1024f; // MB
                _lastUpdateTime = Time.time;
            }
        }

        private void OnGUI()
        {
            if (!Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 10, 200, 100));
            GUILayout.Label($"Red Bull Racing - Debug", GUI.skin.box);
            
            if (_showFPS)
                GUILayout.Label($"FPS: {_fps:F1}");
            
            if (_showMemory)
                GUILayout.Label($"Memory: {_memory:F1} MB");
                
            GUILayout.EndArea();
        }
    }
}