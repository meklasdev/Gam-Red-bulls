extends Node
class_name StuntManager

## System zarządzania stuntami - wykrywa i punktuje akrobacje pojazdów
## Obsługuje różne typy stuntów: skoki, obroty, wheelie, stoppie, kombinacje

signal stunt_started(vehicle: Node3D, stunt_type: String)
signal stunt_completed(vehicle: Node3D, stunt_data: Dictionary)
signal stunt_failed(vehicle: Node3D, stunt_type: String)
signal combo_started(vehicle: Node3D)
signal combo_ended(vehicle: Node3D, total_score: int, combo_count: int)

enum StuntType {
	JUMP,           ## Skok w powietrzu
	FLIP,           ## Obrót w powietrzu
	SPIN,           ## Obrót wokół osi pionowej
	WHEELIE,        ## Jazda na tylnych kołach
	STOPPIE,        ## Jazda na przednich kołach
	BARREL_ROLL,    ## Beczka (obrót wokół osi podłużnej)
	DRIFT,          ## Długi drift
	GRIND,          ## Sliding po barierach
	PERFECT_LANDING ## Idealne lądowanie
}

# Ustawienia punktacji
var stunt_scores := {
	StuntType.JUMP: 100,
	StuntType.FLIP: 300,
	StuntType.SPIN: 250,
	StuntType.WHEELIE: 150,
	StuntType.STOPPIE: 200,
	StuntType.BARREL_ROLL: 400,
	StuntType.DRIFT: 50,  # na sekundę
	StuntType.GRIND: 75,  # na sekundę
	StuntType.PERFECT_LANDING: 500
}

# Mnożniki dla kombinacji
var combo_multipliers := [1.0, 1.2, 1.5, 2.0, 2.5, 3.0]  # dla 1, 2, 3, 4, 5, 6+ stuntów

# Progi wykrywania stuntów
@export var min_jump_height := 2.0                ## Minimalna wysokość skoku
@export var min_flip_angle := 270.0               ## Minimalny kąt dla flip (stopnie)
@export var min_spin_angle := 180.0               ## Minimalny kąt dla spin (stopnie)
@export var min_wheelie_duration := 1.0           ## Minimalny czas wheelie (sekundy)
@export var min_stoppie_duration := 0.5           ## Minimalny czas stoppie (sekundy)
@export var perfect_landing_tolerance := 15.0     ## Tolerancja dla idealnego lądowania (stopnie)
@export var combo_timeout := 3.0                  ## Czas na następny stunt w combo (sekundy)

# Stan stuntujących pojazdów
var vehicle_data := {}  ## Dictionary[Node3D, VehicleStuntData]

# Klasa danych stuntów pojazdu
class VehicleStuntData:
	var vehicle: Node3D
	var is_airborne := false
	var airborne_start_time := 0.0
	var airborne_start_position := Vector3.ZERO
	var airborne_start_rotation := Vector3.ZERO
	var ground_contact_count := 0
	
	var current_stunts: Array[Dictionary] = []
	var combo_active := false
	var combo_start_time := 0.0
	var combo_stunts: Array[Dictionary] = []
	var last_stunt_time := 0.0
	
	var total_score := 0
	var session_stunts := 0
	
	func _init(v: Node3D):
		vehicle = v

func _ready() -> void:
	print("StuntManager: Inicjalizacja systemu stuntów")

## Rejestruje pojazd do śledzenia stuntów
func register_vehicle(vehicle: Node3D) -> void:
	if vehicle in vehicle_data:
		return
	
	vehicle_data[vehicle] = VehicleStuntData.new(vehicle)
	print("StuntManager: Zarejestrowano pojazd: ", vehicle.name)

## Usuwa pojazd ze śledzenia
func unregister_vehicle(vehicle: Node3D) -> void:
	if vehicle in vehicle_data:
		var data = vehicle_data[vehicle]
		
		# Zakończ aktywne stunty
		_end_all_stunts(data)
		
		vehicle_data.erase(vehicle)
		print("StuntManager: Wyrejestrowano pojazd: ", vehicle.name)

func _process(delta: float) -> void:
	for vehicle in vehicle_data:
		_update_vehicle_stunts(vehicle, vehicle_data[vehicle], delta)

## Aktualizuje stunty pojazdu
func _update_vehicle_stunts(vehicle: Node3D, data: VehicleStuntData, delta: float) -> void:
	# Sprawdź kontakt z ziemią
	_check_ground_contact(vehicle, data)
	
	# Aktualizuj stunty w powietrzu
	if data.is_airborne:
		_update_airborne_stunts(vehicle, data, delta)
	
	# Aktualizuj stunty na ziemi
	if not data.is_airborne:
		_update_ground_stunts(vehicle, data, delta)
	
	# Sprawdź timeout combo
	_check_combo_timeout(vehicle, data)

## Sprawdza kontakt z ziemią
func _check_ground_contact(vehicle: Node3D, data: VehicleStuntData) -> void:
	var was_airborne = data.is_airborne
	var contact_count = _count_ground_contacts(vehicle)
	
	data.ground_contact_count = contact_count
	
	# Sprawdź czy pojazd jest w powietrzu
	var is_airborne = contact_count == 0
	
	if not was_airborne and is_airborne:
		# Początek lotu
		_start_airborne_phase(vehicle, data)
	elif was_airborne and not is_airborne:
		# Lądowanie
		_end_airborne_phase(vehicle, data)
	
	data.is_airborne = is_airborne

## Liczy kontakty z ziemią
func _count_ground_contacts(vehicle: Node3D) -> int:
	var contacts = 0
	
	# Dla VehicleBody3D sprawdź koła
	if vehicle is VehicleBody3D:
		var vehicle_body = vehicle as VehicleBody3D
		for i in range(vehicle_body.get_child_count()):
			var child = vehicle_body.get_child(i)
			if child is VehicleWheel3D:
				var wheel = child as VehicleWheel3D
				if wheel.is_in_contact():
					contacts += 1
	
	# Dla RigidBody3D użyj raycastów jeśli dostępne
	elif vehicle is RigidBody3D:
		# Sprawdź czy pojazd ma raycasty kół
		var front_ray = vehicle.get_node_or_null("FrontWheelRay")
		var rear_ray = vehicle.get_node_or_null("RearWheelRay")
		
		if front_ray and front_ray is RayCast3D and front_ray.is_colliding():
			contacts += 1
		if rear_ray and rear_ray is RayCast3D and rear_ray.is_colliding():
			contacts += 1
	
	return contacts

## Rozpoczyna fazę lotu
func _start_airborne_phase(vehicle: Node3D, data: VehicleStuntData) -> void:
	data.airborne_start_time = Time.get_time_dict_from_system()["unix"]
	data.airborne_start_position = vehicle.global_position
	data.airborne_start_rotation = vehicle.global_rotation
	
	print("StuntManager: ", vehicle.name, " jest w powietrzu")

## Kończy fazę lotu
func _end_airborne_phase(vehicle: Node3D, data: VehicleStuntData) -> void:
	var flight_time = Time.get_time_dict_from_system()["unix"] - data.airborne_start_time
	var landing_rotation = vehicle.global_rotation
	
	print("StuntManager: ", vehicle.name, " wylądował po ", flight_time, " sekundach")
	
	# Sprawdź stunty wykonane w powietrzu
	_evaluate_airborne_stunts(vehicle, data, flight_time, landing_rotation)
	
	# Wyczyść stunty w powietrzu
	data.current_stunts.clear()

## Aktualizuje stunty w powietrzu
func _update_airborne_stunts(vehicle: Node3D, data: VehicleStuntData, delta: float) -> void:
	var current_time = Time.get_time_dict_from_system()["unix"]
	var flight_time = current_time - data.airborne_start_time
	var current_rotation = vehicle.global_rotation
	
	# Sprawdź wysokość skoku
	var height = vehicle.global_position.y - data.airborne_start_position.y
	if height >= min_jump_height:
		_detect_jump_stunt(vehicle, data, height)
	
	# Sprawdź obroty
	_detect_rotation_stunts(vehicle, data, current_rotation)

## Aktualizuje stunty na ziemi
func _update_ground_stunts(vehicle: Node3D, data: VehicleStuntData, delta: float) -> void:
	# Sprawdź wheelie/stoppie dla motocykli
	if vehicle is MotoController:
		_check_wheelie_stoppie(vehicle, data)
	
	# Sprawdź drift (integracja z DriftScoring)
	if vehicle.has_method("is_vehicle_drifting") and vehicle.is_vehicle_drifting():
		_detect_drift_stunt(vehicle, data, delta)

## Wykrywa stunt skoku
func _detect_jump_stunt(vehicle: Node3D, data: VehicleStuntData, height: float) -> void:
	# Sprawdź czy skok już został zarejestrowany
	for stunt in data.current_stunts:
		if stunt["type"] == StuntType.JUMP:
			stunt["height"] = max(stunt["height"], height)
			return
	
	# Dodaj nowy stunt skoku
	var jump_stunt = {
		"type": StuntType.JUMP,
		"height": height,
		"start_time": data.airborne_start_time
	}
	
	data.current_stunts.append(jump_stunt)
	stunt_started.emit(vehicle, "Jump")

## Wykrywa stunty obrotowe
func _detect_rotation_stunts(vehicle: Node3D, data: VehicleStuntData, current_rotation: Vector3) -> void:
	var rotation_diff = current_rotation - data.airborne_start_rotation
	
	# Normalizuj kąty do zakresu -π do π
	rotation_diff.x = _normalize_angle(rotation_diff.x)
	rotation_diff.y = _normalize_angle(rotation_diff.y)
	rotation_diff.z = _normalize_angle(rotation_diff.z)
	
	var x_degrees = abs(rad_to_deg(rotation_diff.x))
	var y_degrees = abs(rad_to_deg(rotation_diff.y))
	var z_degrees = abs(rad_to_deg(rotation_diff.z))
	
	# Flip (obrót wokół osi X)
	if x_degrees >= min_flip_angle:
		_detect_flip_stunt(vehicle, data, x_degrees, "front" if rotation_diff.x > 0 else "back")
	
	# Spin (obrót wokół osi Y)
	if y_degrees >= min_spin_angle:
		_detect_spin_stunt(vehicle, data, y_degrees)
	
	# Barrel Roll (obrót wokół osi Z)
	if z_degrees >= min_flip_angle:
		_detect_barrel_roll_stunt(vehicle, data, z_degrees)

## Wykrywa flip stunt
func _detect_flip_stunt(vehicle: Node3D, data: VehicleStuntData, angle: float, direction: String) -> void:
	for stunt in data.current_stunts:
		if stunt["type"] == StuntType.FLIP:
			stunt["angle"] = max(stunt["angle"], angle)
			return
	
	var flip_stunt = {
		"type": StuntType.FLIP,
		"angle": angle,
		"direction": direction,
		"start_time": data.airborne_start_time
	}
	
	data.current_stunts.append(flip_stunt)
	stunt_started.emit(vehicle, "Flip " + direction.capitalize())

## Wykrywa spin stunt
func _detect_spin_stunt(vehicle: Node3D, data: VehicleStuntData, angle: float) -> void:
	for stunt in data.current_stunts:
		if stunt["type"] == StuntType.SPIN:
			stunt["angle"] = max(stunt["angle"], angle)
			return
	
	var spin_stunt = {
		"type": StuntType.SPIN,
		"angle": angle,
		"start_time": data.airborne_start_time
	}
	
	data.current_stunts.append(spin_stunt)
	stunt_started.emit(vehicle, "Spin")

## Wykrywa barrel roll stunt
func _detect_barrel_roll_stunt(vehicle: Node3D, data: VehicleStuntData, angle: float) -> void:
	for stunt in data.current_stunts:
		if stunt["type"] == StuntType.BARREL_ROLL:
			stunt["angle"] = max(stunt["angle"], angle)
			return
	
	var barrel_roll_stunt = {
		"type": StuntType.BARREL_ROLL,
		"angle": angle,
		"start_time": data.airborne_start_time
	}
	
	data.current_stunts.append(barrel_roll_stunt)
	stunt_started.emit(vehicle, "Barrel Roll")

## Sprawdza wheelie/stoppie
func _check_wheelie_stoppie(vehicle: Node3D, data: VehicleStuntData) -> void:
	var moto = vehicle as MotoController
	
	if moto.is_doing_wheelie():
		_detect_wheelie_stunt(vehicle, data)
	
	if moto.is_doing_stoppie():
		_detect_stoppie_stunt(vehicle, data)

## Wykrywa wheelie stunt
func _detect_wheelie_stunt(vehicle: Node3D, data: VehicleStuntData) -> void:
	# Sprawdź czy wheelie już jest aktywne
	for stunt in data.current_stunts:
		if stunt["type"] == StuntType.WHEELIE:
			return
	
	var wheelie_stunt = {
		"type": StuntType.WHEELIE,
		"start_time": Time.get_time_dict_from_system()["unix"],
		"duration": 0.0
	}
	
	data.current_stunts.append(wheelie_stunt)
	stunt_started.emit(vehicle, "Wheelie")

## Wykrywa stoppie stunt
func _detect_stoppie_stunt(vehicle: Node3D, data: VehicleStuntData) -> void:
	var stoppie_stunt = {
		"type": StuntType.STOPPIE,
		"start_time": Time.get_time_dict_from_system()["unix"],
		"duration": min_stoppie_duration
	}
	
	data.current_stunts.append(stoppie_stunt)
	stunt_started.emit(vehicle, "Stoppie")

## Wykrywa drift stunt
func _detect_drift_stunt(vehicle: Node3D, data: VehicleStuntData, delta: float) -> void:
	# Sprawdź czy drift już jest aktywny
	for stunt in data.current_stunts:
		if stunt["type"] == StuntType.DRIFT:
			stunt["duration"] += delta
			return
	
	var drift_stunt = {
		"type": StuntType.DRIFT,
		"start_time": Time.get_time_dict_from_system()["unix"],
		"duration": 0.0
	}
	
	data.current_stunts.append(drift_stunt)

## Ocenia stunty wykonane w powietrzu
func _evaluate_airborne_stunts(vehicle: Node3D, data: VehicleStuntData, flight_time: float, landing_rotation: Vector3) -> void:
	var completed_stunts: Array[Dictionary] = []
	
	for stunt in data.current_stunts:
		var stunt_data = _calculate_stunt_score(stunt, flight_time)
		if stunt_data["score"] > 0:
			completed_stunts.append(stunt_data)
	
	# Sprawdź idealne lądowanie
	var landing_stunt = _check_perfect_landing(vehicle, data, landing_rotation)
	if landing_stunt:
		completed_stunts.append(landing_stunt)
	
	# Przetwórz ukończone stunty
	for stunt_data in completed_stunts:
		_complete_stunt(vehicle, data, stunt_data)

## Oblicza punkty za stunt
func _calculate_stunt_score(stunt: Dictionary, flight_time: float) -> Dictionary:
	var base_score = stunt_scores.get(stunt["type"], 0)
	var final_score = base_score
	var multiplier = 1.0
	
	match stunt["type"]:
		StuntType.JUMP:
			multiplier = 1.0 + (stunt["height"] - min_jump_height) * 0.1
		StuntType.FLIP, StuntType.SPIN, StuntType.BARREL_ROLL:
			var rotations = stunt["angle"] / 360.0
			multiplier = rotations
		StuntType.WHEELIE:
			if stunt["duration"] >= min_wheelie_duration:
				multiplier = stunt["duration"]
			else:
				final_score = 0  # Za krótkie
		StuntType.DRIFT:
			multiplier = stunt["duration"]
	
	final_score = int(base_score * multiplier)
	
	return {
		"type": stunt["type"],
		"score": final_score,
		"multiplier": multiplier,
		"data": stunt
	}

## Sprawdza idealne lądowanie
func _check_perfect_landing(vehicle: Node3D, data: VehicleStuntData, landing_rotation: Vector3) -> Dictionary:
	# Sprawdź czy pojazd wylądował płasko
	var pitch = abs(rad_to_deg(landing_rotation.x))
	var roll = abs(rad_to_deg(landing_rotation.z))
	
	if pitch <= perfect_landing_tolerance and roll <= perfect_landing_tolerance:
		return {
			"type": StuntType.PERFECT_LANDING,
			"score": stunt_scores[StuntType.PERFECT_LANDING],
			"multiplier": 1.0,
			"data": {"pitch": pitch, "roll": roll}
		}
	
	return {}

## Kończy stunt
func _complete_stunt(vehicle: Node3D, data: VehicleStuntData, stunt_data: Dictionary) -> void:
	data.total_score += stunt_data["score"]
	data.session_stunts += 1
	data.last_stunt_time = Time.get_time_dict_from_system()["unix"]
	
	# Sprawdź combo
	_check_combo(vehicle, data, stunt_data)
	
	print("StuntManager: ", vehicle.name, " ukończył stunt: ", StuntType.keys()[stunt_data["type"]], " za ", stunt_data["score"], " punktów")
	stunt_completed.emit(vehicle, stunt_data)

## Sprawdza i zarządza combo
func _check_combo(vehicle: Node3D, data: VehicleStuntData, stunt_data: Dictionary) -> void:
	var current_time = Time.get_time_dict_from_system()["unix"]
	
	if not data.combo_active:
		# Rozpocznij combo
		data.combo_active = true
		data.combo_start_time = current_time
		data.combo_stunts.clear()
		combo_started.emit(vehicle)
	
	data.combo_stunts.append(stunt_data)

## Sprawdza timeout combo
func _check_combo_timeout(vehicle: Node3D, data: VehicleStuntData) -> void:
	if not data.combo_active:
		return
	
	var current_time = Time.get_time_dict_from_system()["unix"]
	var time_since_last_stunt = current_time - data.last_stunt_time
	
	if time_since_last_stunt >= combo_timeout:
		_end_combo(vehicle, data)

## Kończy combo
func _end_combo(vehicle: Node3D, data: VehicleStuntData) -> void:
	if not data.combo_active or data.combo_stunts.is_empty():
		return
	
	var combo_count = data.combo_stunts.size()
	var base_score = 0
	
	for stunt_data in data.combo_stunts:
		base_score += stunt_data["score"]
	
	# Zastosuj mnożnik combo
	var multiplier_index = min(combo_count - 1, combo_multipliers.size() - 1)
	var combo_multiplier = combo_multipliers[multiplier_index]
	var bonus_score = int(base_score * (combo_multiplier - 1.0))
	
	data.total_score += bonus_score
	data.combo_active = false
	
	print("StuntManager: Combo zakończone - ", combo_count, " stuntów, bonus: ", bonus_score)
	combo_ended.emit(vehicle, base_score + bonus_score, combo_count)

## Kończy wszystkie stunty pojazdu
func _end_all_stunts(data: VehicleStuntData) -> void:
	if data.combo_active:
		_end_combo(data.vehicle, data)
	
	data.current_stunts.clear()

## Normalizuje kąt do zakresu -π do π
func _normalize_angle(angle: float) -> float:
	while angle > PI:
		angle -= 2 * PI
	while angle < -PI:
		angle += 2 * PI
	return angle

## Zwraca całkowity wynik stuntów pojazdu
func get_total_score(vehicle: Node3D) -> int:
	if vehicle in vehicle_data:
		return vehicle_data[vehicle].total_score
	return 0

## Zwraca liczbę wykonanych stuntów
func get_stunt_count(vehicle: Node3D) -> int:
	if vehicle in vehicle_data:
		return vehicle_data[vehicle].session_stunts
	return 0

## Sprawdza czy pojazd ma aktywne combo
func has_active_combo(vehicle: Node3D) -> bool:
	if vehicle in vehicle_data:
		return vehicle_data[vehicle].combo_active
	return false

## Resetuje statystyki stuntów pojazdu
func reset_vehicle_stats(vehicle: Node3D) -> void:
	if vehicle in vehicle_data:
		var data = vehicle_data[vehicle]
		_end_all_stunts(data)
		data.total_score = 0
		data.session_stunts = 0
		print("StuntManager: Zresetowano statystyki stuntów - ", vehicle.name)