extends RigidBody3D
class_name BikeController

## Kontroler roweru - używa RigidBody3D z własną fizyką i balansowaniem
## Lżejszy i bardziej zwinny niż motocykl, ale mniej stabilny

signal speed_changed(speed_kmh: float)
signal balance_changed(balance: float)
signal stamina_changed(stamina: float)
signal trick_performed(trick_name: String, points: int)

@export_group("Rower")
@export var pedal_force := 800.0              ## Siła pedałowania
@export var max_speed_kmh := 60.0             ## Maksymalna prędkość
@export var brake_force := 50.0               ## Siła hamowania
@export var steer_torque := 15.0              ## Moment skręcania

@export_group("Balansowanie")
@export var balance_sensitivity := 2.0        ## Czułość balansowania
@export var auto_balance_force := 20.0        ## Siła automatycznego balansowania
@export var fall_threshold := 45.0            ## Próg upadku (stopnie)

@export_group("Stamina")
@export var max_stamina := 100.0              ## Maksymalna wytrzymałość
@export var stamina_consumption := 15.0       ## Zużycie staminy na sekundę
@export var stamina_recovery := 8.0           ## Regeneracja staminy na sekundę

@export_group("Triki")
@export var trick_force := 10.0               ## Siła wykonywania trików
@export var trick_threshold_speed := 20.0     ## Minimalna prędkość dla trików

@export_group("Dźwięki")
@export var pedal_audio: AudioStreamPlayer3D
@export var brake_audio: AudioStreamPlayer3D
@export var crash_audio: AudioStreamPlayer3D

# Stan roweru
var current_speed_kmh := 0.0
var balance := 0.0  # -1.0 do 1.0, 0 = idealne balansowanie
var is_fallen := false
var stamina := 100.0

# Input
var pedal_input := 0.0
var brake_input := 0.0
var steer_input := 0.0
var balance_input := 0.0
var trick_input := ""

# Raycasty dla kół
@onready var front_wheel_ray: RayCast3D = $FrontWheelRay
@onready var rear_wheel_ray: RayCast3D = $RearWheelRay

# Mesh i animacje
@onready var bike_mesh: MeshInstance3D = $BikeMesh
@onready var rider_mesh: MeshInstance3D = $RiderMesh

func _ready() -> void:
	print("BikeController: Inicjalizacja roweru")
	
	# Ustaw podstawowe parametry fizyki
	mass = 15.0  # kg (rower + rider)
	gravity_scale = 1.0
	
	# Konfiguruj raycasty
	_setup_wheel_raycasts()
	
	# Ustaw tryb fizyki
	freeze_mode = RigidBody3D.FREEZE_MODE_KINEMATIC

func _physics_process(delta: float) -> void:
	if is_fallen:
		_handle_fallen_state(delta)
		return
	
	_handle_input()
	_apply_pedaling(delta)
	_apply_steering(delta)
	_apply_braking(delta)
	_update_balance(delta)
	_update_stamina(delta)
	_check_fall()
	_update_audio()
	_update_stats()

## Obsługuje wejście gracza
func _handle_input() -> void:
	if InputManager:
		var movement = InputManager.get_movement_vector()
		pedal_input = max(0.0, movement.y)
		brake_input = max(0.0, -movement.y)
		steer_input = movement.x
		
		# Balansowanie na analogowych drążkach lub A/D
		balance_input = Input.get_action_strength("steer_right") - Input.get_action_strength("steer_left")
		
		# Triki
		if InputManager.is_action_just_pressed("stunt_next"):
			trick_input = "wheelie"
		elif InputManager.is_action_just_pressed("stunt_prev"):
			trick_input = "endo"
		elif InputManager.is_action_just_pressed("jump"):
			trick_input = "jump"
	else:
		# Fallback
		pedal_input = Input.get_action_strength("move_forward")
		brake_input = Input.get_action_strength("move_backward")
		steer_input = Input.get_action_strength("steer_right") - Input.get_action_strength("steer_left")
		balance_input = steer_input * 0.5

## Stosuje siłę pedałowania
func _apply_pedaling(delta: float) -> void:
	if pedal_input > 0 and stamina > 0:
		# Oblicz kierunek ruchu
		var forward = -global_transform.basis.z
		
		# Zastosuj siłę pedałowania
		var force = forward * pedal_force * pedal_input
		
		# Ograniczenie prędkości
		if current_speed_kmh < max_speed_kmh:
			apply_central_force(force)
		
		# Zużywaj stamina
		stamina = max(0, stamina - stamina_consumption * pedal_input * delta)

## Stosuje kierowanie
func _apply_steering(delta: float) -> void:
	if abs(steer_input) > 0.1:
		var steer_strength = steer_input * steer_torque
		
		# Kierowanie jest skuteczniejsze przy większej prędkości
		var speed_factor = clamp(current_speed_kmh / 20.0, 0.2, 1.0)
		steer_strength *= speed_factor
		
		# Zastosuj moment obrotowy
		apply_torque(Vector3.UP * steer_strength)

## Stosuje hamowanie
func _apply_braking(delta: float) -> void:
	if brake_input > 0:
		# Hamowanie redukuje prędkość
		var brake_strength = brake_force * brake_input
		var velocity_reduction = linear_velocity.normalized() * brake_strength * delta
		linear_velocity -= velocity_reduction
		
		# Dźwięk hamowania
		if brake_audio and not brake_audio.playing:
			brake_audio.play()

## Aktualizuje balansowanie
func _update_balance(delta: float) -> void:
	# Automatyczne balansowanie na podstawie prędkości
	var auto_balance_strength = auto_balance_force * clamp(current_speed_kmh / 30.0, 0.1, 1.0)
	
	# Wpływ gracza na balansowanie
	balance += balance_input * balance_sensitivity * delta
	
	# Automatyczne korekcje
	balance = lerp(balance, 0.0, auto_balance_strength * delta)
	
	# Ograniczenie balansu
	balance = clamp(balance, -1.0, 1.0)
	
	# Zastosuj balansowanie jako obrót
	var balance_rotation = balance * 30.0  # maksymalnie 30 stopni
	var target_rotation = Vector3(0, 0, deg_to_rad(balance_rotation))
	
	# Płynnie obracaj rower
	var current_rotation = global_transform.basis.get_euler()
	current_rotation.z = lerp_angle(current_rotation.z, target_rotation.z, delta * 5.0)
	global_transform.basis = Basis.from_euler(current_rotation)

## Aktualizuje wytrzymałość
func _update_stamina(delta: float) -> void:
	if pedal_input == 0:
		# Regeneruj stamina gdy nie pedałujemy
		stamina = min(max_stamina, stamina + stamina_recovery * delta)
	
	stamina_changed.emit(stamina)

## Sprawdza czy rower się przewrócił
func _check_fall() -> void:
	var tilt_angle = abs(rad_to_deg(global_transform.basis.get_euler().z))
	
	if tilt_angle > fall_threshold:
		_fall_down()

## Obsługuje stan po upadku
func _fall_down() -> void:
	if is_fallen:
		return
	
	print("BikeController: Rower się przewrócił!")
	is_fallen = true
	
	# Dźwięk upadku
	if crash_audio:
		crash_audio.play()
	
	# Zatrzymaj rower
	linear_velocity *= 0.3
	angular_velocity *= 0.5

## Obsługuje stan po upadku
func _handle_fallen_state(delta: float) -> void:
	# Gracz może się podnieść przyciskiem
	if Input.is_action_just_pressed("ui_accept") or Input.is_action_just_pressed("jump"):
		_get_back_up()

## Podnosi rower po upadku
func _get_back_up() -> void:
	print("BikeController: Podnoszenie roweru")
	is_fallen = false
	balance = 0.0
	
	# Ustaw rower prosto
	var current_pos = global_position
	global_transform = Transform3D.IDENTITY
	global_position = current_pos + Vector3.UP * 0.5
	
	# Zatrzymaj ruch
	linear_velocity = Vector3.ZERO
	angular_velocity = Vector3.ZERO

## Wykonuje trik
func _perform_trick(trick_name: String) -> void:
	if current_speed_kmh < trick_threshold_speed:
		return
	
	var points = 0
	var force = Vector3.ZERO
	
	match trick_name:
		"wheelie":
			points = 50
			force = Vector3.UP * trick_force + global_transform.basis.z * trick_force
			apply_force(force, Vector3(0, 0, -1))  # Siła z tyłu
		
		"endo":
			points = 75
			force = Vector3.UP * trick_force - global_transform.basis.z * trick_force
			apply_force(force, Vector3(0, 0, 1))  # Siła z przodu
		
		"jump":
			points = 100
			force = Vector3.UP * trick_force * 2
			apply_central_force(force)
	
	if points > 0:
		print("BikeController: Wykonano trik: ", trick_name, " (", points, " punktów)")
		trick_performed.emit(trick_name, points)

## Konfiguruje raycasty kół
func _setup_wheel_raycasts() -> void:
	# Konfiguruj raycast przedniego koła
	if not front_wheel_ray:
		front_wheel_ray = RayCast3D.new()
		add_child(front_wheel_ray)
	
	front_wheel_ray.position = Vector3(0, 0, 1)  # Przód roweru
	front_wheel_ray.target_position = Vector3(0, -1, 0)  # W dół
	front_wheel_ray.enabled = true
	
	# Konfiguruj raycast tylnego koła
	if not rear_wheel_ray:
		rear_wheel_ray = RayCast3D.new()
		add_child(rear_wheel_ray)
	
	rear_wheel_ray.position = Vector3(0, 0, -1)  # Tył roweru
	rear_wheel_ray.target_position = Vector3(0, -1, 0)  # W dół
	rear_wheel_ray.enabled = true

## Aktualizuje dźwięki
func _update_audio() -> void:
	# Dźwięk pedałowania
	if pedal_audio:
		if pedal_input > 0 and stamina > 0:
			if not pedal_audio.playing:
				pedal_audio.play()
			pedal_audio.pitch_scale = 0.8 + pedal_input * 0.4
		else:
			pedal_audio.stop()

## Aktualizuje statystyki
func _update_stats() -> void:
	current_speed_kmh = linear_velocity.length() * 3.6
	speed_changed.emit(current_speed_kmh)
	balance_changed.emit(balance)

## Resetuje pozycję roweru
func reset_vehicle() -> void:
	print("BikeController: Resetowanie pozycji roweru")
	
	is_fallen = false
	balance = 0.0
	stamina = max_stamina
	
	# Zatrzymaj rower
	linear_velocity = Vector3.ZERO
	angular_velocity = Vector3.ZERO
	
	# Ustaw prosto
	global_transform.basis = Basis.IDENTITY
	
	# Przenieś lekko w górę
	global_position += Vector3.UP * 2.0

## Zwraca aktualną prędkość w km/h
func get_speed_kmh() -> float:
	return current_speed_kmh

## Zwraca aktualny balans (-1.0 do 1.0)
func get_balance() -> float:
	return balance

## Zwraca aktualną wytrzymałość (0-100)
func get_stamina_percentage() -> float:
	return (stamina / max_stamina) * 100.0

## Sprawdza czy rower jest przewrócony
func is_bike_fallen() -> bool:
	return is_fallen

## Dodaje wytrzymałość
func add_stamina(amount: float) -> void:
	stamina = min(max_stamina, stamina + amount)

## Naprawia rower
func repair_vehicle() -> void:
	print("BikeController: Naprawianie roweru")
	stamina = max_stamina
	if is_fallen:
		_get_back_up()