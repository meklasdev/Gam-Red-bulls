using System;
using System.Collections.Generic;
using UnityEngine;
using ExtremeRacing.Gameplay.Missions;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Gameplay
{
    [Serializable]
    public class PlayerProfile
    {
        public string playerName = "Player";
        public int level = 1;
        public int experience = 0;
        public int credits = 5000; // Starting money
        public float reputation = 0f;
        public CareerSpecialization specialization = CareerSpecialization.None;
        public List<string> ownedVehicles = new List<string>();
        public List<string> unlockedRegions = new List<string> { "GorskiSzczyt" }; // Start with mountain region
        public Dictionary<string, int> sponsorshipLevels = new Dictionary<string, int>();
    }

    public enum CareerSpecialization
    {
        None,
        MotorSport,    // Rally, Supercars, F1
        ExtremeSports  // BMX, Downhill, Motocross, Stunts
    }

    public class CareerManager : MonoBehaviour
    {
        [Header("Career Configuration")]
        [SerializeField] private MissionDatabase _missionDatabase;
        [SerializeField] private PlayerProfile _playerProfile = new PlayerProfile();
        
        [Header("Progression")]
        [SerializeField] private int[] _levelExperienceRequirements = { 0, 100, 250, 500, 1000, 1500, 2500, 4000, 6000, 10000, 15000 };
        [SerializeField] private float _maxReputation = 10000f;

        [Header("Sponsorship")]
        [SerializeField] private List<Sponsor> _availableSponsors = new List<Sponsor>();

        // Events
        public static event Action<int> OnLevelUp;
        public static event Action<int> OnCreditsChanged;
        public static event Action<float> OnReputationChanged;
        public static event Action<string> OnVehicleUnlocked;
        public static event Action<string> OnRegionUnlocked;

        private static CareerManager _instance;
        public static CareerManager Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCareer();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeCareer()
        {
            // Initialize sponsors
            InitializeSponsors();
            
            // Load player data (in real game would load from save file)
            LoadPlayerProfile();
            
            Debug.Log($"[CareerManager] Career initialized for {_playerProfile.playerName}");
        }

        private void InitializeSponsors()
        {
            if (_availableSponsors.Count == 0)
            {
                _availableSponsors.AddRange(new[]
                {
                    new Sponsor { name = "Red Bull", specialization = CareerSpecialization.ExtremeSports, reputationRequired = 0, contractValue = 1000 },
                    new Sponsor { name = "Monster Energy", specialization = CareerSpecialization.ExtremeSports, reputationRequired = 500, contractValue = 1500 },
                    new Sponsor { name = "BMW Motorsport", specialization = CareerSpecialization.MotorSport, reputationRequired = 1000, contractValue = 2500 },
                    new Sponsor { name = "Ferrari F1", specialization = CareerSpecialization.MotorSport, reputationRequired = 2000, contractValue = 5000 },
                    new Sponsor { name = "GoPro", specialization = CareerSpecialization.ExtremeSports, reputationRequired = 300, contractValue = 800 },
                    new Sponsor { name = "Pirelli", specialization = CareerSpecialization.MotorSport, reputationRequired = 1500, contractValue = 3000 }
                });
            }
        }

        #region Experience & Leveling

        public void AddExperience(int amount)
        {
            _playerProfile.experience += amount;
            CheckLevelUp();
            Debug.Log($"[Career] +{amount} XP (Total: {_playerProfile.experience})");
        }

        private void CheckLevelUp()
        {
            int newLevel = CalculateLevel(_playerProfile.experience);
            if (newLevel > _playerProfile.level)
            {
                int oldLevel = _playerProfile.level;
                _playerProfile.level = newLevel;
                OnLevelUp?.Invoke(newLevel);
                
                // Level up rewards
                int creditsReward = newLevel * 500;
                AddCredits(creditsReward);
                
                // Unlock new content based on level
                UnlockContentForLevel(newLevel);
                
                Debug.Log($"[Career] LEVEL UP! {oldLevel} → {newLevel} (+{creditsReward} credits)");
            }
        }

        private int CalculateLevel(int experience)
        {
            for (int i = _levelExperienceRequirements.Length - 1; i >= 0; i--)
            {
                if (experience >= _levelExperienceRequirements[i])
                    return i + 1;
            }
            return 1;
        }

        private void UnlockContentForLevel(int level)
        {
            switch (level)
            {
                case 2:
                    UnlockRegion("PustynnyKanion");
                    break;
                case 3:
                    UnlockVehicle("Rally_Car_Basic");
                    break;
                case 5:
                    UnlockRegion("MiastoNocy");
                    break;
                case 7:
                    UnlockRegion("PortWyscigowy");
                    break;
                case 10:
                    UnlockRegion("TorMistrzow");
                    UnlockVehicle("F1_Car_Basic");
                    break;
            }
        }

        #endregion

        #region Credits & Economy

        public void AddCredits(int amount)
        {
            _playerProfile.credits += amount;
            OnCreditsChanged?.Invoke(_playerProfile.credits);
            Debug.Log($"[Career] +{amount} credits (Total: {_playerProfile.credits})");
        }

        public bool SpendCredits(int amount)
        {
            if (_playerProfile.credits >= amount)
            {
                _playerProfile.credits -= amount;
                OnCreditsChanged?.Invoke(_playerProfile.credits);
                Debug.Log($"[Career] -{amount} credits (Remaining: {_playerProfile.credits})");
                return true;
            }
            return false;
        }

        public bool CanAfford(int cost) => _playerProfile.credits >= cost;

        #endregion

        #region Reputation & Sponsorship

        public void AddReputation(float amount)
        {
            _playerProfile.reputation = Mathf.Min(_playerProfile.reputation + amount, _maxReputation);
            OnReputationChanged?.Invoke(_playerProfile.reputation);
            
            // Check for new sponsorship opportunities
            CheckNewSponsors();
            
            Debug.Log($"[Career] +{amount} reputation (Total: {_playerProfile.reputation:F0})");
        }

        private void CheckNewSponsors()
        {
            foreach (var sponsor in _availableSponsors)
            {
                if (_playerProfile.reputation >= sponsor.reputationRequired && 
                    !_playerProfile.sponsorshipLevels.ContainsKey(sponsor.name))
                {
                    OfferSponsorshipContract(sponsor);
                }
            }
        }

        private void OfferSponsorshipContract(Sponsor sponsor)
        {
            Debug.Log($"[Career] New sponsorship offer from {sponsor.name}! Contract value: {sponsor.contractValue} credits");
            // In real game, show UI popup for contract offer
        }

        public void AcceptSponsorshipContract(string sponsorName)
        {
            var sponsor = _availableSponsors.Find(s => s.name == sponsorName);
            if (sponsor != null)
            {
                _playerProfile.sponsorshipLevels[sponsorName] = 1;
                AddCredits(sponsor.contractValue);
                Debug.Log($"[Career] Sponsorship contract signed with {sponsorName}!");
            }
        }

        #endregion

        #region Vehicle & Region Management

        public void UnlockVehicle(string vehicleId)
        {
            if (!_playerProfile.ownedVehicles.Contains(vehicleId))
            {
                _playerProfile.ownedVehicles.Add(vehicleId);
                OnVehicleUnlocked?.Invoke(vehicleId);
                Debug.Log($"[Career] Vehicle unlocked: {vehicleId}");
            }
        }

        public void UnlockRegion(string regionId)
        {
            if (!_playerProfile.unlockedRegions.Contains(regionId))
            {
                _playerProfile.unlockedRegions.Add(regionId);
                OnRegionUnlocked?.Invoke(regionId);
                Debug.Log($"[Career] Region unlocked: {regionId}");
            }
        }

        public bool IsVehicleOwned(string vehicleId) => _playerProfile.ownedVehicles.Contains(vehicleId);
        public bool IsRegionUnlocked(string regionId) => _playerProfile.unlockedRegions.Contains(regionId);

        #endregion

        #region Mission Integration

        public void OnMissionCompleted(string missionId, float completionTime, int score)
        {
            if (_missionDatabase == null) return;

            var mission = _missionDatabase.GetMissionById(missionId);
            if (mission != null && !mission.isCompleted)
            {
                // Complete mission in database
                _missionDatabase.CompleteMission(missionId, completionTime, score);
                
                // Reward experience and credits
                AddExperience(mission.rewardExperience);
                AddCredits(mission.rewardCredits);
                
                // Reputation based on mission difficulty and performance
                float reputationGain = CalculateReputationGain(mission, completionTime, score);
                AddReputation(reputationGain);
                
                Debug.Log($"[Career] Mission completed: {mission.title}");
            }
        }

        private float CalculateReputationGain(Mission mission, float time, int score)
        {
            float baseReputation = mission.rewardExperience * 0.5f;
            
            // Performance bonus
            float performanceMultiplier = 1f;
            if (time > 0 && time < mission.bestTime)
                performanceMultiplier += 0.5f; // 50% bonus for new best time
            if (score > mission.bestScore)
                performanceMultiplier += 0.3f; // 30% bonus for new best score
                
            return baseReputation * performanceMultiplier;
        }

        #endregion

        #region Specialization System

        public void ChooseSpecialization(CareerSpecialization specialization)
        {
            if (_playerProfile.specialization == CareerSpecialization.None)
            {
                _playerProfile.specialization = specialization;
                
                // Give starting bonus for chosen specialization
                switch (specialization)
                {
                    case CareerSpecialization.MotorSport:
                        UnlockVehicle("Rally_Car_Starter");
                        AddReputation(100f);
                        Debug.Log("[Career] Motor Sport specialization chosen! Rally car unlocked.");
                        break;
                        
                    case CareerSpecialization.ExtremeSports:
                        UnlockVehicle("BMX_Starter");
                        AddReputation(100f);
                        Debug.Log("[Career] Extreme Sports specialization chosen! BMX bike unlocked.");
                        break;
                }
            }
        }

        #endregion

        #region Save/Load System

        private void LoadPlayerProfile()
        {
            // In real game, load from PlayerPrefs or save file
            string savedData = PlayerPrefs.GetString("CareerProfile", "");
            if (!string.IsNullOrEmpty(savedData))
            {
                try
                {
                    _playerProfile = JsonUtility.FromJson<PlayerProfile>(savedData);
                    Debug.Log("[Career] Profile loaded successfully");
                }
                catch
                {
                    Debug.LogWarning("[Career] Failed to load profile, using default");
                }
            }
        }

        public void SavePlayerProfile()
        {
            try
            {
                string jsonData = JsonUtility.ToJson(_playerProfile);
                PlayerPrefs.SetString("CareerProfile", jsonData);
                PlayerPrefs.Save();
                Debug.Log("[Career] Profile saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Career] Failed to save profile: {e.Message}");
            }
        }

        #endregion

        #region Public API

        public PlayerProfile GetPlayerProfile() => _playerProfile;
        public int GetPlayerLevel() => _playerProfile.level;
        public int GetPlayerCredits() => _playerProfile.credits;
        public float GetPlayerReputation() => _playerProfile.reputation;
        public int GetPlayerExperience() => _playerProfile.experience;
        public CareerSpecialization GetSpecialization() => _playerProfile.specialization;
        
        public int GetExperienceForNextLevel()
        {
            int currentLevel = _playerProfile.level;
            if (currentLevel < _levelExperienceRequirements.Length)
                return _levelExperienceRequirements[currentLevel] - _playerProfile.experience;
            return 0;
        }

        public float GetLevelProgress()
        {
            int currentLevel = _playerProfile.level;
            if (currentLevel >= _levelExperienceRequirements.Length) return 1f;
            
            int currentLevelXP = currentLevel > 0 ? _levelExperienceRequirements[currentLevel - 1] : 0;
            int nextLevelXP = _levelExperienceRequirements[currentLevel];
            int progressXP = _playerProfile.experience - currentLevelXP;
            int totalXPNeeded = nextLevelXP - currentLevelXP;
            
            return (float)progressXP / totalXPNeeded;
        }

        #endregion

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SavePlayerProfile();
        }

        private void OnDestroy()
        {
            SavePlayerProfile();
        }
    }

    [Serializable]
    public class Sponsor
    {
        public string name;
        public CareerSpecialization specialization;
        public float reputationRequired;
        public int contractValue;
        public string description;
    }
}