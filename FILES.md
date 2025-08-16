# Inwentarz plików projektu

## Skrypty C# (Assets/Scripts)

- Core:
  - Assets/Scripts/Core/Singleton.cs
  - Assets/Scripts/Core/SceneLoader.cs
  - Assets/Scripts/Core/GameManager.cs
  - Assets/Scripts/Core/SceneNames.cs
- Input:
  - Assets/Scripts/Input/InputManager.cs
- Vehicles:
  - Assets/Scripts/Vehicles/VehicleStats.cs
  - Assets/Scripts/Vehicles/VehicleController.cs
- AI:
  - Assets/Scripts/AI/AIController.cs
- Systems:
  - Assets/Scripts/Systems/WeatherManager.cs
  - Assets/Scripts/Systems/TimeOfDayManager.cs
  - Assets/Scripts/Systems/MissionSystem.cs
  - Assets/Scripts/Systems/ContractSystem.cs
  - Assets/Scripts/Systems/DriftScoring.cs
  - Assets/Scripts/Systems/LootSpawner.cs (z klasą LootPickup)
  - Assets/Scripts/Systems/StuntManager.cs
  - Assets/Scripts/Systems/DroneController.cs
  - Assets/Scripts/Systems/BettingSystem.cs
  - Assets/Scripts/Systems/MultiplayerManager.cs
- Race:
  - Assets/Scripts/Race/Checkpoint.cs
  - Assets/Scripts/Race/DRSZone.cs
  - Assets/Scripts/Race/GhostRecorder.cs
  - Assets/Scripts/Race/PitStop.cs
  - Assets/Scripts/Race/RaceManager.cs
  - Assets/Scripts/Race/GrandPrixManager.cs
- Stubs (kompilacja poza Unity):
  - Assets/Scripts/Stubs/UnityStubs.cs
  - Assets/Scripts/Stubs/NetcodeStubs.cs
- UI:
  - Assets/Scripts/UI/MainMenuUI.cs
  - Assets/Scripts/UI/MinimapController.cs
- Optimization:
  - Assets/Scripts/Optimization/MobileOptimizationSettings.cs

## Edytor
- Assets/Editor/ProjectBootstrap.cs — generuje brakujące sceny i podstawowe menu.

## Sceny
- Assets/Scenes/MainMenu.unity
- Assets/Scenes/Region_GorskiSzczyt.unity
- Assets/Scenes/Region_PustynnyKanion.unity
- Assets/Scenes/Region_MiastoNocy.unity
- Assets/Scenes/Region_PortWyscigowy.unity
- Assets/Scenes/Region_TorMistrzow.unity
- Assets/Scenes/README.txt — opis scen i ich przeznaczenia

## Prefaby
- Do utworzenia w edytorze (placeholdery w scenach). Brak committed prefabów.

## Zasoby zewnętrzne
- Assets/ExternalAssets/sources.md — rekomendowane, licencyjnie bezpieczne źródła modeli/tekstur/audio (CC0 itp.).

## Dokumentacja
- PROJECT_STATUS.md — bieżący status projektu i zakres brakujących elementów.
