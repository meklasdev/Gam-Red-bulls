# Gam Red Bulls - Godot Edition

Kompletny projekt gry wyścigowej w Godot 4.3, zkonwertowany z konceptu Unity na idiomy Godot.

## 🎮 Opis Gry

Gam Red Bulls to wieloplatformowa gra wyścigowa oferująca:
- **5 różnych regionów** z unikalnymi wyzwaniami
- **3 typy pojazdów**: samochody, motocykle, rowery
- **System misji i kontraktów** z progresją gracza
- **Multiplayer P2P i klient-serwer** dla maksymalnie 8 graczy
- **Zaawansowany system driftu** z punktacją i combo
- **System stuntów** z różnymi akrobacjami
- **Dynamiczna pogoda i pora dnia**
- **Optymalizacje mobilne** z automatycznym dostosowywaniem jakości

## 🚀 Funkcje

### Pojazdy
- **Samochody**: VehicleBody3D z realistyczną fizyką, nitro, drift
- **Motocykle**: RigidBody3D z balansowaniem, wheelie, stoppie
- **Rowery**: CharacterBody3D z systemem wytrzymałości i trików

### Regiony
- **Górski Szczyt**: Górskie trasy z ostrymi zakrętami
- **Pustynny Kanion**: Idealne do driftowania i długich prostych
- **Miasto Nocy**: Nocne wyścigi z ograniczoną widocznością
- **Port Wyścigowy**: Wyścigi między kontenerami i żurawiami
- **Tor Mistrzów**: Profesjonalny tor z DRS i checkpointami

### Systemy Gry
- **Drift Scoring**: Punktacja za kąt, prędkość, czas i kombinacje
- **Race Manager**: Obsługa wyścigów, okrążeń, czasów
- **Stunt System**: Wykrywanie i punktowanie akrobacji
- **Mission System**: Zadania z celami i nagrodami
- **Contract System**: System popularności i specjalizacji

## 🛠️ Struktura Projektu

```
/
├── project.godot              # Konfiguracja główna
├── export_presets.cfg         # Profile eksportu Android/iOS
├── icon.svg                   # Ikona aplikacji
├── scenes/
│   ├── main_menu.tscn         # Menu główne
│   ├── regions/               # Sceny regionów
│   ├── ui/                    # Interfejsy użytkownika
│   └── vehicles/              # Pojazdy sieciowe
├── scripts/
│   ├── core/                  # Systemy podstawowe
│   ├── vehicles/              # Kontrolery pojazdów
│   ├── systems/               # Systemy gry
│   ├── ui/                    # Skrypty UI
│   ├── race/                  # Systemy wyścigów
│   ├── multiplayer/           # Sieć
│   └── optimization/          # Optymalizacje
├── data/
│   ├── missions/              # Definicje misji (.tres)
│   ├── vehicles/              # Statystyki pojazdów
│   └── contracts/             # Dane kontraktów
└── assets/                    # Zasoby graficzne
```

## 🎯 Autoloady (Singletons)

Projekt wykorzystuje następujące globalne systemy:
- **GameManager**: Zarządzanie stanami gry i scenami
- **InputManager**: Obsługa wejścia (klawiatura, pad, dotyk)
- **MultiplayerManager**: Połączenia sieciowe ENet
- **TimeOfDayManager**: Cykl dzień/noc z oświetleniem
- **WeatherManager**: Dynamiczna pogoda wpływająca na rozgrywkę
- **MissionSystem**: System zadań i celów
- **ContractSystem**: Popularność, specjalizacje, nielegalne części

## 🎮 Sterowanie

### Klawiatura
- **WSAD**: Ruch i kierowanie
- **Spacja**: Hamulec ręczny/skok
- **N**: Nitro
- **Shift**: Drift
- **E/Q**: Zmiana stuntów
- **ESC**: Pauza

### Gamepad
- **Lewy analog**: Kierowanie i przyspieszanie
- **Prawy analog**: Kamera (tryb drona)
- **Triggery**: Gaz/hamulec
- **Przyciski**: Nitro, hamulec ręczny, stunty

### Mobile
- **Wirtualny joystick**: Sterowanie
- **Przyciski na ekranie**: Akcje specjalne
- **Automatyczne wykrywanie**: Przełączanie między trybami

## 🌐 Multiplayer

### Tryby Sieciowe
- **P2P Host**: Hostowanie gry peer-to-peer
- **P2P Client**: Dołączanie do gry P2P
- **Dedicated Server**: Serwer dedykowany
- **Client**: Klient serwera dedykowanego

### Synchronizacja
- **MultiplayerSynchronizer**: Automatyczna synchronizacja pozycji
- **RPC**: Efekty, input, stany specjalne
- **Interpolacja**: Płynny ruch zdalnych graczy
- **Predykcja**: Kompensacja opóźnień sieciowych

## 📱 Optymalizacje Mobile

### Automatyczne Dostosowywanie
- **Monitoring FPS**: Ciągłe sprawdzanie wydajności
- **Poziomy jakości**: LOW/MEDIUM/HIGH/ULTRA
- **Dynamiczne zmiany**: Automatyczne dostosowywanie do sprzętu

### Optymalizacje
- **LOD System**: Poziomy detali na podstawie odległości
- **Particle Scaling**: Skalowanie systemów cząsteczek
- **Shadow Control**: Dynamiczne włączanie/wyłączanie cieni
- **Render Scale**: Skalowanie rozdzielczości renderowania
- **Texture Quality**: Dostosowywanie jakości tekstur

## 🚀 Uruchamianie

### Wymagania
- **Godot 4.3** lub nowszy
- **Android SDK 28+** (dla eksportu Android)
- **iOS 13+** (dla eksportu iOS)
- **OpenGL ES 3.0** lub Vulkan

### Pierwsze Uruchomienie
1. Otwórz projekt w Godot 4.3
2. Sprawdź czy wszystkie AutoLoady są poprawnie skonfigurowane
3. Uruchom scenę `main_menu.tscn`
4. Wybierz region i rozpocznij grę

### Eksport
1. **Android**: Skonfiguruj Android SDK w ustawieniach edytora
2. **iOS**: Wymagane Xcode i certyfikaty Apple Developer
3. **Desktop**: Eksport bez dodatkowej konfiguracji

## 🎨 Customizacja

### Dodawanie Nowych Pojazdów
1. Utwórz nowy plik `.tres` w `data/vehicles/`
2. Skonfiguruj statystyki używając `VehicleStats`
3. Dodaj model 3D i skrypty kontrolera

### Nowe Regiony
1. Utwórz scenę w `scenes/regions/`
2. Dodaj spawn pointy, checkpointy, DRS strefy
3. Skonfiguruj oświetlenie i środowisko
4. Dodaj do listy regionów w menu

### Misje
1. Utwórz plik `.tres` w `data/missions/`
2. Użyj klasy `Mission` jako script
3. Zdefiniuj cele, nagrody, wymagania
4. MissionSystem automatycznie załaduje nowe misje

## 🔧 Rozwiązywanie Problemów

### Częste Problemy
- **Brak dźwięku**: Sprawdź konfigurację AudioStreamPlayer3D
- **Problemy z fizyką**: Upewnij się że warstwy kolizji są poprawne
- **Multiplayer nie działa**: Sprawdź konfigurację firewalla
- **Niskie FPS**: Włącz automatyczne dostosowywanie jakości

### Debugowanie
- **FPS Counter**: Włączony domyślnie w debug builds
- **Network Stats**: Dostępne w MultiplayerManager
- **Performance Monitor**: MobileOptimization.get_performance_stats()

## 📄 Licencja

Projekt utworzony jako konwersja konceptu Unity na Godot 4.3.
Kod dostępny na licencji MIT.

## 🤝 Wkład

Projekt gotowy do uruchomienia bez dodatkowego edytowania kodu.
Wszystkie systemy są w pełni funkcjonalne i zoptymalizowane.

---

**Wersja**: 1.0  
**Engine**: Godot 4.3  
**Target**: Android/iOS 60 FPS  
**Utworzono**: 2024