extends RigidBody3D
class_name MotoController

## Kontroler motocykla - używa RigidBody3D z zaawansowaną fizyką balansowania
## Szybszy i potężniejszy niż rower, ale wymaga większej umiejętności

signal speed_changed(speed_kmh: float)
signal gear_changed(gear: int)
signal balance_changed(balance: float)
signal wheelie_started()
signal wheelie_ended(duration: float)
signal stoppie_performed(points: int)

@export_group("Silnik")
@export var engine_power := 2000.0            ## Moc silnika
@export var max_speed_kmh := 200.0            ## Maksymalna prędkość
@export var max_rpm := 8000.0                 ## Maksymalne obroty
@export var torque_curve: Curve               ## Krzywa momentu obrotowego

@export_group("Transmisja")
@export var gear_ratios := [0.0, 3.5, 2.1, 1.4, 1.0, 0.8, 0.6]  ## Przełożenia (0=luz)
@export var final_drive := 3.2                ## Przełożenie główne
@export var clutch_engagement := 0.8          ## Próg sprzęgła

@export_group("Balansowanie")
@export var balance_sensitivity := 3.0        ## Czułość balansowania
@export var lean_angle_max := 50.0           ## Maksymalny kąt przechyłu
@export var stability_factor := 15.0          ## Współczynnik stabilności
@export var gyroscopic_effect := 25.0         ## Efekt żyroskopowy

@export_group("Hamowanie")
@export var front_brake_force := 120.0        ## Siła hamulca przedniego
@export var rear_brake_force := 80.0          ## Siła hamulca tylnego
@export var abs_enabled := true               ## System ABS

@export_group("Wheelie/Stoppie")
@export var wheelie_threshold := 0.7          ## Próg do wheelie
@export var stoppie_threshold := 0.8          ## Próg do stoppie
@export var balance_assist := true            ## Asystent balansowania

@export_group("Dźwięki")
@export var engine_audio: AudioStreamPlayer3D
@export var exhaust_audio: AudioStreamPlayer3D
@export var tire_screech_audio: AudioStreamPlayer3D
@export var crash_audio: AudioStreamPlayer3D

# Stan motocykla
var current_speed_kmh := 0.0
var current_rpm := 1000.0
var current_gear := 1
var balance := 0.0  # -1.0 do 1.0
var lean_angle := 0.0  # kąt przechyłu w stopniach
var is_wheelie := false
var is_stoppie := false
var wheelie_start_time := 0.0

# Input
var throttle_input := 0.0
var front_brake_input := 0.0
var rear_brake_input := 0.0
var steer_input := 0.0
var balance_input := 0.0
var gear_up_input := false
var gear_down_input := false

# Fizyka
var front_wheel_contact := true
var rear_wheel_contact := true
var front_wheel_grip := 1.0
var rear_wheel_grip := 1.0

# Raycasty dla kół
@onready var front_wheel_ray: RayCast3D = $FrontWheelRay
@onready var rear_wheel_ray: RayCast3D = $RearWheelRay

# Mesh i efekty
@onready var moto_mesh: MeshInstance3D = $MotoMesh
@onready var rider_mesh: MeshInstance3D = $RiderMesh
@onready var exhaust_particles: GPUParticles3D = $ExhaustParticles

func _ready() -> void:
	print("MotoController: Inicjalizacja motocykla")
	
	# Ustaw podstawowe parametry fizyki
	mass = 200.0  # kg
	gravity_scale = 1.0
	
	# Ustaw środek masy niżej dla stabilności
	center_of_mass_offset = Vector3(0, -0.3, 0)
	
	# Konfiguruj raycasty kół
	_setup_wheel_raycasts()
	
	# Inicjalizuj krzywą momentu jeśli nie została ustawiona
	if not torque_curve:
		_create_default_torque_curve()

func _physics_process(delta: float) -> void:
	_handle_input()
	_update_wheel_contact()
	_apply_engine_force(delta)
	_apply_braking(delta)
	_apply_steering(delta)
	_update_balance(delta)
	_update_transmission(delta)
	_check_wheelie_stoppie()
	_update_audio()
	_update_effects()
	_update_stats()

## Obsługuje wejście gracza
func _handle_input() -> void:
	if InputManager:
		var movement = InputManager.get_movement_vector()
		throttle_input = max(0.0, movement.y)
		
		# Hamulce - tylny na cofanie, przedni na spację
		rear_brake_input = max(0.0, -movement.y)
		front_brake_input = 1.0 if InputManager.is_action_pressed("handbrake") else 0.0
		
		steer_input = movement.x
		
		# Balansowanie - subtelne ruchy
		balance_input = steer_input * 0.3
		
		# Zmiana biegów
		gear_up_input = InputManager.is_action_just_pressed("stunt_next")
		gear_down_input = InputManager.is_action_just_pressed("stunt_prev")
	else:
		# Fallback
		throttle_input = Input.get_action_strength("move_forward")
		rear_brake_input = Input.get_action_strength("move_backward")
		front_brake_input = 1.0 if Input.is_action_pressed("handbrake") else 0.0
		steer_input = Input.get_action_strength("steer_right") - Input.get_action_strength("steer_left")
		balance_input = steer_input * 0.3

## Aktualizuje kontakt kół z podłożem
func _update_wheel_contact() -> void:
	front_wheel_contact = front_wheel_ray.is_colliding()
	rear_wheel_contact = rear_wheel_ray.is_colliding()
	
	# Oblicz przyczepność na podstawie kontaktu i pogody
	var weather_grip = 1.0
	if WeatherManager:
		weather_grip = WeatherManager.get_grip_multiplier()
	
	front_wheel_grip = 1.0 if front_wheel_contact else 0.0
	rear_wheel_grip = 1.0 if rear_wheel_contact else 0.0
	
	front_wheel_grip *= weather_grip
	rear_wheel_grip *= weather_grip

## Stosuje siłę silnika
func _apply_engine_force(delta: float) -> void:
	if current_gear == 0 or not rear_wheel_contact:
		return  # Luz lub brak kontaktu
	
	# Oblicz moment obrotowy z krzywej
	var rpm_normalized = current_rpm / max_rpm
	var torque_multiplier = torque_curve.sample(rpm_normalized) if torque_curve else 1.0
	
	# Oblicz siłę na kole
	var gear_ratio = gear_ratios[current_gear] * final_drive
	var wheel_force = (engine_power * torque_multiplier * throttle_input) / gear_ratio
	
	# Zastosuj przyczepność
	wheel_force *= rear_wheel_grip
	
	# Kierunek ruchu
	var forward = -global_transform.basis.z
	
	# Ograniczenie prędkości
	if current_speed_kmh < max_speed_kmh:
		apply_central_force(forward * wheel_force)

## Stosuje hamowanie
func _apply_braking(delta: float) -> void:
	var total_brake_force = Vector3.ZERO
	var forward = -global_transform.basis.z
	
	# Hamulec przedni
	if front_brake_input > 0 and front_wheel_contact:
		var front_force = front_brake_force * front_brake_input * front_wheel_grip
		total_brake_force -= forward * front_force
		
		# Sprawdź możliwość stoppie
		if front_brake_input > stoppie_threshold and current_speed_kmh > 30:
			_perform_stoppie()
	
	# Hamulec tylny
	if rear_brake_input > 0 and rear_wheel_contact:
		var rear_force = rear_brake_force * rear_brake_input * rear_wheel_grip
		total_brake_force -= forward * rear_force
	
	# System ABS - zapobiega blokowaniu kół
	if abs_enabled:
		var max_brake = current_speed_kmh * 20  # Uproszczony ABS
		total_brake_force = total_brake_force.limit_length(max_brake)
	
	if total_brake_force.length() > 0:
		apply_central_force(total_brake_force)

## Stosuje kierowanie
func _apply_steering(delta: float) -> void:
	if abs(steer_input) > 0.1:
		# Siła kierowania zależy od prędkości
		var speed_factor = clamp(current_speed_kmh / 50.0, 0.3, 1.0)
		var steer_force = steer_input * 30.0 * speed_factor
		
		# Efekt żyroskopowy - opór przy szybkich zmianach kierunku
		var gyro_resistance = gyroscopic_effect * current_speed_kmh / 100.0
		steer_force /= (1.0 + gyro_resistance)
		
		apply_torque(Vector3.UP * steer_force)

## Aktualizuje balansowanie i przechył
func _update_balance(delta: float) -> void:
	# Oblicz docelowy kąt przechyłu na podstawie skrętu
	var target_lean = steer_input * lean_angle_max * (current_speed_kmh / 100.0)
	
	# Wpływ gracza na balansowanie
	balance += balance_input * balance_sensitivity * delta
	
	# Automatyczna stabilizacja
	var stability_force = stability_factor * (current_speed_kmh / 50.0)
	balance = lerp(balance, 0.0, stability_force * delta)
	
	# Ograniczenie balansu
	balance = clamp(balance, -1.0, 1.0)
	
	# Oblicz rzeczywisty kąt przechyłu
	lean_angle = lerp(lean_angle, target_lean + balance * 20.0, delta * 8.0)
	lean_angle = clamp(lean_angle, -lean_angle_max, lean_angle_max)
	
	# Zastosuj przechył do transformacji
	var lean_rotation = deg_to_rad(lean_angle)
	var current_rotation = global_transform.basis.get_euler()
	current_rotation.z = lean_rotation
	global_transform.basis = Basis.from_euler(current_rotation)
	
	balance_changed.emit(balance)

## Aktualizuje transmisję
func _update_transmission(delta: float) -> void:
	# Automatyczna zmiana biegów na podstawie obrotów
	if gear_up_input and current_gear < gear_ratios.size() - 1:
		current_gear += 1
		gear_changed.emit(current_gear)
		print("MotoController: Bieg w górę -> ", current_gear)
	elif gear_down_input and current_gear > 0:
		current_gear -= 1
		gear_changed.emit(current_gear)
		print("MotoController: Bieg w dół -> ", current_gear)
	
	# Oblicz obroty silnika na podstawie prędkości i biegu
	if current_gear > 0:
		var gear_ratio = gear_ratios[current_gear] * final_drive
		var wheel_rpm = (current_speed_kmh / 3.6) * 60 / (2 * PI * 0.3)  # Promień koła 0.3m
		var target_rpm = wheel_rpm * gear_ratio
		
		# Dodaj obroty od gazu
		target_rpm += throttle_input * 2000
		
		current_rpm = lerp(current_rpm, clamp(target_rpm, 1000, max_rpm), delta * 5.0)
	else:
		# Luz - obroty na biegu jałowym + gaz
		current_rpm = lerp(current_rpm, 1000 + throttle_input * 3000, delta * 3.0)

## Sprawdza wheelie i stoppie
func _check_wheelie_stoppie() -> void:
	# Wheelie - sprawdź czy tylne koło jest na ziemi, a przednie nie
	var potential_wheelie = rear_wheel_contact and not front_wheel_contact and throttle_input > wheelie_threshold
	
	if potential_wheelie and not is_wheelie:
		is_wheelie = true
		wheelie_start_time = Time.get_time_dict_from_system()["unix"]
		wheelie_started.emit()
		print("MotoController: Wheelie rozpoczęte!")
	elif not potential_wheelie and is_wheelie:
		is_wheelie = false
		var duration = Time.get_time_dict_from_system()["unix"] - wheelie_start_time
		wheelie_ended.emit(duration)
		print("MotoController: Wheelie zakończone, czas: ", duration, "s")
	
	# Stoppie jest obsługiwane w _apply_braking()

## Wykonuje stoppie
func _perform_stoppie() -> void:
	if is_stoppie:
		return
	
	is_stoppie = true
	var points = int(current_speed_kmh * 5)  # Punkty na podstawie prędkości
	stoppie_performed.emit(points)
	print("MotoController: Stoppie wykonane! Punkty: ", points)
	
	# Resetuj po chwili
	await get_tree().create_timer(0.5).timeout
	is_stoppie = false

## Konfiguruje raycasty kół
func _setup_wheel_raycasts() -> void:
	if not front_wheel_ray:
		front_wheel_ray = RayCast3D.new()
		add_child(front_wheel_ray)
	
	front_wheel_ray.position = Vector3(0, 0, 1.2)  # Przód motocykla
	front_wheel_ray.target_position = Vector3(0, -0.8, 0)
	front_wheel_ray.enabled = true
	
	if not rear_wheel_ray:
		rear_wheel_ray = RayCast3D.new()
		add_child(rear_wheel_ray)
	
	rear_wheel_ray.position = Vector3(0, 0, -1.2)  # Tył motocykla
	rear_wheel_ray.target_position = Vector3(0, -0.8, 0)
	rear_wheel_ray.enabled = true

## Tworzy domyślną krzywą momentu obrotowego
func _create_default_torque_curve() -> void:
	torque_curve = Curve.new()
	torque_curve.add_point(0.0, 0.3)    # Niskie obroty
	torque_curve.add_point(0.3, 0.8)    # Średnie obroty - maksymalny moment
	torque_curve.add_point(0.7, 1.0)    # Wysokie obroty - maksymalna moc
	torque_curve.add_point(1.0, 0.7)    # Bardzo wysokie - spadek mocy

## Aktualizuje dźwięki
func _update_audio() -> void:
	# Dźwięk silnika
	if engine_audio:
		var engine_pitch = 0.6 + (current_rpm / max_rpm) * 1.0
		engine_audio.pitch_scale = engine_pitch
		engine_audio.volume_db = linear_to_db(0.4 + throttle_input * 0.5)
		
		if not engine_audio.playing:
			engine_audio.play()
	
	# Dźwięk wydechu
	if exhaust_audio and throttle_input > 0.5:
		if not exhaust_audio.playing:
			exhaust_audio.play()
		exhaust_audio.volume_db = linear_to_db(throttle_input * 0.6)
	elif exhaust_audio:
		exhaust_audio.stop()

## Aktualizuje efekty wizualne
func _update_effects() -> void:
	# Cząsteczki wydechu
	if exhaust_particles:
		exhaust_particles.emitting = throttle_input > 0.3
		if exhaust_particles.process_material is ParticleProcessMaterial:
			var material = exhaust_particles.process_material as ParticleProcessMaterial
			material.initial_velocity_min = 5.0 + throttle_input * 10.0
			material.initial_velocity_max = 10.0 + throttle_input * 15.0

## Aktualizuje statystyki
func _update_stats() -> void:
	current_speed_kmh = linear_velocity.length() * 3.6
	speed_changed.emit(current_speed_kmh)

## Resetuje pozycję motocykla
func reset_vehicle() -> void:
	print("MotoController: Resetowanie pozycji motocykla")
	
	# Resetuj stany
	is_wheelie = false
	is_stoppie = false
	balance = 0.0
	lean_angle = 0.0
	current_gear = 1
	
	# Zatrzymaj motocykl
	linear_velocity = Vector3.ZERO
	angular_velocity = Vector3.ZERO
	
	# Ustaw prosto
	global_transform.basis = Basis.IDENTITY
	global_position += Vector3.UP * 2.0

## Zwraca aktualną prędkość w km/h
func get_speed_kmh() -> float:
	return current_speed_kmh

## Zwraca aktualne obroty silnika
func get_rpm() -> float:
	return current_rpm

## Zwraca aktualny bieg
func get_gear() -> int:
	return current_gear

## Zwraca aktualny balans
func get_balance() -> float:
	return balance

## Zwraca kąt przechyłu w stopniach
func get_lean_angle() -> float:
	return lean_angle

## Sprawdza czy wykonuje wheelie
func is_doing_wheelie() -> bool:
	return is_wheelie

## Sprawdza czy wykonuje stoppie
func is_doing_stoppie() -> bool:
	return is_stoppie

## Naprawia motocykl
func repair_vehicle() -> void:
	print("MotoController: Naprawianie motocykla")
	reset_vehicle()