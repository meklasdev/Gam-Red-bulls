extends VehicleBody3D
class_name CarController

## Kontroler samochodu - używa VehicleBody3D z realistyczną fizyką
## Obsługuje sterowanie, nitro, drift i efekty dźwiękowe

signal speed_changed(speed_kmh: float)
signal gear_changed(gear: int)
signal nitro_used()
signal drift_started()
signal drift_ended(score: int)

@export_group("Silnik")
@export var engine_force_base := 1200.0      ## Bazowa siła silnika
@export var max_rpm := 6000.0                ## Maksymalne obroty silnika
@export var max_torque := 400.0              ## Maksymalny moment obrotowy
@export var idle_rpm := 800.0                ## Obroty na biegu jałowym

@export_group("Hamowanie")
@export var brake_force := 80.0              ## Siła hamowania
@export var handbrake_force := 150.0         ## Siła hamulca ręcznego

@export_group("Kierowanie")
@export var steer_limit := 0.8               ## Maksymalny kąt skrętu kół
@export var steer_speed := 2.0               ## Prędkość skręcania

@export_group("Nitro")
@export var nitro_force_multiplier := 1.8    ## Mnożnik siły przy nitro
@export var nitro_capacity := 100.0          ## Pojemność nitro
@export var nitro_consumption_rate := 25.0   ## Zużycie nitro na sekundę
@export var nitro_recharge_rate := 5.0       ## Regeneracja nitro na sekundę

@export_group("Drift")
@export var drift_threshold := 15.0          ## Próg kąta dla driftu (stopnie)
@export var drift_force_reduction := 0.7     ## Redukcja siły przy drifcie

@export_group("Dźwięki")
@export var engine_audio: AudioStreamPlayer3D
@export var tire_screech_audio: AudioStreamPlayer3D
@export var nitro_audio: AudioStreamPlayer3D

# Stan pojazdu
var current_speed_kmh := 0.0
var current_rpm := 800.0
var current_gear := 1
var is_drifting := false
var drift_start_time := 0.0
var drift_score := 0

# Stan nitro
var nitro_amount := 100.0
var is_using_nitro := false

# Input
var throttle_input := 0.0
var brake_input := 0.0
var steer_input := 0.0
var handbrake_input := false
var nitro_input := false

# Referencje do kół
@onready var front_left_wheel: VehicleWheel3D = $FrontLeftWheel
@onready var front_right_wheel: VehicleWheel3D = $FrontRightWheel
@onready var rear_left_wheel: VehicleWheel3D = $RearLeftWheel
@onready var rear_right_wheel: VehicleWheel3D = $RearRightWheel

func _ready() -> void:
	print("CarController: Inicjalizacja samochodu")
	
	# Ustaw podstawowe parametry fizyki
	mass = 1200.0  # kg
	center_of_mass_offset = Vector3(0, -0.5, 0.2)  # Niżej i lekko do tyłu
	
	# Konfiguruj koła jeśli istnieją
	_setup_wheels()
	
	# Połącz sygnały
	if InputManager:
		InputManager.input_device_changed.connect(_on_input_device_changed)

func _physics_process(delta: float) -> void:
	_handle_input()
	_update_engine(delta)
	_update_nitro(delta)
	_update_drift_detection()
	_update_audio()
	_update_stats()

## Obsługuje wejście gracza
func _handle_input() -> void:
	# Pobierz input z InputManager lub bezpośrednio
	if InputManager:
		var movement = InputManager.get_movement_vector()
		throttle_input = max(0.0, movement.y)
		brake_input = max(0.0, -movement.y)
		steer_input = movement.x
		
		handbrake_input = InputManager.is_action_pressed("handbrake")
		nitro_input = InputManager.is_action_pressed("nitro")
	else:
		# Fallback na bezpośredni input
		throttle_input = Input.get_action_strength("move_forward")
		brake_input = Input.get_action_strength("move_backward")
		steer_input = Input.get_action_strength("steer_right") - Input.get_action_strength("steer_left")
		handbrake_input = Input.is_action_pressed("handbrake")
		nitro_input = Input.is_action_pressed("nitro")

## Aktualizuje silnik i transmisję
func _update_engine(delta: float) -> void:
	# Oblicz aktualną prędkość
	current_speed_kmh = linear_velocity.length() * 3.6
	
	# Oblicz docelowe obroty na podstawie prędkości i biegu
	var target_rpm = idle_rpm + (current_speed_kmh / 200.0) * (max_rpm - idle_rpm)
	target_rpm = clamp(target_rpm, idle_rpm, max_rpm)
	
	# Płynnie dostosuj obroty
	current_rpm = lerp(current_rpm, target_rpm, delta * 5.0)
	
	# Oblicz siłę silnika
	var engine_power = engine_force_base
	
	# Zastosuj nitro
	if is_using_nitro and nitro_amount > 0:
		engine_power *= nitro_force_multiplier
	
	# Zastosuj wpływ pogody
	if WeatherManager:
		engine_power *= WeatherManager.get_grip_multiplier()
	
	# Redukcja mocy przy drifcie
	if is_drifting:
		engine_power *= drift_force_reduction
	
	# Zastosuj siły
	engine_force = engine_power * throttle_input
	
	# Hamowanie
	var total_brake = brake_input * brake_force
	if handbrake_input:
		total_brake += handbrake_force
	
	brake = total_brake
	
	# Kierowanie z płynnym przejściem
	var target_steering = steer_limit * steer_input
	steering = lerp(steering, target_steering, delta * steer_speed)
	
	# Automatyczna zmiana biegów (uproszczona)
	_update_transmission()

## Aktualizuje transmisję (automatyczna skrzynia)
func _update_transmission() -> void:
	var new_gear = current_gear
	
	# Zmiana na wyższy bieg
	if current_rpm > max_rpm * 0.8 and current_gear < 6:
		new_gear = current_gear + 1
	# Zmiana na niższy bieg
	elif current_rpm < max_rpm * 0.3 and current_gear > 1:
		new_gear = current_gear - 1
	
	if new_gear != current_gear:
		current_gear = new_gear
		gear_changed.emit(current_gear)

## Aktualizuje system nitro
func _update_nitro(delta: float) -> void:
	if nitro_input and nitro_amount > 0:
		if not is_using_nitro:
			is_using_nitro = true
			nitro_used.emit()
		
		# Zużywaj nitro
		nitro_amount = max(0, nitro_amount - nitro_consumption_rate * delta)
	else:
		is_using_nitro = false
		
		# Regeneruj nitro (wolniej)
		nitro_amount = min(nitro_capacity, nitro_amount + nitro_recharge_rate * delta)

## Wykrywa drift
func _update_drift_detection() -> void:
	# Oblicz kąt poślizgu
	var velocity_angle = atan2(linear_velocity.x, linear_velocity.z)
	var forward_angle = global_transform.basis.z.angle_to(Vector3.FORWARD)
	var slip_angle = abs(rad_to_deg(velocity_angle - forward_angle))
	
	# Sprawdź czy driftujemy
	var should_drift = slip_angle > drift_threshold and current_speed_kmh > 30.0
	
	if should_drift and not is_drifting:
		# Rozpocznij drift
		is_drifting = true
		drift_start_time = Time.get_time_dict_from_system()["unix"]
		drift_score = 0
		drift_started.emit()
	elif not should_drift and is_drifting:
		# Zakończ drift
		is_drifting = false
		var drift_duration = Time.get_time_dict_from_system()["unix"] - drift_start_time
		drift_score = int(slip_angle * current_speed_kmh * drift_duration * 10)
		drift_ended.emit(drift_score)
	elif is_drifting:
		# Aktualizuj punkty driftu
		drift_score += int(slip_angle * current_speed_kmh * 0.1)

## Aktualizuje dźwięki
func _update_audio() -> void:
	# Dźwięk silnika
	if engine_audio:
		var engine_pitch = 0.8 + (current_rpm / max_rpm) * 0.8
		engine_audio.pitch_scale = engine_pitch
		engine_audio.volume_db = linear_to_db(0.3 + throttle_input * 0.4)
		
		if not engine_audio.playing:
			engine_audio.play()
	
	# Dźwięk piszczenia opon
	if tire_screech_audio:
		if is_drifting or handbrake_input:
			if not tire_screech_audio.playing:
				tire_screech_audio.play()
			tire_screech_audio.volume_db = linear_to_db(0.6)
		else:
			tire_screech_audio.stop()
	
	# Dźwięk nitro
	if nitro_audio:
		if is_using_nitro:
			if not nitro_audio.playing:
				nitro_audio.play()
		else:
			nitro_audio.stop()

## Aktualizuje statystyki
func _update_stats() -> void:
	speed_changed.emit(current_speed_kmh)

## Konfiguruje koła
func _setup_wheels() -> void:
	# Sprawdź czy węzły kół istnieją
	var wheels = [
		get_node_or_null("FrontLeftWheel"),
		get_node_or_null("FrontRightWheel"), 
		get_node_or_null("RearLeftWheel"),
		get_node_or_null("RearRightWheel")
	]
	
	for wheel in wheels:
		if wheel is VehicleWheel3D:
			# Ustaw podstawowe parametry kół
			wheel.suspension_travel = 0.3
			wheel.suspension_stiffness = 40.0
			wheel.suspension_max_force = 2000.0
			wheel.damping_compression = 0.88
			wheel.damping_relaxation = 0.88
			wheel.use_as_traction = true
			wheel.use_as_steering = wheel.name.begins_with("Front")

## Resetuje pozycję pojazdu
func reset_vehicle() -> void:
	print("CarController: Resetowanie pozycji pojazdu")
	
	# Zatrzymaj pojazd
	linear_velocity = Vector3.ZERO
	angular_velocity = Vector3.ZERO
	
	# Ustaw prosto
	global_transform.basis = Basis.IDENTITY
	
	# Przenieś lekko w górę
	global_position += Vector3.UP * 2.0

## Callback zmiany urządzenia wejściowego
func _on_input_device_changed(device_type: InputManager.InputDevice) -> void:
	print("CarController: Zmiana urządzenia wejściowego na: ", InputManager.InputDevice.keys()[device_type])

## Zwraca aktualną prędkość w km/h
func get_speed_kmh() -> float:
	return current_speed_kmh

## Zwraca aktualne obroty silnika
func get_rpm() -> float:
	return current_rpm

## Zwraca aktualny bieg
func get_gear() -> int:
	return current_gear

## Zwraca ilość nitro (0-100)
func get_nitro_percentage() -> float:
	return (nitro_amount / nitro_capacity) * 100.0

## Sprawdza czy pojazd dryfuje
func is_vehicle_drifting() -> bool:
	return is_drifting

## Zwraca aktualny wynik driftu
func get_drift_score() -> int:
	return drift_score

## Dodaje nitro
func add_nitro(amount: float) -> void:
	nitro_amount = min(nitro_capacity, nitro_amount + amount)

## Naprawia pojazd (przywraca pełne zdrowie)
func repair_vehicle() -> void:
	print("CarController: Naprawianie pojazdu")
	# Tutaj można dodać system uszkodzeń
	
## Ustawia kolor pojazdu
func set_vehicle_color(color: Color) -> void:
	# Znajdź mesh i zmień materiał
	var mesh_node = get_node_or_null("MeshInstance3D")
	if mesh_node and mesh_node.get_surface_override_material(0):
		var material = mesh_node.get_surface_override_material(0).duplicate()
		material.albedo_color = color
		mesh_node.set_surface_override_material(0, material)