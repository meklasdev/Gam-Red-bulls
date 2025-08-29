extends Node
class_name GameManager

## Główny menedżer gry - zarządza stanami i przejściami między scenami
## Singleton dostępny globalnie jako GameManager

signal game_state_changed(new_state: GameState)
signal scene_loaded(scene_path: String)

enum GameState {
	MAIN_MENU,      ## Menu główne
	PLAYING,        ## Aktywna rozgrywka
	PAUSED,         ## Gra wstrzymana
	LOADING,        ## Ładowanie sceny
	MULTIPLAYER     ## Tryb multiplayer
}

var current_state := GameState.MAIN_MENU
var previous_state := GameState.MAIN_MENU
var current_scene_path := ""

# Dane gracza
var player_data := {
	"name": "Gracz",
	"money": 10000,
	"reputation": 0,
	"level": 1,
	"current_vehicle": "car_basic"
}

# Ustawienia gry
var game_settings := {
	"master_volume": 1.0,
	"sfx_volume": 1.0,
	"music_volume": 0.8,
	"graphics_quality": "medium",
	"vsync": true,
	"fullscreen": false
}

func _ready() -> void:
	print("GameManager: Inicjalizacja...")
	
	# Wczytaj ustawienia z pliku
	_load_game_settings()
	_load_player_data()
	
	# Ustaw początkowy stan
	_change_state(GameState.LOADING)
	
	# Wczytaj menu główne
	await SceneLoader.load_scene("res://scenes/main_menu.tscn")
	current_scene_path = "res://scenes/main_menu.tscn"
	_change_state(GameState.MAIN_MENU)
	
	print("GameManager: Gotowy!")

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("pause"):
		match current_state:
			GameState.PLAYING:
				pause_game()
			GameState.PAUSED:
				resume_game()

## Rozpoczyna grę w wybranym regionie
func start_game(region_path: String) -> void:
	print("GameManager: Rozpoczynanie gry w regionie: ", region_path)
	_change_state(GameState.LOADING)
	
	await SceneLoader.load_scene(region_path)
	current_scene_path = region_path
	
	# Inicjalizuj systemy sceny
	_init_scene_systems()
	
	_change_state(GameState.PLAYING)

## Rozpoczyna tryb multiplayer
func start_multiplayer(scene_path: String = "res://scenes/regions/multiplayer_test.tscn") -> void:
	print("GameManager: Rozpoczynanie multiplayer: ", scene_path)
	_change_state(GameState.LOADING)
	
	await SceneLoader.load_scene(scene_path)
	current_scene_path = scene_path
	
	_init_scene_systems()
	_change_state(GameState.MULTIPLAYER)

## Wstrzymuje grę
func pause_game() -> void:
	if current_state not in [GameState.PLAYING, GameState.MULTIPLAYER]:
		return
	
	print("GameManager: Wstrzymywanie gry")
	previous_state = current_state
	_change_state(GameState.PAUSED)
	Engine.time_scale = 0.0

## Wznawia grę
func resume_game() -> void:
	if current_state != GameState.PAUSED:
		return
	
	print("GameManager: Wznawianie gry")
	Engine.time_scale = 1.0
	_change_state(previous_state)

## Powraca do menu głównego
func return_to_main_menu() -> void:
	print("GameManager: Powrót do menu głównego")
	_change_state(GameState.LOADING)
	
	# Zatrzymaj wszystkie systemy
	if Engine.time_scale != 1.0:
		Engine.time_scale = 1.0
	
	await SceneLoader.load_scene("res://scenes/main_menu.tscn")
	current_scene_path = "res://scenes/main_menu.tscn"
	_change_state(GameState.MAIN_MENU)

## Wyjście z gry
func quit_game() -> void:
	print("GameManager: Zapisywanie i zamykanie gry")
	_save_game_settings()
	_save_player_data()
	get_tree().quit()

## Zmienia stan gry i emituje sygnał
func _change_state(new_state: GameState) -> void:
	if current_state == new_state:
		return
	
	var old_state = current_state
	current_state = new_state
	
	print("GameManager: Zmiana stanu z ", GameState.keys()[old_state], " na ", GameState.keys()[new_state])
	game_state_changed.emit(new_state)

## Inicjalizuje systemy sceny po załadowaniu
func _init_scene_systems() -> void:
	print("GameManager: Inicjalizacja systemów sceny")
	
	# Zastosuj pogodę natychmiast
	if WeatherManager:
		WeatherManager.apply_current_weather_immediate()
	
	# Ustaw porę dnia
	if TimeOfDayManager:
		TimeOfDayManager.apply_now()
	
	# Przygotuj misje sceny
	if MissionSystem:
		MissionSystem.prepare_scene_missions()
	
	# Inicjalizuj kontrakty
	if ContractSystem:
		ContractSystem.init_contracts()
	
	scene_loaded.emit(current_scene_path)

## Ładuje ustawienia gry z pliku
func _load_game_settings() -> void:
	var config = ConfigFile.new()
	var err = config.load("user://settings.cfg")
	
	if err != OK:
		print("GameManager: Brak pliku ustawień, używanie domyślnych")
		return
	
	for key in game_settings.keys():
		game_settings[key] = config.get_value("game", key, game_settings[key])
	
	print("GameManager: Ustawienia załadowane")

## Zapisuje ustawienia gry do pliku
func _save_game_settings() -> void:
	var config = ConfigFile.new()
	
	for key in game_settings.keys():
		config.set_value("game", key, game_settings[key])
	
	var err = config.save("user://settings.cfg")
	if err == OK:
		print("GameManager: Ustawienia zapisane")
	else:
		print("GameManager: Błąd zapisu ustawień: ", err)

## Ładuje dane gracza z pliku
func _load_player_data() -> void:
	var config = ConfigFile.new()
	var err = config.load("user://player_data.cfg")
	
	if err != OK:
		print("GameManager: Brak pliku danych gracza, używanie domyślnych")
		return
	
	for key in player_data.keys():
		player_data[key] = config.get_value("player", key, player_data[key])
	
	print("GameManager: Dane gracza załadowane")

## Zapisuje dane gracza do pliku
func _save_player_data() -> void:
	var config = ConfigFile.new()
	
	for key in player_data.keys():
		config.set_value("player", key, player_data[key])
	
	var err = config.save("user://player_data.cfg")
	if err == OK:
		print("GameManager: Dane gracza zapisane")
	else:
		print("GameManager: Błąd zapisu danych gracza: ", err)

## Zwraca aktualny stan gry
func get_current_state() -> GameState:
	return current_state

## Sprawdza czy gra jest w trakcie rozgrywki
func is_playing() -> bool:
	return current_state in [GameState.PLAYING, GameState.MULTIPLAYER]

## Sprawdza czy gra jest wstrzymana
func is_paused() -> bool:
	return current_state == GameState.PAUSED