extends Node
class_name NetworkedVehicle

## Komponenet sieciowy pojazdu - obsługuje synchronizację i autoritet
## Zarządza inputem lokalnym/zdalnym i interpolacją pozycji

@onready var vehicle: VehicleBody3D = get_parent()
@onready var synchronizer: MultiplayerSynchronizer = get_parent().get_node("MultiplayerSynchronizer")

# Dane sieciowe
var network_position := Vector3.ZERO
var network_rotation := Vector3.ZERO
var network_velocity := Vector3.ZERO
var network_angular_velocity := Vector3.ZERO

# Interpolacja
var interpolation_speed := 10.0
var prediction_enabled := true

# Input buffering dla lepszej responsywności
var input_buffer: Array[Dictionary] = []
var max_input_buffer_size := 60  # 1 sekunda przy 60 FPS

# Autoritet
var is_local_authority := false

func _ready() -> void:
	print("NetworkedVehicle: Inicjalizacja sieciowego pojazdu")
	
	# Sprawdź autoritet
	is_local_authority = vehicle.is_multiplayer_authority()
	
	if is_local_authority:
		print("NetworkedVehicle: Lokalny autoritet dla pojazdu")
		# Dodaj do grupy pojazdów gracza
		vehicle.add_to_group("player_vehicle")
	else:
		print("NetworkedVehicle: Zdalny pojazd")
		# Wyłącz fizyczne sterowanie dla zdalnych pojazdów
		_disable_physics_processing()
	
	# Połącz sygnały synchronizatora
	if synchronizer:
		synchronizer.synchronized.connect(_on_synchronized)

func _physics_process(delta: float) -> void:
	if is_local_authority:
		_handle_local_authority(delta)
	else:
		_handle_remote_authority(delta)

## Obsługuje pojazd z lokalnym autoritetem
func _handle_local_authority(delta: float) -> void:
	# Normalnie przetwarzaj input (CarController robi to automatycznie)
	
	# Zapisz stan do buffora input
	if prediction_enabled:
		_add_input_to_buffer()
	
	# Synchronizuj pozycję przez RPC jeśli znacząca zmiana
	_check_sync_threshold()

## Obsługuje pojazd zdalny
func _handle_remote_authority(delta: float) -> void:
	# Interpoluj do pozycji sieciowej
	_interpolate_to_network_state(delta)

## Dodaje input do bufora
func _add_input_to_buffer() -> void:
	if not InputManager:
		return
	
	var input_data = {
		"timestamp": Time.get_time_dict_from_system()["unix"],
		"throttle": InputManager.get_movement_vector().y,
		"steering": InputManager.get_movement_vector().x,
		"handbrake": InputManager.is_action_pressed("handbrake"),
		"nitro": InputManager.is_action_pressed("nitro")
	}
	
	input_buffer.append(input_data)
	
	# Ogranicz rozmiar bufora
	if input_buffer.size() > max_input_buffer_size:
		input_buffer.pop_front()

## Sprawdza próg synchronizacji
func _check_sync_threshold() -> void:
	var position_diff = vehicle.global_position.distance_to(network_position)
	var rotation_diff = vehicle.global_rotation.distance_to(network_rotation)
	
	# Synchronizuj jeśli znacząca różnica
	if position_diff > 0.5 or rotation_diff > 0.1:
		_sync_vehicle_state.rpc_unreliable(
			vehicle.global_position,
			vehicle.global_rotation,
			vehicle.linear_velocity,
			vehicle.angular_velocity
		)

## RPC: Synchronizuje stan pojazdu
@rpc("any_peer", "unreliable")
func _sync_vehicle_state(pos: Vector3, rot: Vector3, lin_vel: Vector3, ang_vel: Vector3) -> void:
	network_position = pos
	network_rotation = rot
	network_velocity = lin_vel
	network_angular_velocity = ang_vel

## Interpoluje do stanu sieciowego
func _interpolate_to_network_state(delta: float) -> void:
	if network_position == Vector3.ZERO:
		return  # Brak danych sieciowych
	
	# Interpolacja pozycji
	vehicle.global_position = vehicle.global_position.lerp(network_position, interpolation_speed * delta)
	
	# Interpolacja rotacji
	var current_basis = vehicle.global_transform.basis
	var target_basis = Basis.from_euler(network_rotation)
	vehicle.global_transform.basis = current_basis.slerp(target_basis, interpolation_speed * delta)
	
	# Predykcja ruchu
	if prediction_enabled:
		_apply_prediction(delta)

## Stosuje predykcję ruchu
func _apply_prediction(delta: float) -> void:
	# Przewiduj pozycję na podstawie prędkości
	var predicted_position = network_position + network_velocity * delta
	
	# Mieszaj z interpolowaną pozycją
	vehicle.global_position = vehicle.global_position.lerp(predicted_position, 0.3)

## Wyłącza przetwarzanie fizyki dla zdalnych pojazdów
func _disable_physics_processing() -> void:
	# Wyłącz input processing w CarController
	if vehicle.has_method("set_physics_process"):
		vehicle.set_physics_process(false)
	
	# Ustaw jako kinematyczny dla lepszej kontroli
	vehicle.freeze_mode = RigidBody3D.FREEZE_MODE_KINEMATIC

## Callback synchronizacji
func _on_synchronized() -> void:
	if not is_local_authority:
		# Aktualizuj dane sieciowe z synchronizatora
		network_position = vehicle.global_position
		network_rotation = vehicle.global_rotation
		network_velocity = vehicle.linear_velocity
		network_angular_velocity = vehicle.angular_velocity

## RPC: Synchronizuje input gracza
@rpc("any_peer", "unreliable")
func _sync_input(throttle: float, steering: float, handbrake: bool, nitro: bool) -> void:
	if is_local_authority:
		return  # Ignoruj input dla lokalnego pojazdu
	
	# Zastosuj zdalny input do pojazdu
	if vehicle.has_method("_apply_remote_input"):
		vehicle._apply_remote_input(throttle, steering, handbrake, nitro)

## Wysyła input do innych graczy
func send_input_to_peers() -> void:
	if not is_local_authority or not InputManager:
		return
	
	var movement = InputManager.get_movement_vector()
	_sync_input.rpc_unreliable(
		movement.y,  # throttle
		movement.x,  # steering
		InputManager.is_action_pressed("handbrake"),
		InputManager.is_action_pressed("nitro")
	)

## RPC: Efekt kolizji
@rpc("any_peer", "reliable")
func _sync_collision_effect(position: Vector3, normal: Vector3, intensity: float) -> void:
	print("NetworkedVehicle: Efekt kolizji w pozycji ", position, " z intensywnością ", intensity)
	# Tutaj można dodać efekty wizualne/dźwiękowe

## RPC: Efekt nitro
@rpc("any_peer", "reliable")
func _sync_nitro_effect(active: bool) -> void:
	print("NetworkedVehicle: Nitro ", "aktywne" if active else "nieaktywne")
	# Tutaj można dodać efekty nitro

## RPC: Resetowanie pojazdu
@rpc("any_peer", "reliable")
func _sync_vehicle_reset(new_position: Vector3, new_rotation: Vector3) -> void:
	vehicle.global_position = new_position
	vehicle.global_rotation = new_rotation
	vehicle.linear_velocity = Vector3.ZERO
	vehicle.angular_velocity = Vector3.ZERO
	
	network_position = new_position
	network_rotation = new_rotation
	network_velocity = Vector3.ZERO
	network_angular_velocity = Vector3.ZERO

## Resetuje pojazd sieciowo
func reset_vehicle_networked() -> void:
	if not is_local_authority:
		return
	
	# Znajdź punkt respawn
	var spawn_point = _find_respawn_point()
	
	# Resetuj lokalnie
	vehicle.global_position = spawn_point.position
	vehicle.global_rotation = spawn_point.rotation
	vehicle.linear_velocity = Vector3.ZERO
	vehicle.angular_velocity = Vector3.ZERO
	
	# Synchronizuj z innymi
	_sync_vehicle_reset.rpc(spawn_point.position, spawn_point.rotation)

## Znajduje punkt respawn
func _find_respawn_point() -> Dictionary:
	# Domyślny punkt respawn
	var default_spawn = {
		"position": Vector3(0, 2, 0),
		"rotation": Vector3.ZERO
	}
	
	# Spróbuj znaleźć spawn point w scenie
	var spawn_points = get_tree().get_nodes_in_group("spawn_points")
	if not spawn_points.is_empty():
		var spawn_point = spawn_points[0]
		return {
			"position": spawn_point.global_position,
			"rotation": spawn_point.global_rotation
		}
	
	return default_spawn

## Ustawia kolor pojazdu sieciowo
@rpc("any_peer", "reliable")
func _sync_vehicle_color(color: Color) -> void:
	var mesh_instance = vehicle.get_node_or_null("CarMesh")
	if mesh_instance and mesh_instance is MeshInstance3D:
		var material = mesh_instance.get_surface_override_material(0)
		if material:
			material = material.duplicate()
			material.albedo_color = color
			mesh_instance.set_surface_override_material(0, material)

## Ustawia kolor pojazdu
func set_vehicle_color(color: Color) -> void:
	if is_local_authority:
		_sync_vehicle_color.rpc(color)
	else:
		_sync_vehicle_color(color)

## Zwraca czy pojazd jest lokalny
func is_local_player() -> bool:
	return is_local_authority

## Zwraca ID gracza-właściciela
func get_owner_id() -> int:
	return vehicle.get_multiplayer_authority()

## Zwraca ping do właściciela
func get_ping_to_owner() -> int:
	if is_local_authority:
		return 0
	
	# Tutaj można zaimplementować pomiar ping
	return 50  # Placeholder

## Włącza/wyłącza predykcję
func set_prediction_enabled(enabled: bool) -> void:
	prediction_enabled = enabled

## Ustawia prędkość interpolacji
func set_interpolation_speed(speed: float) -> void:
	interpolation_speed = clamp(speed, 1.0, 30.0)

## Zwraca opóźnienie sieciowe
func get_network_lag() -> float:
	# Tutaj można zaimplementować pomiar lag
	return 0.05  # 50ms placeholder