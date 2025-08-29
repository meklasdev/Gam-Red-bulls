extends Node
class_name DriftScoring

## System punktacji driftu - oblicza punkty na podstawie kąta, prędkości i czasu
## Obsługuje combo, mnożniki i różne typy driftów

signal drift_started(vehicle: Node3D)
signal drift_ended(vehicle: Node3D, final_score: int, duration: float)
signal drift_score_updated(vehicle: Node3D, current_score: int, multiplier: float)
signal combo_achieved(vehicle: Node3D, combo_level: int)

# Ustawienia punktacji
@export var base_points_per_second := 10.0    ## Bazowe punkty na sekundę
@export var speed_multiplier := 0.1           ## Mnożnik prędkości (punkty na km/h)
@export var angle_multiplier := 2.0           ## Mnożnik kąta poślizgu
@export var min_drift_angle := 15.0           ## Minimalny kąt dla driftu (stopnie)
@export var min_drift_speed := 20.0           ## Minimalna prędkość dla driftu (km/h)

# Combo system
@export var combo_threshold := 1000           ## Punkty potrzebne do combo
@export var combo_multiplier := 1.5           ## Mnożnik za combo
@export var max_combo_level := 5              ## Maksymalny poziom combo

# Mnożniki za różne typy driftów
var drift_type_multipliers := {
	"normal": 1.0,          ## Zwykły drift
	"long": 1.5,            ## Długi drift (>5 sekund)
	"high_speed": 2.0,      ## Szybki drift (>80 km/h)
	"sharp_angle": 1.8,     ## Ostry kąt (>45 stopni)
	"perfect": 3.0,         ## Perfekcyjny drift (kombinacja wszystkich)
	"chain": 2.5            ## Łańcuch driftów bez przerwy
}

# Stan driftujących pojazdów
var drifting_vehicles := {}  ## Dictionary[Node3D, DriftData]

# Klasa do przechowywania danych driftu
class DriftData:
	var is_drifting := false
	var start_time := 0.0
	var current_score := 0
	var total_score := 0
	var combo_level := 0
	var combo_score := 0
	var last_drift_end_time := 0.0
	var chain_count := 0
	var best_angle := 0.0
	var best_speed := 0.0
	var drift_type := "normal"

func _ready() -> void:
	print("DriftScoring: Inicjalizacja systemu punktacji driftu")

## Rejestruje pojazd do śledzenia driftu
func register_vehicle(vehicle: Node3D) -> void:
	if vehicle in drifting_vehicles:
		return
	
	drifting_vehicles[vehicle] = DriftData.new()
	print("DriftScoring: Zarejestrowano pojazd: ", vehicle.name)

## Usuwa pojazd z śledzenia
func unregister_vehicle(vehicle: Node3D) -> void:
	if vehicle in drifting_vehicles:
		# Zakończ drift jeśli aktywny
		if drifting_vehicles[vehicle].is_drifting:
			end_drift(vehicle)
		
		drifting_vehicles.erase(vehicle)
		print("DriftScoring: Wyrejestrowano pojazd: ", vehicle.name)

## Aktualizuje drift dla pojazdu
func update_drift(vehicle: Node3D, slip_angle: float, speed_kmh: float, delta: float) -> void:
	if not vehicle in drifting_vehicles:
		return
	
	var drift_data = drifting_vehicles[vehicle]
	var should_drift = _should_be_drifting(slip_angle, speed_kmh)
	
	if should_drift and not drift_data.is_drifting:
		_start_drift(vehicle, drift_data)
	elif not should_drift and drift_data.is_drifting:
		_end_drift(vehicle, drift_data)
	elif drift_data.is_drifting:
		_update_drift_score(vehicle, drift_data, slip_angle, speed_kmh, delta)

## Sprawdza czy pojazd powinien driftować
func _should_be_drifting(slip_angle: float, speed_kmh: float) -> bool:
	return abs(slip_angle) >= min_drift_angle and speed_kmh >= min_drift_speed

## Rozpoczyna drift
func _start_drift(vehicle: Node3D, drift_data: DriftData) -> void:
	drift_data.is_drifting = true
	drift_data.start_time = Time.get_time_dict_from_system()["unix"]
	drift_data.current_score = 0
	drift_data.best_angle = 0.0
	drift_data.best_speed = 0.0
	drift_data.drift_type = "normal"
	
	# Sprawdź czy to część łańcucha
	var current_time = Time.get_time_dict_from_system()["unix"]
	if current_time - drift_data.last_drift_end_time < 3.0:  # 3 sekundy na łańcuch
		drift_data.chain_count += 1
	else:
		drift_data.chain_count = 1
	
	print("DriftScoring: Rozpoczęto drift - pojazd: ", vehicle.name)
	drift_started.emit(vehicle)

## Kończy drift
func _end_drift(vehicle: Node3D, drift_data: DriftData) -> void:
	if not drift_data.is_drifting:
		return
	
	drift_data.is_drifting = false
	var current_time = Time.get_time_dict_from_system()["unix"]
	var duration = current_time - drift_data.start_time
	drift_data.last_drift_end_time = current_time
	
	# Oblicz końcowy wynik
	var final_score = _calculate_final_score(drift_data, duration)
	drift_data.total_score += final_score
	
	# Sprawdź combo
	_check_combo(vehicle, drift_data, final_score)
	
	print("DriftScoring: Zakończono drift - pojazd: ", vehicle.name, ", wynik: ", final_score, ", czas: ", duration)
	drift_ended.emit(vehicle, final_score, duration)

## Aktualizuje wynik driftu w czasie rzeczywistym
func _update_drift_score(vehicle: Node3D, drift_data: DriftData, slip_angle: float, speed_kmh: float, delta: float) -> void:
	# Zapisz najlepsze wartości
	drift_data.best_angle = max(drift_data.best_angle, abs(slip_angle))
	drift_data.best_speed = max(drift_data.best_speed, speed_kmh)
	
	# Oblicz punkty za ten frame
	var frame_points = _calculate_frame_points(slip_angle, speed_kmh, delta)
	drift_data.current_score += frame_points
	
	# Określ typ driftu
	drift_data.drift_type = _determine_drift_type(drift_data, Time.get_time_dict_from_system()["unix"] - drift_data.start_time)
	
	# Oblicz aktualny mnożnik
	var multiplier = _calculate_current_multiplier(drift_data)
	
	drift_score_updated.emit(vehicle, int(drift_data.current_score * multiplier), multiplier)

## Oblicza punkty za pojedynczy frame
func _calculate_frame_points(slip_angle: float, speed_kmh: float, delta: float) -> float:
	var angle_factor = abs(slip_angle) / 45.0  # Normalizuj do 45 stopni
	var speed_factor = speed_kmh / 100.0       # Normalizuj do 100 km/h
	
	var points = base_points_per_second * delta
	points *= (1.0 + angle_factor * angle_multiplier)
	points *= (1.0 + speed_factor * speed_multiplier)
	
	return points

## Oblicza końcowy wynik driftu
func _calculate_final_score(drift_data: DriftData, duration: float) -> int:
	var base_score = drift_data.current_score
	
	# Mnożnik typu driftu
	var type_multiplier = drift_type_multipliers.get(drift_data.drift_type, 1.0)
	
	# Bonus za czas
	var time_bonus = 1.0
	if duration > 5.0:
		time_bonus = 1.0 + (duration - 5.0) * 0.1  # +10% za każdą sekundę ponad 5
	
	# Bonus za łańcuch
	var chain_bonus = 1.0
	if drift_data.chain_count > 1:
		chain_bonus = 1.0 + (drift_data.chain_count - 1) * 0.2  # +20% za każdy drift w łańcuchu
	
	# Combo multiplier
	var combo_multiplier_value = 1.0 + (drift_data.combo_level * 0.5)
	
	var final_score = base_score * type_multiplier * time_bonus * chain_bonus * combo_multiplier_value
	return int(final_score)

## Określa typ driftu na podstawie parametrów
func _determine_drift_type(drift_data: DriftData, duration: float) -> String:
	var is_long = duration > 5.0
	var is_high_speed = drift_data.best_speed > 80.0
	var is_sharp = drift_data.best_angle > 45.0
	var is_chain = drift_data.chain_count > 1
	
	# Perfekcyjny drift
	if is_long and is_high_speed and is_sharp:
		return "perfect"
	
	# Łańcuch driftów
	if is_chain:
		return "chain"
	
	# Specjalne typy
	if is_high_speed:
		return "high_speed"
	elif is_sharp:
		return "sharp_angle"
	elif is_long:
		return "long"
	
	return "normal"

## Oblicza aktualny mnożnik
func _calculate_current_multiplier(drift_data: DriftData) -> float:
	var base_multiplier = drift_type_multipliers.get(drift_data.drift_type, 1.0)
	var combo_multiplier_value = 1.0 + (drift_data.combo_level * 0.5)
	
	return base_multiplier * combo_multiplier_value

## Sprawdza i aktualizuje combo
func _check_combo(vehicle: Node3D, drift_data: DriftData, score: int) -> void:
	drift_data.combo_score += score
	
	# Sprawdź czy osiągnięto próg combo
	var new_combo_level = drift_data.combo_score / combo_threshold
	new_combo_level = min(new_combo_level, max_combo_level)
	
	if new_combo_level > drift_data.combo_level:
		drift_data.combo_level = new_combo_level
		print("DriftScoring: Combo poziom ", new_combo_level, " - pojazd: ", vehicle.name)
		combo_achieved.emit(vehicle, new_combo_level)

## Resetuje combo dla pojazdu
func reset_combo(vehicle: Node3D) -> void:
	if vehicle in drifting_vehicles:
		var drift_data = drifting_vehicles[vehicle]
		drift_data.combo_level = 0
		drift_data.combo_score = 0
		print("DriftScoring: Reset combo - pojazd: ", vehicle.name)

## Kończy drift dla pojazdu (publiczne API)
func end_drift(vehicle: Node3D) -> void:
	if vehicle in drifting_vehicles:
		_end_drift(vehicle, drifting_vehicles[vehicle])

## Zwraca aktualny wynik driftu
func get_current_drift_score(vehicle: Node3D) -> int:
	if vehicle in drifting_vehicles:
		var drift_data = drifting_vehicles[vehicle]
		if drift_data.is_drifting:
			var multiplier = _calculate_current_multiplier(drift_data)
			return int(drift_data.current_score * multiplier)
	return 0

## Zwraca całkowity wynik driftu
func get_total_drift_score(vehicle: Node3D) -> int:
	if vehicle in drifting_vehicles:
		return drifting_vehicles[vehicle].total_score
	return 0

## Zwraca poziom combo
func get_combo_level(vehicle: Node3D) -> int:
	if vehicle in drifting_vehicles:
		return drifting_vehicles[vehicle].combo_level
	return 0

## Sprawdza czy pojazd aktualnie driftuje
func is_drifting(vehicle: Node3D) -> bool:
	if vehicle in drifting_vehicles:
		return drifting_vehicles[vehicle].is_drifting
	return false

## Zwraca typ aktualnego driftu
func get_drift_type(vehicle: Node3D) -> String:
	if vehicle in drifting_vehicles:
		return drifting_vehicles[vehicle].drift_type
	return "normal"

## Resetuje wszystkie statystyki dla pojazdu
func reset_vehicle_stats(vehicle: Node3D) -> void:
	if vehicle in drifting_vehicles:
		var drift_data = drifting_vehicles[vehicle]
		drift_data.total_score = 0
		drift_data.combo_level = 0
		drift_data.combo_score = 0
		drift_data.chain_count = 0
		
		if drift_data.is_drifting:
			end_drift(vehicle)
		
		print("DriftScoring: Reset statystyk - pojazd: ", vehicle.name)