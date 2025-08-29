extends Node
class_name MobileOptimization

## System optymalizacji mobilnej - zarządza wydajnością na urządzeniach mobilnych
## Automatycznie dostosowuje jakość grafiki, LOD i efekty na podstawie wydajności

signal performance_changed(performance_level: PerformanceLevel)
signal fps_target_changed(new_target: int)

enum PerformanceLevel {
	LOW,        ## Niska wydajność - maksymalne optymalizacje
	MEDIUM,     ## Średnia wydajność - zbalansowane ustawienia  
	HIGH,       ## Wysoka wydajność - lepsza jakość
	ULTRA       ## Najwyższa wydajność - pełna jakość
}

enum QualitySettings {
	SHADOWS,
	PARTICLES,
	TEXTURES,
	EFFECTS,
	LOD_DISTANCE,
	RENDER_SCALE
}

# Ustawienia wydajności
@export var target_fps := 60
@export var auto_adjust_quality := true
@export var performance_monitoring_interval := 2.0

# Monitoring wydajności
var fps_history: Array[float] = []
var frame_time_history: Array[float] = []
var memory_history: Array[int] = []
var current_performance_level := PerformanceLevel.MEDIUM

# Timery
var performance_timer := 0.0
var adjustment_cooldown := 0.0
var adjustment_cooldown_time := 5.0  # Sekundy między dostosowaniami

# Ustawienia jakości dla różnych poziomów wydajności
var quality_presets := {
	PerformanceLevel.LOW: {
		QualitySettings.SHADOWS: false,
		QualitySettings.PARTICLES: 0.3,
		QualitySettings.TEXTURES: 0.5,
		QualitySettings.EFFECTS: 0.2,
		QualitySettings.LOD_DISTANCE: 0.3,
		QualitySettings.RENDER_SCALE: 0.7
	},
	PerformanceLevel.MEDIUM: {
		QualitySettings.SHADOWS: true,
		QualitySettings.PARTICLES: 0.6,
		QualitySettings.TEXTURES: 0.75,
		QualitySettings.EFFECTS: 0.5,
		QualitySettings.LOD_DISTANCE: 0.6,
		QualitySettings.RENDER_SCALE: 0.85
	},
	PerformanceLevel.HIGH: {
		QualitySettings.SHADOWS: true,
		QualitySettings.PARTICLES: 0.8,
		QualitySettings.TEXTURES: 1.0,
		QualitySettings.EFFECTS: 0.8,
		QualitySettings.LOD_DISTANCE: 0.8,
		QualitySettings.RENDER_SCALE: 1.0
	},
	PerformanceLevel.ULTRA: {
		QualitySettings.SHADOWS: true,
		QualitySettings.PARTICLES: 1.0,
		QualitySettings.TEXTURES: 1.0,
		QualitySettings.EFFECTS: 1.0,
		QualitySettings.LOD_DISTANCE: 1.0,
		QualitySettings.RENDER_SCALE: 1.0
	}
}

# Cache obiektów do optymalizacji
var cached_lights: Array[Light3D] = []
var cached_particles: Array[GPUParticles3D] = []
var cached_mesh_instances: Array[MeshInstance3D] = []
var lod_objects: Array[Dictionary] = []

func _ready() -> void:
	print("MobileOptimization: Inicjalizacja systemu optymalizacji mobilnej")
	
	# Wykryj czy jesteśmy na urządzeniu mobilnym
	if OS.has_feature("mobile"):
		print("MobileOptimization: Wykryto urządzenie mobilne")
		_detect_device_performance()
	else:
		print("MobileOptimization: Platforma desktop - ustawienia wysokiej wydajności")
		current_performance_level = PerformanceLevel.HIGH
	
	# Zastosuj początkowe ustawienia
	_apply_performance_settings()
	
	# Uruchom monitoring jeśli włączony
	if auto_adjust_quality:
		_start_performance_monitoring()

func _process(delta: float) -> void:
	if auto_adjust_quality:
		_update_performance_monitoring(delta)
	
	# Aktualizuj LOD
	_update_lod_system(delta)

## Wykrywa wydajność urządzenia na podstawie specyfikacji
func _detect_device_performance() -> void:
	# Pobierz informacje o systemie
	var processor_count = OS.get_processor_count()
	var memory_mb = OS.get_static_memory_usage_by_type()
	
	print("MobileOptimization: Procesory: ", processor_count)
	print("MobileOptimization: Pamięć: ", memory_mb)
	
	# Heurystyka wykrywania wydajności
	if processor_count >= 8:
		current_performance_level = PerformanceLevel.HIGH
	elif processor_count >= 4:
		current_performance_level = PerformanceLevel.MEDIUM  
	else:
		current_performance_level = PerformanceLevel.LOW
	
	# Dodatkowe sprawdzenie na podstawie rozdzielczości ekranu
	var screen_size = DisplayServer.screen_get_size()
	var total_pixels = screen_size.x * screen_size.y
	
	if total_pixels > 2073600:  # Powyżej 1920x1080
		# Zmniejsz poziom wydajności dla wysokich rozdzielczości
		if current_performance_level > PerformanceLevel.LOW:
			current_performance_level -= 1
	
	print("MobileOptimization: Wykryty poziom wydajności: ", PerformanceLevel.keys()[current_performance_level])

## Rozpoczyna monitoring wydajności
func _start_performance_monitoring() -> void:
	print("MobileOptimization: Uruchamianie monitoringu wydajności")
	performance_timer = 0.0

## Aktualizuje monitoring wydajności
func _update_performance_monitoring(delta: float) -> void:
	performance_timer += delta
	adjustment_cooldown -= delta
	
	if performance_timer >= performance_monitoring_interval:
		performance_timer = 0.0
		_collect_performance_data()
		
		# Dostosuj jakość jeśli cooldown minął
		if adjustment_cooldown <= 0.0:
			_auto_adjust_quality()

## Zbiera dane wydajności
func _collect_performance_data() -> void:
	var current_fps = Engine.get_frames_per_second()
	var frame_time = 1.0 / max(current_fps, 1.0) * 1000.0  # w milisekundach
	var memory_usage = OS.get_static_memory_usage_by_type().values().reduce(func(a, b): return a + b, 0)
	
	# Dodaj do historii
	fps_history.append(current_fps)
	frame_time_history.append(frame_time)
	memory_history.append(memory_usage)
	
	# Ogranicz rozmiar historii
	var max_history = 10
	if fps_history.size() > max_history:
		fps_history.pop_front()
		frame_time_history.pop_front()
		memory_history.pop_front()

## Automatycznie dostosowuje jakość na podstawie wydajności
func _auto_adjust_quality() -> void:
	if fps_history.is_empty():
		return
	
	# Oblicz średnie FPS
	var avg_fps = fps_history.reduce(func(a, b): return a + b, 0.0) / fps_history.size()
	var target_tolerance = target_fps * 0.1  # 10% tolerancji
	
	var should_adjust = false
	var new_level = current_performance_level
	
	# Jeśli FPS zbyt niskie - zmniejsz jakość
	if avg_fps < target_fps - target_tolerance:
		if current_performance_level > PerformanceLevel.LOW:
			new_level = current_performance_level - 1
			should_adjust = true
			print("MobileOptimization: FPS zbyt niskie (", avg_fps, "), zmniejszanie jakości")
	
	# Jeśli FPS znacznie powyżej celu - zwiększ jakość
	elif avg_fps > target_fps + target_tolerance * 2:
		if current_performance_level < PerformanceLevel.ULTRA:
			new_level = current_performance_level + 1
			should_adjust = true
			print("MobileOptimization: FPS wysokie (", avg_fps, "), zwiększanie jakości")
	
	if should_adjust:
		set_performance_level(new_level)
		adjustment_cooldown = adjustment_cooldown_time

## Stosuje ustawienia wydajności
func _apply_performance_settings() -> void:
	var settings = quality_presets[current_performance_level]
	
	print("MobileOptimization: Stosowanie ustawień dla poziomu: ", PerformanceLevel.keys()[current_performance_level])
	
	# Zastosuj ustawienia renderowania
	_apply_render_settings(settings)
	
	# Zastosuj ustawienia cieni
	_apply_shadow_settings(settings[QualitySettings.SHADOWS])
	
	# Zastosuj ustawienia cząsteczek
	_apply_particle_settings(settings[QualitySettings.PARTICLES])
	
	# Zastosuj ustawienia tekstur
	_apply_texture_settings(settings[QualitySettings.TEXTURES])
	
	# Zastosuj ustawienia LOD
	_apply_lod_settings(settings[QualitySettings.LOD_DISTANCE])
	
	performance_changed.emit(current_performance_level)

## Stosuje ustawienia renderowania
func _apply_render_settings(settings: Dictionary) -> void:
	var render_scale = settings[QualitySettings.RENDER_SCALE]
	
	# Ustaw skalę renderowania
	get_viewport().scaling_3d_scale = render_scale
	
	# Dostosuj tryb skalowania
	if render_scale < 1.0:
		get_viewport().scaling_3d_mode = Viewport.SCALING_3D_MODE_FSR
	else:
		get_viewport().scaling_3d_mode = Viewport.SCALING_3D_MODE_BILINEAR
	
	print("MobileOptimization: Skala renderowania: ", render_scale)

## Stosuje ustawienia cieni
func _apply_shadow_settings(shadows_enabled: bool) -> void:
	# Znajdź wszystkie światła w scenie
	_cache_lights()
	
	for light in cached_lights:
		if light and is_instance_valid(light):
			light.shadow_enabled = shadows_enabled
	
	print("MobileOptimization: Cienie: ", "włączone" if shadows_enabled else "wyłączone")

## Stosuje ustawienia cząsteczek
func _apply_particle_settings(particles_scale: float) -> void:
	# Znajdź wszystkie systemy cząsteczek
	_cache_particles()
	
	for particles in cached_particles:
		if particles and is_instance_valid(particles):
			# Skaluj ilość cząsteczek
			var original_amount = particles.get_meta("original_amount", particles.amount)
			particles.set_meta("original_amount", original_amount)
			particles.amount = int(original_amount * particles_scale)
			
			# Dostosuj jakość
			if particles_scale < 0.5:
				particles.visibility_aabb = particles.visibility_aabb * 0.5
	
	print("MobileOptimization: Cząsteczki skalowane do: ", particles_scale)

## Stosuje ustawienia tekstur
func _apply_texture_settings(texture_scale: float) -> void:
	# Ustaw globalny filtr tekstur
	if texture_scale < 0.75:
		# Użyj prostszego filtrowania dla niższej jakości
		RenderingServer.canvas_item_set_default_texture_filter(get_viewport().get_canvas_item(), RenderingServer.CANVAS_ITEM_TEXTURE_FILTER_NEAREST)
	else:
		RenderingServer.canvas_item_set_default_texture_filter(get_viewport().get_canvas_item(), RenderingServer.CANVAS_ITEM_TEXTURE_FILTER_LINEAR)
	
	print("MobileOptimization: Jakość tekstur: ", texture_scale)

## Stosuje ustawienia LOD
func _apply_lod_settings(lod_distance_scale: float) -> void:
	# Aktualizuj dystanse LOD
	for lod_obj in lod_objects:
		var original_distance = lod_obj.get("original_distance", 50.0)
		lod_obj["current_distance"] = original_distance * lod_distance_scale
	
	print("MobileOptimization: Skala dystansu LOD: ", lod_distance_scale)

## Cachuje światła w scenie
func _cache_lights() -> void:
	if not cached_lights.is_empty():
		return
	
	cached_lights = _find_nodes_of_type(Light3D)
	print("MobileOptimization: Znaleziono ", cached_lights.size(), " świateł")

## Cachuje systemy cząsteczek
func _cache_particles() -> void:
	if not cached_particles.is_empty():
		return
	
	cached_particles = _find_nodes_of_type(GPUParticles3D)
	print("MobileOptimization: Znaleziono ", cached_particles.size(), " systemów cząsteczek")

## Znajduje węzły określonego typu
func _find_nodes_of_type(node_type) -> Array:
	var nodes = []
	_find_nodes_recursive(get_tree().root, node_type, nodes)
	return nodes

## Rekursywnie znajduje węzły
func _find_nodes_recursive(node: Node, node_type, result_array: Array) -> void:
	if node is node_type:
		result_array.append(node)
	
	for child in node.get_children():
		_find_nodes_recursive(child, node_type, result_array)

## Rejestruje obiekt do systemu LOD
func register_lod_object(object: Node3D, distances: Array[float], meshes: Array[Mesh]) -> void:
	var lod_data = {
		"object": object,
		"distances": distances,
		"meshes": meshes,
		"original_distance": distances[0] if not distances.is_empty() else 50.0,
		"current_distance": distances[0] if not distances.is_empty() else 50.0,
		"current_lod": 0
	}
	
	lod_objects.append(lod_data)
	print("MobileOptimization: Zarejestrowano obiekt LOD: ", object.name)

## Aktualizuje system LOD
func _update_lod_system(delta: float) -> void:
	var camera = get_viewport().get_camera_3d()
	if not camera:
		return
	
	var camera_pos = camera.global_position
	
	for lod_data in lod_objects:
		var obj = lod_data["object"]
		if not obj or not is_instance_valid(obj):
			continue
		
		var distance = camera_pos.distance_to(obj.global_position)
		var distances = lod_data["distances"]
		var meshes = lod_data["meshes"]
		
		# Znajdź odpowiedni poziom LOD
		var new_lod = distances.size() - 1
		for i in range(distances.size()):
			if distance <= distances[i] * lod_data["current_distance"]:
				new_lod = i
				break
		
		# Zastosuj LOD jeśli się zmienił
		if new_lod != lod_data["current_lod"]:
			lod_data["current_lod"] = new_lod
			_apply_lod_to_object(obj, meshes[new_lod] if new_lod < meshes.size() else null)

## Stosuje LOD do obiektu
func _apply_lod_to_object(object: Node3D, mesh: Mesh) -> void:
	var mesh_instance = object.get_node_or_null("MeshInstance3D")
	if mesh_instance and mesh_instance is MeshInstance3D:
		if mesh:
			mesh_instance.mesh = mesh
			mesh_instance.visible = true
		else:
			mesh_instance.visible = false  # Ukryj obiekt jeśli za daleko

## Ustawia poziom wydajności
func set_performance_level(level: PerformanceLevel) -> void:
	if level == current_performance_level:
		return
	
	current_performance_level = level
	_apply_performance_settings()
	
	print("MobileOptimization: Ustawiono poziom wydajności: ", PerformanceLevel.keys()[level])

## Ustawia docelowe FPS
func set_target_fps(fps: int) -> void:
	target_fps = clamp(fps, 30, 120)
	fps_target_changed.emit(target_fps)
	
	print("MobileOptimization: Docelowe FPS: ", target_fps)

## Włącza/wyłącza automatyczne dostosowywanie jakości
func set_auto_adjust_quality(enabled: bool) -> void:
	auto_adjust_quality = enabled
	
	if enabled:
		_start_performance_monitoring()
		print("MobileOptimization: Automatyczne dostosowywanie włączone")
	else:
		print("MobileOptimization: Automatyczne dostosowywanie wyłączone")

## Wymusza odświeżenie cache'u
func refresh_cache() -> void:
	cached_lights.clear()
	cached_particles.clear()
	cached_mesh_instances.clear()
	
	print("MobileOptimization: Cache odświeżony")

## Zwraca aktualny poziom wydajności
func get_current_performance_level() -> PerformanceLevel:
	return current_performance_level

## Zwraca średnie FPS z historii
func get_average_fps() -> float:
	if fps_history.is_empty():
		return 0.0
	
	return fps_history.reduce(func(a, b): return a + b, 0.0) / fps_history.size()

## Zwraca statystyki wydajności
func get_performance_stats() -> Dictionary:
	return {
		"current_fps": Engine.get_frames_per_second(),
		"average_fps": get_average_fps(),
		"target_fps": target_fps,
		"performance_level": PerformanceLevel.keys()[current_performance_level],
		"auto_adjust": auto_adjust_quality,
		"lod_objects": lod_objects.size(),
		"cached_lights": cached_lights.size(),
		"cached_particles": cached_particles.size()
	}

## Resetuje ustawienia do domyślnych
func reset_to_defaults() -> void:
	current_performance_level = PerformanceLevel.MEDIUM
	target_fps = 60
	auto_adjust_quality = true
	
	_apply_performance_settings()
	print("MobileOptimization: Przywrócono ustawienia domyślne")