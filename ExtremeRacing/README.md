# Extreme Racing (URP Mobile, Unity 2023 LTS)

Projekt szkieletowy gry open world z multiplayerem oparty o Unity 2023 LTS (URP Mobile), zgodny z GDD "Red Bull Game: Extreme Racing".

## Wymagania
- Unity 2023 LTS (np. 2023.3.x)
- Moduły platform: Android, iOS
- Universal Render Pipeline (URP)
- Netcode for GameObjects, Transport
- Input System

## Jak uruchomić
1. Otwórz folder projektu w Unity: `Open > /workspace/ExtremeRacing`
2. Pierwsze otwarcie:
   - Skrypt edytora `ProjectBootstrap` automatycznie utworzy:
     - URP Mobile (asset + renderer) i podstawi w Graphics Settings
     - Sceny: `MainMenu`, `Region_GorskiSzczyt`, `Region_PustynnyKanion`, `Region_MiastoNocy`, `Region_PortWyscigowy`, `Region_TorMistrzow`
     - Prefaby: `GameSystems`, `NetworkManager`, prototypy pojazdów, HUD, Minimap
     - ScriptableObject-y parametrów pojazdów
     - Doda sceny do Build Settings
3. Upewnij się, że `Edit > Project Settings`:
   - Player > Active Input Handling: Input System Package (lub Both)
   - Android/iOS: minimalne SDK (Android 8.0 / iOS 13.0)
   - Quality: docelowo 60 FPS (VSync off, targetFrameRate = 60)

## Struktura
- `Assets/Scripts`
  - `Managers/` GameManager, InputManager, WeatherManager, TimeOfDayManager, MissionSystem, ContractSystem, DriftScoring, LootSpawner, AssetBundleLoader
  - `Vehicles/` VehicleSpec, VehicleController, VehicleSpawner
  - `Multiplayer/` NetworkVehicleSync, SimpleLobby, RuntimeNetworkBootstrap
  - `Procedural/` ProceduralTrackGenerator, SurvivalModeManager
  - `Gameplay/` ActivityZone
  - `UI/` HUDController, MinimapController, MainMenuUI, ScoreboardUI, GarageUI, ContractsUI, ContractsMenuController, ShopUI, TouchControlsOverlay
  - `Optimization/` MobileOptimizer, LODSetup
  - `Infrastructure/` Singleton
- `Assets/Editor` – automatyczna konfiguracja URP, scen, prefabów, build skrypty, prototypy prefabów
- `Assets/Prefabs` – generowane placeholdery

## Multiplayer
- Prefab `NetworkManager` (UnityTransport) + `RuntimeNetworkBootstrap` rejestrujący prefaby z `Resources/NetworkPrefabs`
- `SimpleLobby` udostępnia Host/Join, `NetworkVehicleSync` synchronizuje pozycję/rotację pojazdu

## Build
- Android: File > Build Settings > Android > Switch Platform > Build
- iOS: File > Build Settings > iOS > Build (wymaga macOS/Xcode do kompilacji IPA)

## Assety 3D (CC0 / free)
- Kenney: https://kenney.nl/assets?category=3D
- Poly Haven: https://polyhaven.com/models
- Sketchfab (filtr CC): https://sketchfab.com/search?licenses=9c89c3f703364dfaa5b5678bfb3b1f10
- Unity Asset Store (free): https://assetstore.unity.com/top-assets/free
- Blend Swap: https://www.blendswap.com/

## Uwaga
Ten projekt to kompletny szkielet z działającymi systemami i auto-konfiguracją. Zastąp placeholdery docelowymi modelami, teksturami i trasami oraz dostrój fizykę pod docelowe urządzenia mobilne, aby utrzymać 60 FPS.