extends Control
class_name MinimapController

## Kontroler minimapy - zarządza kamerą i znacznikami na minimapie
## Pokazuje pozycję gracza, checkpointy, cele misji i innych graczy

@onready var minimap_viewport: SubViewport = $MinimapViewport
@onready var minimap_camera: Camera3D = $MinimapViewport/MinimapCamera
@onready var player_marker: Control = $PlayerMarker
@onready var markers_container: Control = $MarkersContainer

# Śledzony obiekt (zazwyczaj pojazd gracza)
var tracked_object: Node3D = null

# Ustawienia minimapy
@export var zoom_level := 100.0          ## Poziom powiększenia
@export var follow_rotation := true      ## Czy obrócić mapę z pojazdem
@export var update_frequency := 0.1      ## Częstotliwość aktualizacji (sekundy)

# Znaczniki na mapie
var markers := {}  ## Dictionary[String, Control] - ID -> marker
var marker_colors := {
	"checkpoint": Color.YELLOW,
	"mission": Color.GREEN,
	"player": Color.BLUE,
	"enemy": Color.RED,
	"pickup": Color.CYAN,
	"waypoint": Color.WHITE
}

var update_timer := 0.0

func _ready() -> void:
	print("MinimapController: Inicjalizacja minimapy")
	
	# Konfiguruj kamerę minimapy
	_setup_camera()
	
	# Ukryj znacznik gracza na początku
	if player_marker:
		player_marker.hide()

func _process(delta: float) -> void:
	update_timer += delta
	
	if update_timer >= update_frequency:
		update_timer = 0.0
		_update_minimap()

## Konfiguruje kamerę minimapy
func _setup_camera() -> void:
	if not minimap_camera:
		return
	
	# Ustaw projekcję ortogonalną (widok z góry)
	minimap_camera.projection = PROJECTION_ORTHOGONAL
	minimap_camera.size = zoom_level
	
	# Pozycja nad światem, patrzenie w dół
	minimap_camera.position = Vector3(0, 100, 0)
	minimap_camera.rotation_degrees = Vector3(-90, 0, 0)
	
	print("MinimapController: Kamera minimapy skonfigurowana")

## Ustawia obiekt do śledzenia
func set_tracked_object(object: Node3D) -> void:
	tracked_object = object
	
	if object:
		print("MinimapController: Śledzenie obiektu: ", object.name)
		
		# Pokaż znacznik gracza
		if player_marker:
			player_marker.show()
	else:
		# Ukryj znacznik gracza
		if player_marker:
			player_marker.hide()

## Aktualizuje pozycję kamery i znaczników
func _update_minimap() -> void:
	if not tracked_object or not minimap_camera:
		return
	
	# Aktualizuj pozycję kamery nad śledzonym obiektem
	var target_pos = tracked_object.global_position
	target_pos.y = 100  # Wysokość kamery
	minimap_camera.global_position = target_pos
	
	# Obróć kamerę jeśli włączona rotacja
	if follow_rotation:
		var target_rotation = tracked_object.global_rotation
		minimap_camera.global_rotation = Vector3(-90, target_rotation.y, 0)
	
	# Aktualizuj pozycję znacznika gracza na UI
	_update_player_marker()
	
	# Aktualizuj inne znaczniki
	_update_markers()

## Aktualizuje pozycję znacznika gracza na UI
func _update_player_marker() -> void:
	if not player_marker or not tracked_object:
		return
	
	# Znacznik gracza zawsze w środku minimapy
	var center = size / 2
	player_marker.position = center - player_marker.size / 2
	
	# Obróć znacznik jeśli nie obracamy całej mapy
	if not follow_rotation:
		var rotation = tracked_object.global_rotation.y
		player_marker.rotation = rotation

## Aktualizuje pozycje znaczników na mapie
func _update_markers() -> void:
	if not tracked_object or not minimap_camera:
		return
	
	for marker_id in markers:
		var marker_data = markers[marker_id]
		var marker_ui = marker_data.get("ui")
		var world_pos = marker_data.get("position", Vector3.ZERO)
		
		if marker_ui and marker_ui is Control:
			# Przekształć pozycję świata na pozycję UI
			var screen_pos = _world_to_minimap_position(world_pos)
			
			# Sprawdź czy znacznik jest w zasięgu minimapy
			if _is_position_in_minimap(screen_pos):
				marker_ui.position = screen_pos - marker_ui.size / 2
				marker_ui.show()
			else:
				marker_ui.hide()

## Przekształca pozycję świata na pozycję minimapy
func _world_to_minimap_position(world_pos: Vector3) -> Vector2:
	if not tracked_object or not minimap_camera:
		return Vector2.ZERO
	
	# Oblicz względną pozycję od gracza
	var relative_pos = world_pos - tracked_object.global_position
	
	# Przekształć na koordynaty minimapy
	var scale = size.x / zoom_level  # Skala pikseli na metr
	var minimap_pos = Vector2(relative_pos.x, -relative_pos.z) * scale
	
	# Dodaj offset do środka minimapy
	minimap_pos += size / 2
	
	# Obróć jeśli włączona rotacja
	if follow_rotation:
		var rotation = -tracked_object.global_rotation.y
		minimap_pos = minimap_pos.rotated(rotation)
		minimap_pos += size / 2
	
	return minimap_pos

## Sprawdza czy pozycja jest widoczna na minimapie
func _is_position_in_minimap(pos: Vector2) -> bool:
	return pos.x >= 0 and pos.x <= size.x and pos.y >= 0 and pos.y <= size.y

## Dodaje znacznik na mapę
func add_marker(id: String, world_position: Vector3, marker_type: String = "waypoint") -> void:
	# Usuń stary znacznik jeśli istnieje
	remove_marker(id)
	
	# Utwórz UI znacznika
	var marker_ui = _create_marker_ui(marker_type)
	if not marker_ui:
		return
	
	markers_container.add_child(marker_ui)
	
	# Zapisz dane znacznika
	markers[id] = {
		"ui": marker_ui,
		"position": world_position,
		"type": marker_type
	}
	
	print("MinimapController: Dodano znacznik ", id, " typu ", marker_type)

## Tworzy UI znacznika
func _create_marker_ui(marker_type: String) -> Control:
	var marker = ColorRect.new()
	marker.size = Vector2(8, 8)
	marker.color = marker_colors.get(marker_type, Color.WHITE)
	
	# Dodaj obramowanie
	var border = ColorRect.new()
	border.size = Vector2(10, 10)
	border.color = Color.BLACK
	border.position = Vector2(-1, -1)
	marker.add_child(border)
	marker.move_child(border, 0)  # Przenieś na spód
	
	return marker

## Usuwa znacznik z mapy
func remove_marker(id: String) -> void:
	if id in markers:
		var marker_data = markers[id]
		var marker_ui = marker_data.get("ui")
		
		if marker_ui:
			marker_ui.queue_free()
		
		markers.erase(id)
		print("MinimapController: Usunięto znacznik ", id)

## Aktualizuje pozycję znacznika
func update_marker_position(id: String, world_position: Vector3) -> void:
	if id in markers:
		markers[id]["position"] = world_position

## Ustawia poziom powiększenia
func set_zoom_level(zoom: float) -> void:
	zoom_level = clamp(zoom, 50.0, 500.0)
	
	if minimap_camera:
		minimap_camera.size = zoom_level

## Przełącza tryb rotacji
func toggle_rotation_follow() -> void:
	follow_rotation = not follow_rotation
	print("MinimapController: Rotacja mapy: ", follow_rotation)

## Czyści wszystkie znaczniki
func clear_markers() -> void:
	for marker_id in markers:
		var marker_data = markers[marker_id]
		var marker_ui = marker_data.get("ui")
		if marker_ui:
			marker_ui.queue_free()
	
	markers.clear()
	print("MinimapController: Wyczyszczono wszystkie znaczniki")

## Dodaje znaczniki checkpointów
func add_checkpoint_markers(checkpoint_positions: Array[Vector3]) -> void:
	for i in range(checkpoint_positions.size()):
		var checkpoint_id = "checkpoint_" + str(i)
		add_marker(checkpoint_id, checkpoint_positions[i], "checkpoint")

## Dodaje znaczniki misji
func add_mission_markers(mission_positions: Array[Vector3]) -> void:
	for i in range(mission_positions.size()):
		var mission_id = "mission_" + str(i)
		add_marker(mission_id, mission_positions[i], "mission")

## Dodaje znacznik gracza multiplayer
func add_player_marker(player_id: String, world_position: Vector3) -> void:
	add_marker("player_" + player_id, world_position, "player")

## Zwraca aktualny poziom powiększenia
func get_zoom_level() -> float:
	return zoom_level

## Sprawdza czy śledzony obiekt jest ustawiony
func has_tracked_object() -> bool:
	return tracked_object != null