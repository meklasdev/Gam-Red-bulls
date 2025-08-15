# Gam-Red-bulls


Twoim zadaniem jest stworzenie kompletnej gry mobilnej w Unity 2023 LTS (C#) z Universal Render Pipeline (URP Mobile) na Android i iOS, na podstawie poniższego Game Design Document. Gra ma działać w 60 FPS, wspierać sterowanie dotykowe i multiplayer, i zawierać wszystkie mechaniki, regiony, systemy i tryby opisane w dokumencie.

====================
GAME DESIGN DOCUMENT
====================

Tytuł: Red Bull Game: Extreme Racing
Gatunek: Wyścigi w otwartym świecie 3D, z elementami realistycznej fizyki i arcade.
Tryby: Singleplayer i Multiplayer (P2P i lokalny).

Opcje startowe:
1. Pełny tryb – wszystko odblokowane.
2. Kariera – start od starego roweru, rozwój poprzez wyścigi, wyzwania i reputację.
3. Sandbox – wszystkie regiony dostępne, ale część pojazdów i misji wymaga progresji.

REGIONY I ICH AKTYWNOŚCI:
1. Górski Szczyt:
   - Downhill na rowerze: strome ścieżki, drewniane mostki, kamieniste przesmyki, czasówki.
   - Motocross: naturalne rampy, balans ciałem, trudne zakręty.
   - Endurance: długie trasy między szczytami, zarządzanie paliwem i energią.
   - Pogoda dynamiczna, wpływ na przyczepność.
   - Skrzynie górskie: w jaskiniach lub na mostach, zawartość – części i malowania.

2. Pustynny Kanion:
   - Rally po piachu: WRC, proste odcinki, ostre zakręty, kamienie na drodze.
   - Motocross w kanionach: wąskie ścieżki, wysokie skoki, wymagane precyzyjne lądowanie.
   - Drift na piasku: eventy na otwartych przestrzeniach, punktacja za poślizg.
   - Ukryte oazy: skrzynki z oponami driftowymi i pustynnymi malowaniami.

3. Miasto Nocy:
   - Drift contest: ocena stylu i płynności przez jury.
   - Sprinty supercarami: proste i kręte odcinki GT i Hypercar.
   - Skróty po dachach: rampy, parkingi, dachy.
   - Miejskie legendy: NPC z tajnymi wyzwaniami i skrzynkami.

4. Port Wyścigowy:
   - Gokarty: krótkie, kręte tory.
   - Rajdy po nadbrzeżu: trasy asfaltowo-terenowe wśród kontenerów i dźwigów.
   - Sprinty na czas: wąskie alejki portu.
   - Ukryte skrzynie: w kontenerach i na dźwigach.

5. Tor Mistrzów:
   - Wyścigi F1: pełne okrążenia, pitstopy, strategie.
   - Time trial: ghost mode.
   - Strefy DRS: wyprzedzanie przy zbliżeniu.
   - Grand Prix: kwalifikacje + seria wyścigów.

SYSTEMY:
- Popularność i kontrakty: punkty za nagrania z eventów, followersi, kontrakty z Red Bull i sponsorami.
- Specjalizacje: sport motorowy (rajdy, supercary, F1) lub rowery/motocykle (BMX, downhill, motocross, stunt).
- Eventy dronowe: loty przez pierścienie, nagrywanie trików.
- Akrobacje motocross: triki (backflip, frontflip, can-can).
- Zarządzanie garażem i ekipą: zakup garaży, mechanicy, menedżer eventów, operator drona.
- DJ Red Bull Live i 64 Bars: miksowanie muzyki, freestyle z NPC, pokazy kaskaderskie.
- Sezony: wiosna (błoto), lato (nocne eventy), jesień (burze piaskowe), zima (śnieg i lód).
- Tajne misje: ukryte lokacje, skrzynki z częściami, unikalny pojazd po zebraniu kompletu.
- Nielegalne części: czarny rynek, mody z ryzykiem wykrycia.
- Legendarne lokacje: rekordy prędkości, X-Fighters Arena, skoki z klifów.
- Tryb fabularny: rywale, cutscenki, wybory moralne, wielki finał.
- Tryb przetrwania: proceduralne trasy, ograniczone paliwo, dynamiczne przeszkody.
- System zakładów: obstawianie wyników, ryzyko i nagrody.

PRZYKŁADY MISJI:
- Wygraj rajd podczas burzy piaskowej.
- Skok motocrossem przez kanion.
- Drift 500 m bez przerwy.
- Znajdź wszystkie skrzynki w regionie.
- Wykonaj serię backflipów w stunt parku.

==============================
UNITY MOBILE IMPLEMENTATION PLAN
==============================

Silnik: Unity 2023 LTS, URP Mobile.
Platformy: Android 8.0+, iOS 13+.
Cel FPS: 60.
Sterowanie: dotykowe przyciski + gesty.
Optymalizacja: LOD, occlusion culling, batching, GPU instancing, asset bundles.

Struktura projektu:
- Scenes: MainMenu, Region_GorskiSzczyt, Region_PustynnyKanion, Region_MiastoNocy, Region_PortWyscigowy, Region_TorMistrzow.
- Scripts: GameManager.cs, InputManager.cs, VehicleController.cs, AIController.cs, WeatherManager.cs, TimeOfDayManager.cs, MissionSystem.cs, ContractSystem.cs, DriftScoring.cs, LootSpawner.cs.
- UI: Canvas główny, HUD, minimapa, menu kontraktów, garaż, sklep.
- Prefabs: Pojazdy, NPC, skrzynki, rampy, efekty cząsteczkowe.

Systemy w Unity:
- Vehicle physics: WheelCollider dla aut/motocykli, prosta fizyka dla rowerów.
- Pogoda: burze piaskowe, deszcz, śnieg, wpływ na fizykę.
- Cykl dnia/nocy: TimeOfDayManager z kontrolą słońca i świateł.
- Misje: MissionSystem z definicją warunków, nagród i triggerów.
- Multiplayer: Netcode for GameObjects, synchronizacja pozycji pojazdów.

========================
DEVELOPMENT INSTRUCTIONS
========================
1. Zaimportuj URP Mobile i skonfiguruj jako domyślny pipeline.
2. Utwórz podstawowe sceny regionów z placeholder terenem i obiektami.
3. Zaimplementuj GameManager do ładowania scen i zarządzania stanem gry.
4. Dodaj InputManager obsługujący sterowanie dotykowe.
5. Zaimplementuj VehicleController z parametrami w ScriptableObject dla każdego typu pojazdu.
6. Stwórz WeatherManager i TimeOfDayManager.
7. Zaimplementuj MissionSystem, LootSpawner, ContractSystem.
8. Utwórz UI w Canvas z przyciskami i panelami.
9. Zaimplementuj wszystkie przykładowe misje i systemy.
10. Dodaj optymalizację pod mobile.
11. Na końcu wypisz listę wszystkich plików .cs, scen i prefabów.

Wygeneruj cały kod C# z podziałem na pliki i komentarzami w języku polskim.
