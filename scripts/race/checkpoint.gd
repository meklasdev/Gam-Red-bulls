extends Area3D
class_name Checkpoint

## Checkpoint wyścigu - wykrywa przejście pojazdów i emituje sygnały
## Może być używany jako punkt kontrolny, meta lub punkt pośredni

signal vehicle_entered(vehicle: Node3D, checkpoint: Checkpoint)
signal vehicle_exited(vehicle: Node3D, checkpoint: Checkpoint)

@export_group("Checkpoint Settings")
@export var checkpoint_id := 0                    ## Unikalny ID checkpointu
@export var is_start_finish_line := false         ## Czy to linia startu/mety
@export var is_mandatory := true                  ## Czy przejście jest obowiązkowe
@export var checkpoint_name := "Checkpoint"       ## Nazwa checkpointu

@export_group("Visual Settings")
@export var show_debug_info := true               ## Pokazuj informacje debug
@export var highlight_on_approach := true         ## Podświetl przy zbliżaniu
@export var approach_distance := 50.0             ## Dystans podświetlenia

@export_group("Audio")
@export var checkpoint_sound: AudioStream         ## Dźwięk przejścia
@export var volume_db := 0.0                      ## Głośność dźwięku

# Stan checkpointu
var vehicles_inside: Array[Node3D] = []
var total_crossings := 0
var last_crossing_time := 0.0

# Komponenty wizualne
@onready var mesh_instance: MeshInstance3D = $MeshInstance3D
@onready var collision_shape: CollisionShape3D = $CollisionShape3D
@onready var audio_player: AudioStreamPlayer3D = $AudioStreamPlayer3D
@onready var label_3d: Label3D = $Label3D

func _ready() -> void:
	print("Checkpoint: Inicjalizacja checkpointu ", checkpoint_id, " - ", checkpoint_name)
	
	# Połącz sygnały
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	
	# Konfiguruj wygląd
	_setup_visuals()
	_setup_audio()
	
	# Dodaj do grupy checkpointów
	add_to_group("checkpoints")

## Konfiguruje wygląd checkpointu
func _setup_visuals() -> void:
	# Ustaw kolor na podstawie typu
	if mesh_instance and mesh_instance.get_surface_override_material(0):
		var material = mesh_instance.get_surface_override_material(0)
		if is_start_finish_line:
			material.albedo_color = Color.WHITE
		elif is_mandatory:
			material.albedo_color = Color.YELLOW
		else:
			material.albedo_color = Color.CYAN
	
	# Konfiguruj label
	if label_3d:
		if show_debug_info:
			label_3d.text = checkpoint_name + " (" + str(checkpoint_id) + ")"
			label_3d.show()
		else:
			label_3d.hide()

## Konfiguruje dźwięk
func _setup_audio() -> void:
	if audio_player:
		if checkpoint_sound:
			audio_player.stream = checkpoint_sound
		audio_player.volume_db = volume_db

func _process(delta: float) -> void:
	if highlight_on_approach:
		_check_approaching_vehicles()

## Sprawdza zbliżające się pojazdy dla podświetlenia
func _check_approaching_vehicles() -> void:
	if not mesh_instance:
		return
	
	var player_vehicle = _find_player_vehicle()
	if not player_vehicle:
		return
	
	var distance = global_position.distance_to(player_vehicle.global_position)
	
	# Podświetl jeśli pojazd jest blisko
	if distance <= approach_distance:
		_highlight_checkpoint(true)
	else:
		_highlight_checkpoint(false)

## Znajduje pojazd gracza
func _find_player_vehicle() -> Node3D:
	# Szukaj pojazdu gracza w scenie
	var vehicles = get_tree().get_nodes_in_group("player_vehicle")
	if not vehicles.is_empty():
		return vehicles[0]
	
	# Fallback - znajdź pierwszy pojazd
	vehicles = get_tree().get_nodes_in_group("vehicles")
	if not vehicles.is_empty():
		return vehicles[0]
	
	return null

## Podświetla checkpoint
func _highlight_checkpoint(highlight: bool) -> void:
	if not mesh_instance or not mesh_instance.get_surface_override_material(0):
		return
	
	var material = mesh_instance.get_surface_override_material(0)
	
	if highlight:
		material.emission_enabled = true
		material.emission = material.albedo_color * 0.5
	else:
		material.emission_enabled = false

## Callback wejścia pojazdu
func _on_body_entered(body: Node3D) -> void:
	# Sprawdź czy to pojazd
	if not _is_vehicle(body):
		return
	
	if body in vehicles_inside:
		return
	
	vehicles_inside.append(body)
	total_crossings += 1
	last_crossing_time = Time.get_time_dict_from_system()["unix"]
	
	print("Checkpoint: Pojazd ", body.name, " wszedł do checkpointu ", checkpoint_id)
	
	# Odtwórz dźwięk
	_play_checkpoint_sound()
	
	# Efekt wizualny
	_trigger_visual_effect()
	
	# Emituj sygnał
	vehicle_entered.emit(body, self)

## Callback wyjścia pojazdu
func _on_body_exited(body: Node3D) -> void:
	if not _is_vehicle(body):
		return
	
	if body in vehicles_inside:
		vehicles_inside.erase(body)
		print("Checkpoint: Pojazd ", body.name, " opuścił checkpoint ", checkpoint_id)
		vehicle_exited.emit(body, self)

## Sprawdza czy obiekt to pojazd
func _is_vehicle(body: Node3D) -> bool:
	# Sprawdź po typie klasy
	if body is CarController or body is BikeController or body is MotoController:
		return true
	
	# Sprawdź po grupie
	if body.is_in_group("vehicles"):
		return true
	
	# Sprawdź po parent (dla kół VehicleBody3D)
	var parent = body.get_parent()
	if parent and (parent is VehicleBody3D or parent is RigidBody3D):
		return _is_vehicle(parent)
	
	return false

## Odtwarza dźwięk checkpointu
func _play_checkpoint_sound() -> void:
	if audio_player and checkpoint_sound:
		audio_player.play()

## Wywołuje efekt wizualny
func _trigger_visual_effect() -> void:
	if not mesh_instance:
		return
	
	# Animacja pulsowania
	var tween = create_tween()
	tween.set_loops(3)
	tween.tween_property(mesh_instance, "scale", Vector3.ONE * 1.2, 0.1)
	tween.tween_property(mesh_instance, "scale", Vector3.ONE, 0.1)

## Zwraca liczbę pojazdów wewnątrz
func get_vehicle_count() -> int:
	return vehicles_inside.size()

## Zwraca listę pojazdów wewnątrz
func get_vehicles_inside() -> Array[Node3D]:
	return vehicles_inside.duplicate()

## Sprawdza czy pojazd jest wewnątrz
func is_vehicle_inside(vehicle: Node3D) -> bool:
	return vehicle in vehicles_inside

## Zwraca całkowitą liczbę przejść
func get_total_crossings() -> int:
	return total_crossings

## Zwraca czas ostatniego przejścia
func get_last_crossing_time() -> float:
	return last_crossing_time

## Resetuje statystyki checkpointu
func reset_stats() -> void:
	vehicles_inside.clear()
	total_crossings = 0
	last_crossing_time = 0.0
	print("Checkpoint: Zresetowano statystyki checkpointu ", checkpoint_id)

## Ustawia aktywność checkpointu
func set_active(active: bool) -> void:
	monitoring = active
	monitorable = active
	
	if mesh_instance:
		mesh_instance.visible = active
	
	if collision_shape:
		collision_shape.disabled = not active

## Sprawdza czy checkpoint jest aktywny
func is_active() -> bool:
	return monitoring

## Ustawia kolor checkpointu
func set_checkpoint_color(color: Color) -> void:
	if mesh_instance and mesh_instance.get_surface_override_material(0):
		var material = mesh_instance.get_surface_override_material(0)
		material.albedo_color = color

## Ustawia tekst labela
func set_label_text(text: String) -> void:
	if label_3d:
		label_3d.text = text

## Pokazuje/ukrywa label
func set_label_visible(visible: bool) -> void:
	if label_3d:
		label_3d.visible = visible

## Zwraca pozycję checkpointu
func get_checkpoint_position() -> Vector3:
	return global_position

## Zwraca kierunek checkpointu (do następnego)
func get_checkpoint_direction() -> Vector3:
	# Domyślnie kierunek do przodu
	return -global_transform.basis.z

## Sprawdza czy pojazd przeszedł checkpoint w prawidłowym kierunku
func is_crossing_valid(vehicle: Node3D, entry_position: Vector3, exit_position: Vector3) -> bool:
	# Oblicz kierunek przejścia
	var crossing_direction = (exit_position - entry_position).normalized()
	var checkpoint_direction = get_checkpoint_direction()
	
	# Sprawdź czy kierunki są zgodne (dot product > 0)
	return crossing_direction.dot(checkpoint_direction) > 0.0

## Ustawia dźwięk checkpointu
func set_checkpoint_sound(sound: AudioStream, volume: float = 0.0) -> void:
	checkpoint_sound = sound
	volume_db = volume
	_setup_audio()

## Zwraca informacje o checkpoincie
func get_checkpoint_info() -> Dictionary:
	return {
		"id": checkpoint_id,
		"name": checkpoint_name,
		"position": global_position,
		"is_start_finish": is_start_finish_line,
		"is_mandatory": is_mandatory,
		"vehicles_inside": vehicles_inside.size(),
		"total_crossings": total_crossings,
		"last_crossing": last_crossing_time,
		"is_active": is_active()
	}