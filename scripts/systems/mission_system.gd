extends Node
class_name MissionSystem

## System misji - zarządza zadaniami, celami i nagrodami
## Singleton dostępny globalnie jako MissionSystem

signal mission_started(mission: Mission)
signal mission_completed(mission: Mission, rewards: Dictionary)
signal mission_failed(mission: Mission)
signal objective_completed(mission: Mission, objective_index: int)
signal mission_progress_updated(mission: Mission, progress: float)

class_name Mission extends Resource

@export var id: String = ""
@export var title: String = ""
@export var description: String = ""
@export var region: String = ""
@export var difficulty: int = 1  # 1-5
@export var objectives: Array[String] = []
@export var rewards: Dictionary = {"money": 0, "reputation": 0, "unlock": ""}
@export var requirements: Dictionary = {"level": 1, "vehicle_type": ""}
@export var time_limit: float = 0.0  # 0 = bez limitu czasu
@export var is_story_mission: bool = false

# Dane stanu misji
var available_missions: Array[Mission] = []
var active_missions: Array[Mission] = []
var completed_missions: Array[String] = []  # IDs misji
var current_mission: Mission = null

# Progress aktywnych misji
var mission_progress: Dictionary = {}  # mission_id -> {"objectives": [bool], "start_time": float}

func _ready() -> void:
	print("MissionSystem: Inicjalizacja...")
	
	# Wczytaj misje z plików
	_load_missions()
	
	# Wczytaj progress z zapisu
	_load_mission_progress()
	
	print("MissionSystem: Załadowano ", available_missions.size(), " misji")

## Ładuje wszystkie misje z katalogu data/missions
func _load_missions() -> void:
	var missions_dir = "res://data/missions/"
	var dir = DirAccess.open(missions_dir)
	
	if not dir:
		print("MissionSystem: Nie można otworzyć katalogu misji")
		_create_example_missions()
		return
	
	dir.list_dir_begin()
	var file_name = dir.get_next()
	
	while file_name != "":
		if file_name.ends_with(".tres"):
			var mission_path = missions_dir + file_name
			var mission = load(mission_path) as Mission
			if mission:
				available_missions.append(mission)
				print("MissionSystem: Załadowano misję: ", mission.title)
		
		file_name = dir.get_next()
	
	dir.list_dir_end()
	
	# Jeśli brak misji, utwórz przykładowe
	if available_missions.is_empty():
		_create_example_missions()

## Tworzy przykładowe misje
func _create_example_missions() -> void:
	print("MissionSystem: Tworzenie przykładowych misji...")
	
	# Misja 1: Pierwsza jazda
	var mission1 = Mission.new()
	mission1.id = "first_drive"
	mission1.title = "Pierwsza Jazda"
	mission1.description = "Ukończ rundę po torze w Górskim Szczycie"
	mission1.region = "gorski_szczyt"
	mission1.difficulty = 1
	mission1.objectives = ["Ukończ okrążenie", "Osiągnij prędkość 100 km/h"]
	mission1.rewards = {"money": 500, "reputation": 10}
	mission1.requirements = {"level": 1}
	mission1.is_story_mission = true
	
	# Misja 2: Drift Master
	var mission2 = Mission.new()
	mission2.id = "drift_master"
	mission2.title = "Mistrz Driftu"
	mission2.description = "Zdobądź 5000 punktów za drift w Pustynnym Kanionie"
	mission2.region = "pustynny_kanion"
	mission2.difficulty = 2
	mission2.objectives = ["Zdobądź 5000 punktów za drift"]
	mission2.rewards = {"money": 1000, "reputation": 25, "unlock": "drift_tires"}
	mission2.requirements = {"level": 2}
	
	# Misja 3: Nocny Wyścig
	var mission3 = Mission.new()
	mission3.id = "night_race"
	mission3.title = "Nocny Wyścig"
	mission3.description = "Wygraj wyścig w Mieście Nocy między 22:00 a 04:00"
	mission3.region = "miasto_nocy"
	mission3.difficulty = 3
	mission3.objectives = ["Ukończ wyścig na 1. miejscu", "Wyścig musi odbywać się w nocy"]
	mission3.rewards = {"money": 2000, "reputation": 50}
	mission3.requirements = {"level": 3}
	mission3.time_limit = 300.0  # 5 minut
	
	available_missions = [mission1, mission2, mission3]

## Przygotowuje misje dla aktualnej sceny
func prepare_scene_missions() -> void:
	var current_scene = SceneLoader.get_current_scene_name()
	print("MissionSystem: Przygotowywanie misji dla sceny: ", current_scene)
	
	# Znajdź misje dla aktualnej sceny
	for mission in available_missions:
		if mission.region in current_scene or current_scene in mission.region:
			if _is_mission_available(mission):
				print("MissionSystem: Misja dostępna: ", mission.title)

## Rozpoczyna misję
func start_mission(mission_id: String) -> bool:
	var mission = _get_mission_by_id(mission_id)
	if not mission:
		print("MissionSystem: Nie znaleziono misji: ", mission_id)
		return false
	
	if not _is_mission_available(mission):
		print("MissionSystem: Misja niedostępna: ", mission.title)
		return false
	
	print("MissionSystem: Rozpoczynanie misji: ", mission.title)
	
	# Dodaj do aktywnych misji
	active_missions.append(mission)
	current_mission = mission
	
	# Inicjalizuj progress
	mission_progress[mission.id] = {
		"objectives": [],
		"start_time": Time.get_time_dict_from_system()["unix"]
	}
	
	# Wypełnij objectives false'ami
	for i in range(mission.objectives.size()):
		mission_progress[mission.id]["objectives"].append(false)
	
	mission_started.emit(mission)
	return true

## Kończy misję sukcesem
func complete_mission(mission_id: String) -> void:
	var mission = _get_active_mission_by_id(mission_id)
	if not mission:
		return
	
	print("MissionSystem: Ukończono misję: ", mission.title)
	
	# Usuń z aktywnych
	active_missions.erase(mission)
	if current_mission == mission:
		current_mission = null
	
	# Dodaj do ukończonych
	completed_missions.append(mission.id)
	
	# Przyznaj nagrody
	_award_rewards(mission.rewards)
	
	# Wyczyść progress
	mission_progress.erase(mission.id)
	
	mission_completed.emit(mission, mission.rewards)
	
	# Zapisz progress
	_save_mission_progress()

## Kończy misję niepowodzeniem
func fail_mission(mission_id: String) -> void:
	var mission = _get_active_mission_by_id(mission_id)
	if not mission:
		return
	
	print("MissionSystem: Misja nieudana: ", mission.title)
	
	# Usuń z aktywnych
	active_missions.erase(mission)
	if current_mission == mission:
		current_mission = null
	
	# Wyczyść progress
	mission_progress.erase(mission.id)
	
	mission_failed.emit(mission)

## Oznacza cel jako ukończony
func complete_objective(mission_id: String, objective_index: int) -> void:
	if not mission_id in mission_progress:
		return
	
	var progress = mission_progress[mission_id]
	if objective_index >= progress["objectives"].size():
		return
	
	if progress["objectives"][objective_index]:
		return  # Już ukończony
	
	progress["objectives"][objective_index] = true
	
	var mission = _get_active_mission_by_id(mission_id)
	if mission:
		print("MissionSystem: Ukończono cel: ", mission.objectives[objective_index])
		objective_completed.emit(mission, objective_index)
		
		# Sprawdź czy wszystkie cele ukończone
		if _all_objectives_completed(mission_id):
			complete_mission(mission_id)
		else:
			# Aktualizuj progress
			var completion_progress = _calculate_mission_progress(mission_id)
			mission_progress_updated.emit(mission, completion_progress)

## Sprawdza czy misja jest dostępna
func _is_mission_available(mission: Mission) -> bool:
	# Sprawdź czy już ukończona
	if mission.id in completed_missions:
		return false
	
	# Sprawdź czy już aktywna
	for active_mission in active_missions:
		if active_mission.id == mission.id:
			return false
	
	# Sprawdź wymagania poziomu
	if mission.requirements.has("level"):
		if GameManager.player_data.level < mission.requirements["level"]:
			return false
	
	# Sprawdź wymagania pojazdu
	if mission.requirements.has("vehicle_type") and mission.requirements["vehicle_type"] != "":
		# Tutaj można dodać logikę sprawdzania typu pojazdu
		pass
	
	return true

## Zwraca misję po ID
func _get_mission_by_id(mission_id: String) -> Mission:
	for mission in available_missions:
		if mission.id == mission_id:
			return mission
	return null

## Zwraca aktywną misję po ID
func _get_active_mission_by_id(mission_id: String) -> Mission:
	for mission in active_missions:
		if mission.id == mission_id:
			return mission
	return null

## Sprawdza czy wszystkie cele misji są ukończone
func _all_objectives_completed(mission_id: String) -> bool:
	if not mission_id in mission_progress:
		return false
	
	var objectives = mission_progress[mission_id]["objectives"]
	for completed in objectives:
		if not completed:
			return false
	
	return true

## Oblicza progress misji (0.0 - 1.0)
func _calculate_mission_progress(mission_id: String) -> float:
	if not mission_id in mission_progress:
		return 0.0
	
	var objectives = mission_progress[mission_id]["objectives"]
	if objectives.is_empty():
		return 0.0
	
	var completed_count = 0
	for completed in objectives:
		if completed:
			completed_count += 1
	
	return float(completed_count) / float(objectives.size())

## Przyznaje nagrody
func _award_rewards(rewards: Dictionary) -> void:
	if rewards.has("money"):
		GameManager.player_data.money += rewards["money"]
		print("MissionSystem: Przyznano ", rewards["money"], " pieniędzy")
	
	if rewards.has("reputation"):
		GameManager.player_data.reputation += rewards["reputation"]
		print("MissionSystem: Przyznano ", rewards["reputation"], " reputacji")
	
	if rewards.has("unlock") and rewards["unlock"] != "":
		print("MissionSystem: Odblokowano: ", rewards["unlock"])
		# Tutaj można dodać logikę odblokowywania

## Ładuje progress misji z pliku
func _load_mission_progress() -> void:
	var config = ConfigFile.new()
	var err = config.load("user://mission_progress.cfg")
	
	if err != OK:
		print("MissionSystem: Brak pliku progress misji")
		return
	
	completed_missions = config.get_value("missions", "completed", [])
	print("MissionSystem: Załadowano progress - ukończone misje: ", completed_missions.size())

## Zapisuje progress misji do pliku
func _save_mission_progress() -> void:
	var config = ConfigFile.new()
	config.set_value("missions", "completed", completed_missions)
	
	var err = config.save("user://mission_progress.cfg")
	if err == OK:
		print("MissionSystem: Progress misji zapisany")
	else:
		print("MissionSystem: Błąd zapisu progress misji: ", err)

## Zwraca listę dostępnych misji
func get_available_missions() -> Array[Mission]:
	var available: Array[Mission] = []
	for mission in available_missions:
		if _is_mission_available(mission):
			available.append(mission)
	return available

## Zwraca listę aktywnych misji
func get_active_missions() -> Array[Mission]:
	return active_missions

## Zwraca aktualną główną misję
func get_current_mission() -> Mission:
	return current_mission

## Sprawdza czy misja jest ukończona
func is_mission_completed(mission_id: String) -> bool:
	return mission_id in completed_missions