using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRacing.Gameplay.Missions
{
    [Serializable]
    public class Mission
    {
        public string id;
        public string title;
        public string description;
        public MissionType type;
        public string regionId;
        public VehicleType requiredVehicle;
        public int rewardCredits;
        public int rewardExperience;
        public MissionObjective[] objectives;
        public bool isCompleted;
        public bool isUnlocked = true;
        public float bestTime = float.MaxValue;
        public int bestScore = 0;
    }

    [Serializable]
    public class MissionObjective
    {
        public string description;
        public ObjectiveType type;
        public float targetValue;
        public float currentValue;
        public bool isCompleted;
    }

    public enum MissionType
    {
        Race,
        TimeAttack,
        Stunt,
        Collect,
        Survival,
        Drift,
        Jump
    }

    public enum ObjectiveType
    {
        ReachSpeed,
        CompleteInTime,
        CollectItems,
        PerformStunts,
        SurviveTime,
        ReachCheckpoint,
        MaintainDrift,
        JumpDistance
    }

    public enum VehicleType
    {
        Any,
        Supercar,
        F1,
        Rally,
        Bike,
        Motorcycle,
        Gokart
    }

    [CreateAssetMenu(fileName = "MissionDatabase", menuName = "Red Bull Racing/Mission Database")]
    public class MissionDatabase : ScriptableObject
    {
        [Header("Mission Configuration")]
        public List<Mission> missions = new List<Mission>();

        [Header("Auto-Generate")]
        [SerializeField] private bool _autoGenerateOnStart = true;

        private void OnEnable()
        {
            if (_autoGenerateOnStart && missions.Count == 0)
            {
                GenerateEssentialMissions();
            }
        }

        public void GenerateEssentialMissions()
        {
            missions.Clear();

            // === GÓRSKI SZCZYT (5 misji) ===
            
            // 1. Downhill básico
            missions.Add(new Mission
            {
                id = "mountain_downhill_basic",
                title = "Pierwszy Zjazd",
                description = "Ukończ trasę downhill w Górskim Szczycie",
                type = MissionType.Race,
                regionId = "GorskiSzczyt",
                requiredVehicle = VehicleType.Bike,
                rewardCredits = 500,
                rewardExperience = 100,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Ukończ trasę w czasie 3:00", type = ObjectiveType.CompleteInTime, targetValue = 180f },
                    new MissionObjective { description = "Nie przewróć się więcej niż 2 razy", type = ObjectiveType.SurviveTime, targetValue = 2f }
                }
            });

            // 2. Motocross jumps
            missions.Add(new Mission
            {
                id = "mountain_motocross_jumps",
                title = "Skoki Motocross",
                description = "Wykonaj 5 udanych skoków na trasie motocross",
                type = MissionType.Jump,
                regionId = "GorskiSzczyt",
                requiredVehicle = VehicleType.Motorcycle,
                rewardCredits = 750,
                rewardExperience = 150,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Wykonaj 5 skoków", type = ObjectiveType.PerformStunts, targetValue = 5f },
                    new MissionObjective { description = "Skacz na minimum 20 metrów", type = ObjectiveType.JumpDistance, targetValue = 20f }
                }
            });

            // 3. Endurance mountain
            missions.Add(new Mission
            {
                id = "mountain_endurance",
                title = "Górska Wytrzymałość",
                description = "Przejedź 10 km w górach bez zatankowania",
                type = MissionType.Survival,
                regionId = "GorskiSzczyt",
                requiredVehicle = VehicleType.Rally,
                rewardCredits = 1000,
                rewardExperience = 200,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Przejedź 10 km", type = ObjectiveType.SurviveTime, targetValue = 10000f },
                    new MissionObjective { description = "Nie zostań bez paliwa", type = ObjectiveType.SurviveTime, targetValue = 1f }
                }
            });

            // 4. Collect mountain crates
            missions.Add(new Mission
            {
                id = "mountain_collect_crates",
                title = "Górskie Skarby",
                description = "Znajdź wszystkie 8 skrzynek w jaskiniach górskich",
                type = MissionType.Collect,
                regionId = "GorskiSzczyt",
                requiredVehicle = VehicleType.Any,
                rewardCredits = 800,
                rewardExperience = 120,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Zbierz 8 skrzynek", type = ObjectiveType.CollectItems, targetValue = 8f }
                }
            });

            // 5. Weather challenge
            missions.Add(new Mission
            {
                id = "mountain_storm_race",
                title = "Burza w Górach",
                description = "Wygraj wyścig podczas burzy śnieżnej",
                type = MissionType.Race,
                regionId = "GorskiSzczyt",
                requiredVehicle = VehicleType.Rally,
                rewardCredits = 1200,
                rewardExperience = 250,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Zajmij 1. miejsce", type = ObjectiveType.ReachCheckpoint, targetValue = 1f },
                    new MissionObjective { description = "Wyścig podczas złej pogody", type = ObjectiveType.SurviveTime, targetValue = 1f }
                }
            });

            // === PUSTYNNY KANION (4 misje) ===

            // 6. Rally básico
            missions.Add(new Mission
            {
                id = "desert_rally_basic",
                title = "Pustynny Rally",
                description = "Ukończ pierwszy etap WRC w kanionach",
                type = MissionType.Race,
                regionId = "PustynnyKanion",
                requiredVehicle = VehicleType.Rally,
                rewardCredits = 900,
                rewardExperience = 180,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Ukończ etap w czasie 4:30", type = ObjectiveType.CompleteInTime, targetValue = 270f },
                    new MissionObjective { description = "Osiągnij średnią 60 km/h", type = ObjectiveType.ReachSpeed, targetValue = 60f }
                }
            });

            // 7. Desert drift
            missions.Add(new Mission
            {
                id = "desert_drift_master",
                title = "Mistrz Driftu",
                description = "Utrzymaj drift przez 500 metrów na piasku",
                type = MissionType.Drift,
                regionId = "PustynnyKanion",
                requiredVehicle = VehicleType.Supercar,
                rewardCredits = 1500,
                rewardExperience = 300,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Drift przez 500m", type = ObjectiveType.MaintainDrift, targetValue = 500f },
                    new MissionObjective { description = "Zdobądź 10,000 punktów", type = ObjectiveType.PerformStunts, targetValue = 10000f }
                }
            });

            // 8. Canyon jump
            missions.Add(new Mission
            {
                id = "desert_canyon_jump",
                title = "Skok przez Kanion",
                description = "Przeskocz kanion na motocyklu",
                type = MissionType.Jump,
                regionId = "PustynnyKanion",
                requiredVehicle = VehicleType.Motorcycle,
                rewardCredits = 2000,
                rewardExperience = 400,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Przeskocz kanion (50m)", type = ObjectiveType.JumpDistance, targetValue = 50f },
                    new MissionObjective { description = "Wyląduj bezpiecznie", type = ObjectiveType.SurviveTime, targetValue = 1f }
                }
            });

            // 9. Oasis discovery
            missions.Add(new Mission
            {
                id = "desert_find_oasis",
                title = "Ukryta Oaza",
                description = "Znajdź wszystkie 3 ukryte oazy na pustyni",
                type = MissionType.Collect,
                regionId = "PustynnyKanion",
                requiredVehicle = VehicleType.Any,
                rewardCredits = 1200,
                rewardExperience = 200,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Znajdź 3 oazy", type = ObjectiveType.CollectItems, targetValue = 3f }
                }
            });

            // === MIASTO NOCY (4 misje) ===

            // 10. Night street race
            missions.Add(new Mission
            {
                id = "city_night_race",
                title = "Nocny Wyścig",
                description = "Wygraj wyścig uliczny supercarami",
                type = MissionType.Race,
                regionId = "MiastoNocy",
                requiredVehicle = VehicleType.Supercar,
                rewardCredits = 1800,
                rewardExperience = 250,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Zajmij 1. miejsce", type = ObjectiveType.ReachCheckpoint, targetValue = 1f },
                    new MissionObjective { description = "Osiągnij 200 km/h", type = ObjectiveType.ReachSpeed, targetValue = 200f }
                }
            });

            // 11. Rooftop parkour
            missions.Add(new Mission
            {
                id = "city_rooftop_parkour",
                title = "Parkour po Dachach",
                description = "Przejedź trasę po dachach bez spadnięcia",
                type = MissionType.Stunt,
                regionId = "MiastoNocy",
                requiredVehicle = VehicleType.Bike,
                rewardCredits = 2500,
                rewardExperience = 400,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Ukończ trasę bez spadnięcia", type = ObjectiveType.SurviveTime, targetValue = 1f },
                    new MissionObjective { description = "Wykonaj 10 skoków", type = ObjectiveType.PerformStunts, targetValue = 10f }
                }
            });

            // 12. Drift contest
            missions.Add(new Mission
            {
                id = "city_drift_contest",
                title = "Konkurs Driftu",
                description = "Zdobądź ocenę 9.0+ od jury",
                type = MissionType.Drift,
                regionId = "MiastoNocy",
                requiredVehicle = VehicleType.Supercar,
                rewardCredits = 3000,
                rewardExperience = 500,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Zdobądź ocenę 9.0+", type = ObjectiveType.PerformStunts, targetValue = 9000f },
                    new MissionObjective { description = "Utrzymaj combo przez 30s", type = ObjectiveType.MaintainDrift, targetValue = 30f }
                }
            });

            // 13. Urban legends
            missions.Add(new Mission
            {
                id = "city_urban_legends",
                title = "Miejskie Legendy",
                description = "Pokonaj 5 legendarnych kierowców",
                type = MissionType.Race,
                regionId = "MiastoNocy",
                requiredVehicle = VehicleType.Any,
                rewardCredits = 2200,
                rewardExperience = 350,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Pokonaj 5 legend", type = ObjectiveType.ReachCheckpoint, targetValue = 5f }
                }
            });

            // === PORT WYŚCIGOWY (3 misje) ===

            // 14. Gokart championship
            missions.Add(new Mission
            {
                id = "port_gokart_championship",
                title = "Mistrzostwa Gokartów",
                description = "Wygraj 3 wyścigi gokartami z rzędu",
                type = MissionType.Race,
                regionId = "PortWyscigowy",
                requiredVehicle = VehicleType.Gokart,
                rewardCredits = 1500,
                rewardExperience = 200,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Wygraj 3 wyścigi", type = ObjectiveType.ReachCheckpoint, targetValue = 3f },
                    new MissionObjective { description = "Najlepszy czas okrążenia", type = ObjectiveType.CompleteInTime, targetValue = 45f }
                }
            });

            // 15. Container slalom
            missions.Add(new Mission
            {
                id = "port_container_slalom",
                title = "Slalom Kontenerowy",
                description = "Przejedź między kontenerami na czas",
                type = MissionType.TimeAttack,
                regionId = "PortWyscigowy",
                requiredVehicle = VehicleType.Rally,
                rewardCredits = 1200,
                rewardExperience = 150,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Ukończ w czasie 2:30", type = ObjectiveType.CompleteInTime, targetValue = 150f },
                    new MissionObjective { description = "Nie uderz w kontenery", type = ObjectiveType.SurviveTime, targetValue = 1f }
                }
            });

            // 16. Harbor sprint
            missions.Add(new Mission
            {
                id = "port_harbor_sprint",
                title = "Sprint Portowy",
                description = "Najszybszy czas na trasie wśród dźwigów",
                type = MissionType.TimeAttack,
                regionId = "PortWyscigowy",
                requiredVehicle = VehicleType.Supercar,
                rewardCredits = 1800,
                rewardExperience = 220,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Pobij rekord 1:45", type = ObjectiveType.CompleteInTime, targetValue = 105f }
                }
            });

            // === TOR MISTRZÓW (4 misje) ===

            // 17. First F1 race
            missions.Add(new Mission
            {
                id = "track_first_f1",
                title = "Pierwszy Wyścig F1",
                description = "Ukończ swój pierwszy wyścig Formuły 1",
                type = MissionType.Race,
                regionId = "TorMistrzow",
                requiredVehicle = VehicleType.F1,
                rewardCredits = 5000,
                rewardExperience = 1000,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Ukończ wyścig", type = ObjectiveType.ReachCheckpoint, targetValue = 1f },
                    new MissionObjective { description = "Skończ w pierwszej 10", type = ObjectiveType.ReachCheckpoint, targetValue = 10f }
                }
            });

            // 18. Perfect pitstop
            missions.Add(new Mission
            {
                id = "track_perfect_pitstop",
                title = "Idealny Pitstop",
                description = "Wykonaj pitstop w czasie poniżej 3 sekund",
                type = MissionType.TimeAttack,
                regionId = "TorMistrzow",
                requiredVehicle = VehicleType.F1,
                rewardCredits = 3000,
                rewardExperience = 400,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Pitstop < 3 sekundy", type = ObjectiveType.CompleteInTime, targetValue = 3f }
                }
            });

            // 19. DRS master
            missions.Add(new Mission
            {
                id = "track_drs_master",
                title = "Mistrz DRS",
                description = "Wyprzedź 5 przeciwników używając DRS",
                type = MissionType.Race,
                regionId = "TorMistrzow",
                requiredVehicle = VehicleType.F1,
                rewardCredits = 4000,
                rewardExperience = 600,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Wyprzedź 5 rywali z DRS", type = ObjectiveType.PerformStunts, targetValue = 5f }
                }
            });

            // 20. Grand Prix champion
            missions.Add(new Mission
            {
                id = "track_grand_prix_champion",
                title = "Mistrz Grand Prix",
                description = "Wygraj swoje pierwsze Grand Prix",
                type = MissionType.Race,
                regionId = "TorMistrzow",
                requiredVehicle = VehicleType.F1,
                rewardCredits = 10000,
                rewardExperience = 2000,
                objectives = new MissionObjective[]
                {
                    new MissionObjective { description = "Wygraj Grand Prix", type = ObjectiveType.ReachCheckpoint, targetValue = 1f },
                    new MissionObjective { description = "Najszybsze okrążenie", type = ObjectiveType.CompleteInTime, targetValue = 90f }
                }
            });

            Debug.Log($"[MissionDatabase] Generated {missions.Count} essential missions");
        }

        public Mission GetMissionById(string id)
        {
            return missions.Find(m => m.id == id);
        }

        public List<Mission> GetMissionsByRegion(string regionId)
        {
            return missions.FindAll(m => m.regionId == regionId);
        }

        public List<Mission> GetMissionsByType(MissionType type)
        {
            return missions.FindAll(m => m.type == type);
        }

        public List<Mission> GetAvailableMissions()
        {
            return missions.FindAll(m => m.isUnlocked && !m.isCompleted);
        }

        public List<Mission> GetCompletedMissions()
        {
            return missions.FindAll(m => m.isCompleted);
        }

        public void CompleteMission(string missionId, float time = 0f, int score = 0)
        {
            var mission = GetMissionById(missionId);
            if (mission != null)
            {
                mission.isCompleted = true;
                if (time > 0 && time < mission.bestTime)
                    mission.bestTime = time;
                if (score > mission.bestScore)
                    mission.bestScore = score;

                // Unlock next missions based on completion
                UnlockNextMissions(mission);
                
                Debug.Log($"[MissionDatabase] Mission completed: {mission.title}");
            }
        }

        private void UnlockNextMissions(Mission completedMission)
        {
            // Simple progression system - unlock missions based on completed ones
            switch (completedMission.id)
            {
                case "mountain_downhill_basic":
                    GetMissionById("mountain_motocross_jumps")?.SetUnlocked(true);
                    break;
                case "desert_rally_basic":
                    GetMissionById("desert_drift_master")?.SetUnlocked(true);
                    break;
                case "city_night_race":
                    GetMissionById("city_rooftop_parkour")?.SetUnlocked(true);
                    break;
                case "track_first_f1":
                    GetMissionById("track_perfect_pitstop")?.SetUnlocked(true);
                    break;
            }
        }

        public int GetTotalRewardCredits()
        {
            return missions.FindAll(m => m.isCompleted).Sum(m => m.rewardCredits);
        }

        public int GetTotalExperience()
        {
            return missions.FindAll(m => m.isCompleted).Sum(m => m.rewardExperience);
        }

        public float GetCompletionPercentage()
        {
            if (missions.Count == 0) return 0f;
            return (float)missions.Count(m => m.isCompleted) / missions.Count * 100f;
        }

        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Red Bull Racing/Generate Mission Database")]
        public static void CreateMissionDatabase()
        {
            var asset = CreateInstance<MissionDatabase>();
            asset.GenerateEssentialMissions();
            
            UnityEditor.AssetDatabase.CreateAsset(asset, "Assets/ScriptableObjects/MissionDatabase.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            
            Debug.Log("Mission Database created with 20 essential missions!");
        }
        #endif
    }

    // Extension methods
    public static class MissionExtensions
    {
        public static void SetUnlocked(this Mission mission, bool unlocked)
        {
            if (mission != null)
                mission.isUnlocked = unlocked;
        }

        public static int Sum<T>(this List<T> source, System.Func<T, int> selector)
        {
            int sum = 0;
            foreach (var item in source)
                sum += selector(item);
            return sum;
        }

        public static int Count<T>(this List<T> source, System.Func<T, bool> predicate)
        {
            int count = 0;
            foreach (var item in source)
                if (predicate(item)) count++;
            return count;
        }
    }
}