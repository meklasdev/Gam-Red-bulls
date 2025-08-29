extends Node
class_name RaceManager

## Menedżer wyścigów - zarządza checkpointami, czasem okrążeń i klasyfikacją
## Obsługuje różne typy wyścigów: okrążenia, sprint, eliminacje

signal race_started()
signal race_finished(results: Array[Dictionary])
signal lap_completed(vehicle: Node3D, lap_time: float, lap_number: int)
signal checkpoint_passed(vehicle: Node3D, checkpoint_index: int)
signal position_changed(vehicle: Node3D, new_position: int)
signal countdown_tick(seconds_remaining: int)

enum RaceType {
	LAPS,           ## Wyścig na okrążenia
	TIME_TRIAL,     ## Próba czasowa
	SPRINT,         ## Sprint do mety
	ELIMINATION,    ## Eliminacje
	DRIFT_CONTEST   ## Konkurs driftu
}

enum RaceState {
	WAITING,        ## Oczekiwanie na start
	COUNTDOWN,      ## Odliczanie
	RACING,         ## Wyścig w toku
	FINISHED        ## Wyścig zakończony
}

# Ustawienia wyścigu
@export var race_type := RaceType.LAPS
@export var total_laps := 3
@export var race_duration := 300.0  # sekundy (dla wyścigów na czas)
@export var countdown_duration := 3.0
@export var enable_false_start_detection := true

# Stan wyścigu
var race_state := RaceState.WAITING
var race_start_time := 0.0
var countdown_timer := 0.0
var race_timer := 0.0

# Checkpointy
var checkpoints: Array[Area3D] = []
var checkpoint_order: Array[int] = []

# Uczestnicy wyścigu
var participants: Array[Node3D] = []
var participant_data := {}  ## Dictionary[Node3D, ParticipantData]

# Wyniki
var race_results: Array[Dictionary] = []
var current_positions: Array[Node3D] = []

# Klasa danych uczestnika
class ParticipantData:
	var vehicle: Node3D
	var current_lap := 0
	var next_checkpoint := 0
	var lap_times: Array[float] = []
	var best_lap_time := 999999.0
	var total_time := 0.0
	var position := 1
	var checkpoints_passed := 0
	var is_finished := false
	var false_start := false
	
	func _init(v: Node3D):
		vehicle = v

func _ready() -> void:
	print("RaceManager: Inicjalizacja menedżera wyścigów")
	
	# Znajdź checkpointy w scenie
	_find_checkpoints()

func _process(delta: float) -> void:
	match race_state:
		RaceState.COUNTDOWN:
			_update_countdown(delta)
		RaceState.RACING:
			_update_race(delta)

## Znajduje checkpointy w scenie
func _find_checkpoints() -> void:
	checkpoints.clear()
	checkpoint_order.clear()
	
	# Szukaj w grupie Checkpoints
	var checkpoints_group = get_tree().get_first_node_in_group("checkpoints")
	if checkpoints_group:
		for child in checkpoints_group.get_children():
			if child is Area3D:
				checkpoints.append(child)
	
	# Sortuj checkpointy według nazwy lub pozycji
	checkpoints.sort_custom(_sort_checkpoints)
	
	# Ustaw kolejność
	for i in range(checkpoints.size()):
		checkpoint_order.append(i)
	
	print("RaceManager: Znaleziono ", checkpoints.size(), " checkpointów")
	
	# Połącz sygnały checkpointów
	_connect_checkpoint_signals()

## Sortuje checkpointy
func _sort_checkpoints(a: Area3D, b: Area3D) -> bool:
	# Sortuj według nazwy (np. Checkpoint1, Checkpoint2)
	return a.name < b.name

## Łączy sygnały checkpointów
func _connect_checkpoint_signals() -> void:
	for i in range(checkpoints.size()):
		var checkpoint = checkpoints[i]
		if not checkpoint.body_entered.is_connected(_on_checkpoint_entered):
			checkpoint.body_entered.connect(_on_checkpoint_entered.bind(i))

## Dodaje uczestnika do wyścigu
func add_participant(vehicle: Node3D) -> void:
	if vehicle in participants:
		return
	
	participants.append(vehicle)
	participant_data[vehicle] = ParticipantData.new(vehicle)
	
	print("RaceManager: Dodano uczestnika: ", vehicle.name)
	_update_positions()

## Usuwa uczestnika z wyścigu
func remove_participant(vehicle: Node3D) -> void:
	if not vehicle in participants:
		return
	
	participants.erase(vehicle)
	participant_data.erase(vehicle)
	
	print("RaceManager: Usunięto uczestnika: ", vehicle.name)
	_update_positions()

## Rozpoczyna wyścig
func start_race() -> void:
	if race_state != RaceState.WAITING:
		return
	
	if participants.is_empty():
		print("RaceManager: Brak uczestników wyścigu!")
		return
	
	print("RaceManager: Rozpoczynanie wyścigu...")
	race_state = RaceState.COUNTDOWN
	countdown_timer = countdown_duration
	
	# Resetuj dane uczestników
	_reset_participant_data()

## Resetuje dane uczestników
func _reset_participant_data() -> void:
	for vehicle in participants:
		var data = participant_data[vehicle]
		data.current_lap = 0
		data.next_checkpoint = 0
		data.lap_times.clear()
		data.best_lap_time = 999999.0
		data.total_time = 0.0
		data.checkpoints_passed = 0
		data.is_finished = false
		data.false_start = false

## Aktualizuje odliczanie
func _update_countdown(delta: float) -> void:
	countdown_timer -= delta
	
	var seconds_remaining = int(ceil(countdown_timer))
	if seconds_remaining != int(ceil(countdown_timer + delta)):
		countdown_tick.emit(seconds_remaining)
	
	# Sprawdź false start
	if enable_false_start_detection:
		_check_false_starts()
	
	if countdown_timer <= 0:
		_start_racing()

## Sprawdza false starty
func _check_false_starts() -> void:
	for vehicle in participants:
		var data = participant_data[vehicle]
		
		# Sprawdź czy pojazd się porusza przed startem
		if vehicle.has_method("get_speed_kmh"):
			var speed = vehicle.get_speed_kmh()
			if speed > 5.0 and not data.false_start:
				data.false_start = true
				print("RaceManager: False start - ", vehicle.name)
				# Tutaj można dodać karę

## Rozpoczyna właściwy wyścig
func _start_racing() -> void:
	race_state = RaceState.RACING
	race_start_time = Time.get_time_dict_from_system()["unix"]
	race_timer = 0.0
	
	print("RaceManager: Wyścig rozpoczęty!")
	race_started.emit()

## Aktualizuje wyścig
func _update_race(delta: float) -> void:
	race_timer += delta
	
	# Sprawdź warunki zakończenia
	match race_type:
		RaceType.TIME_TRIAL:
			if race_timer >= race_duration:
				_finish_race()
		RaceType.LAPS:
			_check_lap_completion()
		RaceType.SPRINT:
			_check_sprint_finish()

## Sprawdza ukończenie okrążeń
func _check_lap_completion() -> void:
	var finished_count = 0
	
	for vehicle in participants:
		var data = participant_data[vehicle]
		if data.is_finished:
			finished_count += 1
	
	# Zakończ wyścig jeśli wszyscy ukończyli lub lider skończył
	if finished_count >= participants.size() or _check_leader_finished():
		_finish_race()

## Sprawdza czy lider ukończył wyścig
func _check_leader_finished() -> bool:
	if current_positions.is_empty():
		return false
	
	var leader = current_positions[0]
	var leader_data = participant_data[leader]
	return leader_data.is_finished

## Sprawdza zakończenie sprintu
func _check_sprint_finish() -> void:
	for vehicle in participants:
		var data = participant_data[vehicle]
		if data.next_checkpoint >= checkpoints.size():
			data.is_finished = true
			_finish_race()
			return

## Callback przejścia przez checkpoint
func _on_checkpoint_entered(checkpoint_index: int, body: Node3D) -> void:
	if race_state != RaceState.RACING:
		return
	
	if not body in participants:
		return
	
	var data = participant_data[body]
	
	# Sprawdź czy to właściwy checkpoint
	if checkpoint_index != data.next_checkpoint:
		return
	
	print("RaceManager: ", body.name, " przeszedł checkpoint ", checkpoint_index)
	
	data.checkpoints_passed += 1
	data.next_checkpoint = (data.next_checkpoint + 1) % checkpoints.size()
	
	# Sprawdź ukończenie okrążenia
	if data.next_checkpoint == 0:
		_complete_lap(body, data)
	
	checkpoint_passed.emit(body, checkpoint_index)
	_update_positions()

## Kończy okrążenie
func _complete_lap(vehicle: Node3D, data: ParticipantData) -> void:
	data.current_lap += 1
	var lap_time = race_timer - data.total_time
	
	data.lap_times.append(lap_time)
	data.total_time = race_timer
	
	if lap_time < data.best_lap_time:
		data.best_lap_time = lap_time
	
	print("RaceManager: ", vehicle.name, " ukończył okrążenie ", data.current_lap, " w czasie ", lap_time)
	lap_completed.emit(vehicle, lap_time, data.current_lap)
	
	# Sprawdź czy ukończył wyścig
	if data.current_lap >= total_laps:
		data.is_finished = true
		print("RaceManager: ", vehicle.name, " ukończył wyścig!")

## Aktualizuje pozycje
func _update_positions() -> void:
	# Sortuj uczestników według postępu
	var sorted_participants = participants.duplicate()
	sorted_participants.sort_custom(_compare_participants)
	
	# Aktualizuj pozycje
	for i in range(sorted_participants.size()):
		var vehicle = sorted_participants[i]
		var data = participant_data[vehicle]
		var old_position = data.position
		data.position = i + 1
		
		if old_position != data.position:
			position_changed.emit(vehicle, data.position)
	
	current_positions = sorted_participants

## Porównuje uczestników dla sortowania
func _compare_participants(a: Node3D, b: Node3D) -> bool:
	var data_a = participant_data[a]
	var data_b = participant_data[b]
	
	# Najpierw ukończeni
	if data_a.is_finished and not data_b.is_finished:
		return true
	if not data_a.is_finished and data_b.is_finished:
		return false
	
	if data_a.is_finished and data_b.is_finished:
		# Sortuj według czasu ukończenia
		return data_a.total_time < data_b.total_time
	
	# Następnie według okrążeń
	if data_a.current_lap != data_b.current_lap:
		return data_a.current_lap > data_b.current_lap
	
	# Na koniec według checkpointów
	return data_a.checkpoints_passed > data_b.checkpoints_passed

## Kończy wyścig
func _finish_race() -> void:
	if race_state == RaceState.FINISHED:
		return
	
	race_state = RaceState.FINISHED
	
	# Przygotuj wyniki
	_prepare_race_results()
	
	print("RaceManager: Wyścig zakończony!")
	race_finished.emit(race_results)

## Przygotowuje wyniki wyścigu
func _prepare_race_results() -> void:
	race_results.clear()
	
	_update_positions()  # Ostateczne sortowanie
	
	for i in range(current_positions.size()):
		var vehicle = current_positions[i]
		var data = participant_data[vehicle]
		
		var result = {
			"vehicle": vehicle,
			"position": i + 1,
			"total_time": data.total_time,
			"best_lap": data.best_lap_time,
			"laps_completed": data.current_lap,
			"false_start": data.false_start,
			"finished": data.is_finished
		}
		
		race_results.append(result)

## Zwraca aktualną pozycję pojazdu
func get_vehicle_position(vehicle: Node3D) -> int:
	if vehicle in participant_data:
		return participant_data[vehicle].position
	return 0

## Zwraca najlepszy czas okrążenia pojazdu
func get_best_lap_time(vehicle: Node3D) -> float:
	if vehicle in participant_data:
		return participant_data[vehicle].best_lap_time
	return 0.0

## Zwraca aktualny numer okrążenia
func get_current_lap(vehicle: Node3D) -> int:
	if vehicle in participant_data:
		return participant_data[vehicle].current_lap
	return 0

## Zwraca czas wyścigu
func get_race_time() -> float:
	return race_timer

## Sprawdza czy wyścig jest aktywny
func is_race_active() -> bool:
	return race_state == RaceState.RACING

## Sprawdza czy wyścig jest zakończony
func is_race_finished() -> bool:
	return race_state == RaceState.FINISHED

## Resetuje wyścig
func reset_race() -> void:
	race_state = RaceState.WAITING
	race_timer = 0.0
	countdown_timer = 0.0
	race_results.clear()
	current_positions.clear()
	
	_reset_participant_data()
	print("RaceManager: Wyścig zresetowany")

## Ustawia typ wyścigu
func set_race_type(type: RaceType) -> void:
	race_type = type
	print("RaceManager: Ustawiono typ wyścigu: ", RaceType.keys()[type])

## Ustawia liczbę okrążeń
func set_total_laps(laps: int) -> void:
	total_laps = max(1, laps)
	print("RaceManager: Ustawiono liczbę okrążeń: ", total_laps)