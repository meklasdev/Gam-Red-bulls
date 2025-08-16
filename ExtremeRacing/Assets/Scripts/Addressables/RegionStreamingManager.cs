using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using ExtremeRacing.Infrastructure;

namespace ExtremeRacing.Addressables
{
    [Serializable]
    public class RegionData
    {
        public string regionId;
        public string sceneName;
        public AssetReference sceneReference;
        public List<AssetReference> assetBundles = new List<AssetReference>();
        public float preloadDistance = 2000f;
        public Vector3 centerPosition;
        public float boundingRadius = 1000f;
        public bool isLoaded;
        public bool isPreloading;
    }

    public class RegionStreamingManager : Singleton<RegionStreamingManager>
    {
        [Header("Region Configuration")]
        [SerializeField] private List<RegionData> _regions = new List<RegionData>();
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _updateInterval = 1f;
        [SerializeField] private int _maxConcurrentLoads = 2;

        [Header("Memory Management")]
        [SerializeField] private float _memoryThresholdMB = 2048f;
        [SerializeField] private float _unloadDelaySeconds = 30f;

        private Dictionary<string, SceneInstance> _loadedScenes = new Dictionary<string, SceneInstance>();
        private Dictionary<string, List<AsyncOperationHandle>> _loadedAssets = new Dictionary<string, List<AsyncOperationHandle>>();
        private Queue<RegionData> _loadQueue = new Queue<RegionData>();
        private Queue<string> _unloadQueue = new Queue<string>();
        private int _currentLoadsInProgress = 0;
        private Coroutine _streamingCoroutine;

        public event Action<string> OnRegionLoaded;
        public event Action<string> OnRegionUnloaded;
        public event Action<string, float> OnRegionLoadProgress;

        protected override void Awake()
        {
            base.Awake();
            InitializeRegions();
        }

        private void Start()
        {
            if (_playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _playerTransform = player.transform;
            }

            _streamingCoroutine = StartCoroutine(StreamingUpdateLoop());
        }

        private void InitializeRegions()
        {
            // Inicjalizacja danych regionów
            if (_regions.Count == 0)
            {
                _regions.Add(new RegionData
                {
                    regionId = "GorskiSzczyt",
                    sceneName = "Region_GorskiSzczyt",
                    centerPosition = new Vector3(0, 500, 0),
                    boundingRadius = 1500f,
                    preloadDistance = 2000f
                });

                _regions.Add(new RegionData
                {
                    regionId = "PustynnyKanion",
                    sceneName = "Region_PustynnyKanion",
                    centerPosition = new Vector3(5000, 100, 0),
                    boundingRadius = 2000f,
                    preloadDistance = 2500f
                });

                _regions.Add(new RegionData
                {
                    regionId = "MiastoNocy",
                    sceneName = "Region_MiastoNocy",
                    centerPosition = new Vector3(0, 50, 5000),
                    boundingRadius = 1200f,
                    preloadDistance = 1800f
                });

                _regions.Add(new RegionData
                {
                    regionId = "PortWyscigowy",
                    sceneName = "Region_PortWyscigowy",
                    centerPosition = new Vector3(-3000, 10, 2000),
                    boundingRadius = 800f,
                    preloadDistance = 1500f
                });

                _regions.Add(new RegionData
                {
                    regionId = "TorMistrzow",
                    sceneName = "Region_TorMistrzow",
                    centerPosition = new Vector3(2000, 50, -3000),
                    boundingRadius = 1000f,
                    preloadDistance = 1800f
                });
            }
        }

        private IEnumerator StreamingUpdateLoop()
        {
            while (true)
            {
                if (_playerTransform != null)
                {
                    UpdateRegionStreaming();
                    ProcessLoadQueue();
                    ProcessUnloadQueue();
                    CheckMemoryUsage();
                }

                yield return new WaitForSeconds(_updateInterval);
            }
        }

        private void UpdateRegionStreaming()
        {
            Vector3 playerPos = _playerTransform.position;

            foreach (var region in _regions)
            {
                float distanceToRegion = Vector3.Distance(playerPos, region.centerPosition);
                bool shouldLoad = distanceToRegion <= region.preloadDistance;
                bool shouldUnload = distanceToRegion > region.preloadDistance + 500f; // Hysteresis

                if (shouldLoad && !region.isLoaded && !region.isPreloading)
                {
                    QueueRegionLoad(region);
                }
                else if (shouldUnload && region.isLoaded)
                {
                    QueueRegionUnload(region.regionId);
                }
            }
        }

        private void QueueRegionLoad(RegionData region)
        {
            if (!_loadQueue.Contains(region))
            {
                region.isPreloading = true;
                _loadQueue.Enqueue(region);
                Debug.Log($"[RegionStreaming] Queued region for loading: {region.regionId}");
            }
        }

        private void QueueRegionUnload(string regionId)
        {
            if (!_unloadQueue.Contains(regionId))
            {
                _unloadQueue.Enqueue(regionId);
                Debug.Log($"[RegionStreaming] Queued region for unloading: {regionId}");
            }
        }

        private void ProcessLoadQueue()
        {
            while (_loadQueue.Count > 0 && _currentLoadsInProgress < _maxConcurrentLoads)
            {
                var region = _loadQueue.Dequeue();
                StartCoroutine(LoadRegionAsync(region));
            }
        }

        private void ProcessUnloadQueue()
        {
            while (_unloadQueue.Count > 0)
            {
                string regionId = _unloadQueue.Dequeue();
                StartCoroutine(UnloadRegionAsync(regionId));
            }
        }

        private IEnumerator LoadRegionAsync(RegionData region)
        {
            _currentLoadsInProgress++;
            Debug.Log($"[RegionStreaming] Starting load of region: {region.regionId}");

            try
            {
                // Ładowanie sceny additively
                var sceneHandle = Addressables.LoadSceneAsync(region.sceneName, LoadSceneMode.Additive);
                
                while (!sceneHandle.IsDone)
                {
                    OnRegionLoadProgress?.Invoke(region.regionId, sceneHandle.PercentComplete * 0.7f);
                    yield return null;
                }

                if (sceneHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedScenes[region.regionId] = sceneHandle.Result;
                    
                    // Ładowanie dodatkowych assetów
                    var assetHandles = new List<AsyncOperationHandle>();
                    float assetProgress = 0f;

                    foreach (var assetRef in region.assetBundles)
                    {
                        if (assetRef.RuntimeKeyIsValid())
                        {
                            var assetHandle = Addressables.LoadAssetAsync<GameObject>(assetRef);
                            assetHandles.Add(assetHandle);
                            yield return assetHandle;
                            
                            assetProgress += 1f / region.assetBundles.Count;
                            OnRegionLoadProgress?.Invoke(region.regionId, 0.7f + assetProgress * 0.3f);
                        }
                    }

                    _loadedAssets[region.regionId] = assetHandles;
                    region.isLoaded = true;
                    region.isPreloading = false;

                    OnRegionLoaded?.Invoke(region.regionId);
                    Debug.Log($"[RegionStreaming] Successfully loaded region: {region.regionId}");
                }
                else
                {
                    Debug.LogError($"[RegionStreaming] Failed to load region: {region.regionId}");
                    region.isPreloading = false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RegionStreaming] Exception loading region {region.regionId}: {e.Message}");
                region.isPreloading = false;
            }
            finally
            {
                _currentLoadsInProgress--;
            }
        }

        private IEnumerator UnloadRegionAsync(string regionId)
        {
            Debug.Log($"[RegionStreaming] Starting unload of region: {regionId}");

            // Delay przed unload dla lepszego UX
            yield return new WaitForSeconds(_unloadDelaySeconds);

            var region = _regions.Find(r => r.regionId == regionId);
            if (region == null || !region.isLoaded) yield break;

            try
            {
                // Unload assets
                if (_loadedAssets.ContainsKey(regionId))
                {
                    foreach (var handle in _loadedAssets[regionId])
                    {
                        if (handle.IsValid())
                            Addressables.Release(handle);
                    }
                    _loadedAssets.Remove(regionId);
                }

                // Unload scene
                if (_loadedScenes.ContainsKey(regionId))
                {
                    var sceneHandle = Addressables.UnloadSceneAsync(_loadedScenes[regionId]);
                    yield return sceneHandle;
                    _loadedScenes.Remove(regionId);
                }

                region.isLoaded = false;
                OnRegionUnloaded?.Invoke(regionId);
                Debug.Log($"[RegionStreaming] Successfully unloaded region: {regionId}");

                // Force garbage collection po unload
                System.GC.Collect();
                Resources.UnloadUnusedAssets();
            }
            catch (Exception e)
            {
                Debug.LogError($"[RegionStreaming] Exception unloading region {regionId}: {e.Message}");
            }
        }

        private void CheckMemoryUsage()
        {
            float memoryUsageMB = (System.GC.GetTotalMemory(false) / 1024f / 1024f);
            
            if (memoryUsageMB > _memoryThresholdMB)
            {
                Debug.LogWarning($"[RegionStreaming] Memory usage high: {memoryUsageMB:F1}MB. Triggering aggressive cleanup.");
                
                // Unload najdalszych regionów
                var loadedRegions = _regions.FindAll(r => r.isLoaded);
                loadedRegions.Sort((a, b) => 
                {
                    float distA = Vector3.Distance(_playerTransform.position, a.centerPosition);
                    float distB = Vector3.Distance(_playerTransform.position, b.centerPosition);
                    return distB.CompareTo(distA); // Sortuj malejąco po odległości
                });

                for (int i = 0; i < Mathf.Min(2, loadedRegions.Count); i++)
                {
                    QueueRegionUnload(loadedRegions[i].regionId);
                }
            }
        }

        public void ForceLoadRegion(string regionId)
        {
            var region = _regions.Find(r => r.regionId == regionId);
            if (region != null && !region.isLoaded && !region.isPreloading)
            {
                QueueRegionLoad(region);
            }
        }

        public void ForceUnloadRegion(string regionId)
        {
            QueueRegionUnload(regionId);
        }

        public bool IsRegionLoaded(string regionId)
        {
            var region = _regions.Find(r => r.regionId == regionId);
            return region?.isLoaded ?? false;
        }

        public float GetRegionLoadProgress(string regionId)
        {
            // Ta wartość będzie aktualizowana przez LoadRegionAsync
            return 0f; // Implementacja w razie potrzeby
        }

        private void OnDestroy()
        {
            if (_streamingCoroutine != null)
            {
                StopCoroutine(_streamingCoroutine);
            }

            // Cleanup wszystkich loaded assets
            foreach (var assetList in _loadedAssets.Values)
            {
                foreach (var handle in assetList)
                {
                    if (handle.IsValid())
                        Addressables.Release(handle);
                }
            }

            foreach (var scene in _loadedScenes.Values)
            {
                Addressables.UnloadSceneAsync(scene);
            }
        }

        // Debug methods
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugPrintStatus()
        {
            Debug.Log($"[RegionStreaming] Status - Loaded: {_loadedScenes.Count}, Queue: {_loadQueue.Count}, Loading: {_currentLoadsInProgress}");
        }
    }
}