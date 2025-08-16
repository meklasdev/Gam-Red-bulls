using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtremeRacing.Vehicles;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Gameplay.F1
{
    [Serializable]
    public class F1Driver
    {
        public string name;
        public VehicleController vehicle;
        public int position;
        public int lapsCompleted;
        public float totalTime;
        public float lastLapTime;
        public float bestLapTime = float.MaxValue;
        public bool isInPitLane;
        public TyreCompound currentTyres = TyreCompound.Medium;
        public float tyreWear; // 0-1
        public float fuel; // 0-100
        public bool drsEnabled;
        public bool drsAvailable;
    }

    public enum TyreCompound
    {
        Soft,   // Czerwone - szybkie, szybko się zużywają
        Medium, // Żółte - średnie
        Hard,   // Białe - wolne, długo wytrzymują
        Wet     // Niebieskie - na mokro
    }

    public enum RaceState
    {
        Qualifying,
        Formation,
        Racing,
        SafetyCar,
        Finished
    }

    public class F1RaceManager : MonoBehaviour
    {
        [Header("Race Configuration")]
        [SerializeField] private int _totalLaps = 50;
        [SerializeField] private float _raceDistanceKm = 250f;
        [SerializeField] private int _maxDrivers = 20;
        [SerializeField] private bool _enableDRS = true;
        [SerializeField] private bool _enablePitStops = true;

        [Header("DRS Zones")]
        [SerializeField] private Transform[] _drsZones;
        [SerializeField] private float _drsActivationDistance = 1.0f; // sekundy za liderem

        [Header("Pit Lane")]
        [SerializeField] private Transform _pitLaneEntry;
        [SerializeField] private Transform _pitLaneExit;
        [SerializeField] private Transform[] _pitStops;
        [SerializeField] private float _pitLaneSpeedLimit = 80f; // km/h

        [Header("Weather")]
        [SerializeField] private bool _dynamicWeather = true;
        [SerializeField] private float _rainProbability = 0.2f;

        private List<F1Driver> _drivers = new List<F1Driver>();
        private RaceState _currentState = RaceState.Qualifying;
        private float _raceStartTime;
        private int _currentLap = 1;
        private bool _isRaining = false;
        private F1Driver _playerDriver;

        // Events
        public event Action<F1Driver> OnDriverFinishedLap;
        public event Action<F1Driver> OnDriverPitStop;
        public event Action<F1Driver> OnDRSActivated;
        public event Action<RaceState> OnRaceStateChanged;

        private void Start()
        {
            InitializeRace();
        }

        private void Update()
        {
            switch (_currentState)
            {
                case RaceState.Qualifying:
                    UpdateQualifying();
                    break;
                case RaceState.Racing:
                    UpdateRace();
                    break;
                case RaceState.SafetyCar:
                    UpdateSafetyCar();
                    break;
            }

            UpdateDRS();
            UpdateWeather();
        }

        private void InitializeRace()
        {
            // Znajdź wszystkie pojazdy F1
            VehicleController[] vehicles = FindObjectsOfType<VehicleController>();
            
            foreach (var vehicle in vehicles)
            {
                if (vehicle.spec != null && vehicle.spec.vehicleType == VehicleType.F1)
                {
                    F1Driver driver = new F1Driver
                    {
                        name = $"Driver {_drivers.Count + 1}",
                        vehicle = vehicle,
                        position = _drivers.Count + 1,
                        fuel = 100f,
                        tyreWear = 0f
                    };

                    // Sprawdź czy to gracz
                    if (vehicle.GetComponent<ExtremeRacing.Multiplayer.NetworkVehicleSync>()?.IsOwner == true || 
                        vehicle.CompareTag("Player"))
                    {
                        _playerDriver = driver;
                        driver.name = "Player";
                    }

                    _drivers.Add(driver);
                }
            }

            // Sortuj według pozycji startowej (można dodać kwalifikacje)
            _drivers.Sort((a, b) => a.position.CompareTo(b.position));

            Debug.Log($"[F1RaceManager] Initialized race with {_drivers.Count} drivers");
            
            StartQualifying();
        }

        #region Qualifying

        private void StartQualifying()
        {
            _currentState = RaceState.Qualifying;
            OnRaceStateChanged?.Invoke(_currentState);

            Debug.Log("[F1] Qualifying started - 15 minutes session");
            
            // Qualifying trwa 15 minut (skrócone dla gry)
            Invoke(nameof(EndQualifying), 180f); // 3 minuty w grze
        }

        private void UpdateQualifying()
        {
            // W kwalifikacjach każdy jeździ na swój czas
            foreach (var driver in _drivers)
            {
                UpdateDriverQualifying(driver);
            }
        }

        private void UpdateDriverQualifying(F1Driver driver)
        {
            // Symuluj qualifying laps dla AI
            if (driver != _playerDriver && UnityEngine.Random.Range(0f, 1f) < 0.01f)
            {
                float qualifyingTime = UnityEngine.Random.Range(65f, 75f); // 1:05 - 1:15
                if (qualifyingTime < driver.bestLapTime)
                {
                    driver.bestLapTime = qualifyingTime;
                    Debug.Log($"[F1] {driver.name} set new qualifying time: {FormatTime(qualifyingTime)}");
                }
            }
        }

        private void EndQualifying()
        {
            // Sortuj według najlepszych czasów
            _drivers.Sort((a, b) => a.bestLapTime.CompareTo(b.bestLapTime));
            
            for (int i = 0; i < _drivers.Count; i++)
            {
                _drivers[i].position = i + 1;
                Debug.Log($"[F1] Grid Position {i + 1}: {_drivers[i].name} - {FormatTime(_drivers[i].bestLapTime)}");
            }

            StartFormationLap();
        }

        #endregion

        #region Race

        private void StartFormationLap()
        {
            _currentState = RaceState.Formation;
            OnRaceStateChanged?.Invoke(_currentState);

            Debug.Log("[F1] Formation lap started");
            
            // Formation lap trwa 2 minuty
            Invoke(nameof(StartRace), 30f);
        }

        private void StartRace()
        {
            _currentState = RaceState.Racing;
            _raceStartTime = Time.time;
            OnRaceStateChanged?.Invoke(_currentState);

            Debug.Log("[F1] RACE STARTED!");
            
            // Włącz DRS po pierwszym okrążeniu
            if (_enableDRS)
            {
                Invoke(nameof(EnableDRS), 120f);
            }
        }

        private void UpdateRace()
        {
            foreach (var driver in _drivers)
            {
                UpdateDriverRace(driver);
                UpdateTyreWear(driver);
                UpdateFuelConsumption(driver);
                CheckPitStopNeeded(driver);
            }

            UpdatePositions();
            CheckRaceFinished();
        }

        private void UpdateDriverRace(F1Driver driver)
        {
            if (driver.vehicle == null) return;

            // Sprawdź czy ukończył okrążenie (prosty checkpoint system)
            CheckLapCompletion(driver);
            
            // Ograniczenie prędkości w pit lane
            if (driver.isInPitLane)
            {
                LimitPitLaneSpeed(driver);
            }
        }

        private void CheckLapCompletion(F1Driver driver)
        {
            // Uproszczone - sprawdź czy przejechał wystarczająco daleko
            float distanceTraveled = Vector3.Distance(driver.vehicle.transform.position, Vector3.zero);
            
            // Symuluj ukończenie okrążenia co jakiś czas
            if (Time.time - driver.totalTime > 90f) // ~1.5 minuty na okrążenie
            {
                CompleteLap(driver);
            }
        }

        private void CompleteLap(F1Driver driver)
        {
            driver.lapsCompleted++;
            driver.lastLapTime = Time.time - driver.totalTime;
            driver.totalTime = Time.time - _raceStartTime;

            if (driver.lastLapTime < driver.bestLapTime)
            {
                driver.bestLapTime = driver.lastLapTime;
                Debug.Log($"[F1] {driver.name} set new best lap: {FormatTime(driver.bestLapTime)}");
            }

            OnDriverFinishedLap?.Invoke(driver);
            
            Debug.Log($"[F1] {driver.name} completed lap {driver.lapsCompleted}/{_totalLaps} - {FormatTime(driver.lastLapTime)}");
        }

        #endregion

        #region Pit Stops

        private void CheckPitStopNeeded(F1Driver driver)
        {
            if (!_enablePitStops || driver.isInPitLane) return;

            // AI decyduje o pit stopie na podstawie strategii
            if (driver != _playerDriver)
            {
                bool needsPit = driver.tyreWear > 0.8f || driver.fuel < 20f || 
                              (_isRaining && driver.currentTyres != TyreCompound.Wet);
                
                if (needsPit && UnityEngine.Random.Range(0f, 1f) < 0.1f)
                {
                    EnterPitLane(driver);
                }
            }
        }

        public void EnterPitLane(F1Driver driver)
        {
            if (driver.isInPitLane) return;

            driver.isInPitLane = true;
            StartCoroutine(PitStopSequence(driver));
            
            Debug.Log($"[F1] {driver.name} entered pit lane");
        }

        private IEnumerator PitStopSequence(F1Driver driver)
        {
            // Jedź do pit stopu
            yield return new WaitForSeconds(5f);

            // Wykonaj pit stop
            float pitTime = PerformPitStop(driver);
            yield return new WaitForSeconds(pitTime);

            // Wyjdź z pit lane
            driver.isInPitLane = false;
            
            OnDriverPitStop?.Invoke(driver);
            Debug.Log($"[F1] {driver.name} completed pit stop in {pitTime:F1} seconds");
        }

        private float PerformPitStop(F1Driver driver)
        {
            float baseTime = 3.0f; // Podstawowy czas pit stopu
            
            // Zmień opony
            TyreCompound newTyres = SelectBestTyres();
            driver.currentTyres = newTyres;
            driver.tyreWear = 0f;
            
            // Zatankuj
            driver.fuel = 100f;
            
            // Dodaj losowy element (błędy mechaników)
            float randomFactor = UnityEngine.Random.Range(0.8f, 1.5f);
            
            return baseTime * randomFactor;
        }

        private TyreCompound SelectBestTyres()
        {
            if (_isRaining) return TyreCompound.Wet;
            
            // Prosta strategia - wybierz compound na podstawie stanu wyścigu
            float raceProgress = (float)_currentLap / _totalLaps;
            
            if (raceProgress < 0.3f) return TyreCompound.Soft;
            if (raceProgress < 0.7f) return TyreCompound.Medium;
            return TyreCompound.Hard;
        }

        private void LimitPitLaneSpeed(F1Driver driver)
        {
            var vehicle = driver.vehicle;
            if (vehicle != null)
            {
                float currentSpeed = vehicle.GetSpeedKmh();
                if (currentSpeed > _pitLaneSpeedLimit)
                {
                    // Dodaj karę za przekroczenie prędkości w pit lane
                    Debug.LogWarning($"[F1] {driver.name} exceeded pit lane speed limit!");
                }
            }
        }

        #endregion

        #region DRS System

        private void EnableDRS()
        {
            foreach (var driver in _drivers)
            {
                driver.drsAvailable = true;
            }
            Debug.Log("[F1] DRS now available");
        }

        private void UpdateDRS()
        {
            if (!_enableDRS) return;

            foreach (var driver in _drivers)
            {
                UpdateDriverDRS(driver);
            }
        }

        private void UpdateDriverDRS(F1Driver driver)
        {
            if (!driver.drsAvailable || driver.vehicle == null) return;

            // Sprawdź czy jest w DRS zone
            bool inDRSZone = IsInDRSZone(driver.vehicle.transform.position);
            
            // Sprawdź czy może używać DRS (jest blisko za innym kierowcą)
            bool canUseDRS = inDRSZone && IsWithinDRSActivationDistance(driver);

            if (canUseDRS && !driver.drsEnabled)
            {
                ActivateDRS(driver);
            }
            else if (!inDRSZone && driver.drsEnabled)
            {
                DeactivateDRS(driver);
            }
        }

        private bool IsInDRSZone(Vector3 position)
        {
            foreach (var zone in _drsZones)
            {
                if (Vector3.Distance(position, zone.position) < 100f) // 100m strefa
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsWithinDRSActivationDistance(F1Driver driver)
        {
            // Znajdź kierowcę przed sobą
            F1Driver driverAhead = GetDriverAhead(driver);
            if (driverAhead == null) return false;

            // Sprawdź dystans (uproszczone)
            float distance = Vector3.Distance(driver.vehicle.transform.position, 
                                            driverAhead.vehicle.transform.position);
            
            return distance < 50f; // 50 metrów = ~1 sekunda różnicy
        }

        private F1Driver GetDriverAhead(F1Driver driver)
        {
            int currentPos = driver.position;
            if (currentPos == 1) return null; // Lider nie ma nikogo przed sobą

            return _drivers.Find(d => d.position == currentPos - 1);
        }

        private void ActivateDRS(F1Driver driver)
        {
            driver.drsEnabled = true;
            
            // Zwiększ prędkość maksymalną (symulacja DRS)
            if (driver.vehicle.spec != null)
            {
                // Tymczasowo zwiększ max speed o 10 km/h
                driver.vehicle.spec.maxSpeedKmh += 10f;
            }

            OnDRSActivated?.Invoke(driver);
            Debug.Log($"[F1] {driver.name} activated DRS");
        }

        private void DeactivateDRS(F1Driver driver)
        {
            driver.drsEnabled = false;
            
            // Przywróć normalną prędkość
            if (driver.vehicle.spec != null)
            {
                driver.vehicle.spec.maxSpeedKmh -= 10f;
            }

            Debug.Log($"[F1] {driver.name} deactivated DRS");
        }

        #endregion

        #region Race Management

        private void UpdateTyreWear(F1Driver driver)
        {
            if (driver.vehicle == null) return;

            float wearRate = GetTyreWearRate(driver.currentTyres);
            float speedFactor = driver.vehicle.GetSpeedKmh() / 200f; // Szybciej = więcej zużycia
            
            driver.tyreWear += wearRate * speedFactor * Time.deltaTime;
            driver.tyreWear = Mathf.Clamp01(driver.tyreWear);

            // Wpływ zużycia na performance
            if (driver.tyreWear > 0.7f)
            {
                // Zmniejsz grip przy zużytych oponach
                var vehicleSpec = driver.vehicle.spec;
                if (vehicleSpec != null)
                {
                    float gripPenalty = (driver.tyreWear - 0.7f) * 0.5f;
                    // vehicleSpec.grip = Mathf.Max(0.5f, 1f - gripPenalty);
                }
            }
        }

        private float GetTyreWearRate(TyreCompound compound)
        {
            switch (compound)
            {
                case TyreCompound.Soft: return 0.02f;   // Szybko się zużywają
                case TyreCompound.Medium: return 0.01f; // Średnie zużycie
                case TyreCompound.Hard: return 0.005f;  // Wolno się zużywają
                case TyreCompound.Wet: return 0.03f;    // Na suchej nawierzchni bardzo szybko
                default: return 0.01f;
            }
        }

        private void UpdateFuelConsumption(F1Driver driver)
        {
            if (driver.vehicle == null) return;

            float consumptionRate = 0.05f; // 5% na okrążenie
            float speedFactor = driver.vehicle.GetSpeedKmh() / 200f;
            
            driver.fuel -= consumptionRate * speedFactor * Time.deltaTime;
            driver.fuel = Mathf.Clamp(driver.fuel, 0f, 100f);

            // Mniej paliwa = lżejszy samochód = szybciej
            if (driver.vehicle.spec != null)
            {
                float weightReduction = (100f - driver.fuel) * 0.001f;
                // Symulacja: mniej paliwa = lepsze osiągi
            }
        }

        private void UpdatePositions()
        {
            // Sortuj kierowców według okrążeń i czasu
            _drivers.Sort((a, b) => {
                if (a.lapsCompleted != b.lapsCompleted)
                    return b.lapsCompleted.CompareTo(a.lapsCompleted);
                return a.totalTime.CompareTo(b.totalTime);
            });

            // Aktualizuj pozycje
            for (int i = 0; i < _drivers.Count; i++)
            {
                _drivers[i].position = i + 1;
            }
        }

        private void CheckRaceFinished()
        {
            // Sprawdź czy lider ukończył wszystkie okrążenia
            if (_drivers[0].lapsCompleted >= _totalLaps)
            {
                FinishRace();
            }
        }

        private void FinishRace()
        {
            _currentState = RaceState.Finished;
            OnRaceStateChanged?.Invoke(_currentState);

            Debug.Log("[F1] RACE FINISHED!");
            
            // Pokaż wyniki
            DisplayResults();
        }

        private void DisplayResults()
        {
            Debug.Log("=== F1 RACE RESULTS ===");
            for (int i = 0; i < _drivers.Count; i++)
            {
                var driver = _drivers[i];
                Debug.Log($"{i + 1}. {driver.name} - {driver.lapsCompleted} laps - {FormatTime(driver.totalTime)} - Best: {FormatTime(driver.bestLapTime)}");
            }
        }

        #endregion

        #region Weather

        private void UpdateWeather()
        {
            if (!_dynamicWeather) return;

            // Prosta symulacja pogody
            if (UnityEngine.Random.Range(0f, 1f) < _rainProbability * Time.deltaTime)
            {
                if (!_isRaining)
                {
                    StartRain();
                }
            }
            else if (_isRaining && UnityEngine.Random.Range(0f, 1f) < 0.01f)
            {
                StopRain();
            }
        }

        private void StartRain()
        {
            _isRaining = true;
            Debug.Log("[F1] Rain started!");
            
            // Wszystkie pojazdy tracą grip
            foreach (var driver in _drivers)
            {
                if (driver.currentTyres != TyreCompound.Wet)
                {
                    // Zmniejsz grip na mokrej nawierzchni
                    if (driver.vehicle.spec != null)
                    {
                        // driver.vehicle.spec.grip *= 0.7f;
                    }
                }
            }
        }

        private void StopRain()
        {
            _isRaining = false;
            Debug.Log("[F1] Rain stopped!");
            
            // Przywróć normalny grip
            foreach (var driver in _drivers)
            {
                if (driver.vehicle.spec != null)
                {
                    // driver.vehicle.spec.grip = 1f;
                }
            }
        }

        #endregion

        #region Safety Car

        private void UpdateSafetyCar()
        {
            // Safety Car logic - uproszczone
            // Po jakimś czasie wróć do normalnego wyścigu
            if (UnityEngine.Random.Range(0f, 1f) < 0.01f)
            {
                EndSafetyCar();
            }
        }

        public void DeploySafetyCar()
        {
            _currentState = RaceState.SafetyCar;
            OnRaceStateChanged?.Invoke(_currentState);
            
            Debug.Log("[F1] Safety Car deployed!");
            
            // Wszyscy kierowcy jadą wolniej
            foreach (var driver in _drivers)
            {
                if (driver.vehicle.spec != null)
                {
                    // Ograniczenie prędkości pod Safety Car
                }
            }
        }

        private void EndSafetyCar()
        {
            _currentState = RaceState.Racing;
            OnRaceStateChanged?.Invoke(_currentState);
            
            Debug.Log("[F1] Safety Car returns to pits - RACING RESUMED!");
        }

        #endregion

        #region Public API

        public void PlayerEnterPitLane()
        {
            if (_playerDriver != null)
            {
                EnterPitLane(_playerDriver);
            }
        }

        public void PlayerActivateDRS()
        {
            if (_playerDriver != null && _playerDriver.drsAvailable)
            {
                if (IsInDRSZone(_playerDriver.vehicle.transform.position) && 
                    IsWithinDRSActivationDistance(_playerDriver))
                {
                    ActivateDRS(_playerDriver);
                }
            }
        }

        public F1Driver GetPlayerDriver() => _playerDriver;
        public List<F1Driver> GetAllDrivers() => new List<F1Driver>(_drivers);
        public RaceState GetCurrentState() => _currentState;
        public int GetCurrentLap() => _currentLap;
        public bool IsRaining() => _isRaining;

        #endregion

        #region Utilities

        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            float seconds = timeInSeconds % 60f;
            return $"{minutes:00}:{seconds:00.000}";
        }

        #endregion

        // Debug metody
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void OnDrawGizmosSelected()
        {
            // Rysuj DRS zones
            if (_drsZones != null)
            {
                Gizmos.color = Color.green;
                foreach (var zone in _drsZones)
                {
                    if (zone != null)
                        Gizmos.DrawWireSphere(zone.position, 100f);
                }
            }

            // Rysuj pit lane
            if (_pitLaneEntry != null && _pitLaneExit != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_pitLaneEntry.position, _pitLaneExit.position);
                
                if (_pitStops != null)
                {
                    foreach (var pitStop in _pitStops)
                    {
                        if (pitStop != null)
                            Gizmos.DrawWireCube(pitStop.position, Vector3.one * 5f);
                    }
                }
            }
        }
    }
}