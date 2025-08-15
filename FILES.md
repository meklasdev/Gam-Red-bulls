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
- UI:
  - Assets/Scripts/UI/MainMenuUI.cs
  - Assets/Scripts/UI/MinimapController.cs
- Optimization:
  - Assets/Scripts/Optimization/MobileOptimizationSettings.cs
- Multiplayer (placeholders pod NGO):
  - Assets/Scripts/Multiplayer/NetworkBootstrap.cs
  - Assets/Scripts/Multiplayer/NetworkVehicle.cs

## Edytor
- Assets/Editor/ProjectBootstrap.cs — generuje brakujące sceny i podstawowe menu.

## Sceny
- Assets/Scenes/README.txt — opis. Sceny tworzone automatycznie przy pierwszym otwarciu projektu:
  - MainMenu
  - Region_GorskiSzczyt
  - Region_PustynnyKanion
  - Region_MiastoNocy
  - Region_PortWyscigowy
  - Region_TorMistrzow

## Prefaby
- Do utworzenia w edytorze (placeholdery w scenach). Brak committed prefabów.

## Zasoby zewnętrzne
- Assets/ExternalAssets/sources.md — rekomendowane, licencyjnie bezpieczne źródła modeli/tekstur/audio (CC0 itp.).