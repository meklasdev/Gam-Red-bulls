extends Node
class_name SceneLoader

## System ładowania scen z obsługą asynchronicznego ładowania
## Używa Godot 4.3 API dla płynnego przechodzenia między scenami

signal scene_load_started(path: String)
signal scene_load_progress(progress: float)
signal scene_load_finished(path: String)

static var _current_loader: SceneLoader = null

## Ładuje scenę synchronicznie (natychmiast)
static func load_scene(path: String) -> void:
	print("SceneLoader: Ładowanie sceny: ", path)
	
	var packed_scene = load(path) as PackedScene
	if not packed_scene:
		push_error("SceneLoader: Nie można załadować sceny: " + path)
		return
	
	get_tree().change_scene_to_packed(packed_scene)
	print("SceneLoader: Scena załadowana: ", path)

## Ładuje scenę asynchronicznie z progress barem
static func load_scene_async(path: String) -> void:
	print("SceneLoader: Asynchroniczne ładowanie sceny: ", path)
	
	# Anuluj poprzednie ładowanie jeśli jest aktywne
	if _current_loader:
		_current_loader.queue_free()
	
	_current_loader = SceneLoader.new()
	get_tree().root.add_child(_current_loader)
	
	await _current_loader._load_scene_async_internal(path)

## Wewnętrzna implementacja asynchronicznego ładowania
func _load_scene_async_internal(path: String) -> void:
	scene_load_started.emit(path)
	
	# Rozpocznij ładowanie w tle
	var error = ResourceLoader.load_threaded_request(path)
	if error != OK:
		push_error("SceneLoader: Błąd rozpoczęcia ładowania: " + str(error))
		return
	
	# Czekaj na zakończenie ładowania z monitorowaniem postępu
	var progress = []
	var status = ResourceLoader.THREAD_LOAD_IN_PROGRESS
	
	while status == ResourceLoader.THREAD_LOAD_IN_PROGRESS:
		status = ResourceLoader.load_threaded_get_status(path, progress)
		
		if progress.size() > 0:
			scene_load_progress.emit(progress[0])
		
		await get_tree().process_frame
	
	# Sprawdź czy ładowanie się powiodło
	if status != ResourceLoader.THREAD_LOAD_LOADED:
		push_error("SceneLoader: Błąd ładowania sceny: " + path)
		return
	
	# Pobierz załadowany zasób
	var packed_scene = ResourceLoader.load_threaded_get(path) as PackedScene
	if not packed_scene:
		push_error("SceneLoader: Załadowany zasób nie jest sceną: " + path)
		return
	
	# Zmień scenę
	get_tree().change_scene_to_packed(packed_scene)
	
	scene_load_finished.emit(path)
	print("SceneLoader: Scena załadowana asynchronicznie: ", path)
	
	# Wyczyść loader
	queue_free()
	if _current_loader == self:
		_current_loader = null

## Preloaduje scenę do pamięci bez zmiany aktualnej sceny
static func preload_scene(path: String) -> PackedScene:
	print("SceneLoader: Preloadowanie sceny: ", path)
	
	var packed_scene = load(path) as PackedScene
	if not packed_scene:
		push_error("SceneLoader: Nie można preloadować sceny: " + path)
		return null
	
	print("SceneLoader: Scena preloadowana: ", path)
	return packed_scene

## Sprawdza czy scena istnieje
static func scene_exists(path: String) -> bool:
	return ResourceLoader.exists(path)

## Zwraca listę wszystkich scen w katalogu
static func get_scenes_in_directory(dir_path: String) -> Array[String]:
	var scenes: Array[String] = []
	var dir = DirAccess.open(dir_path)
	
	if not dir:
		push_error("SceneLoader: Nie można otworzyć katalogu: " + dir_path)
		return scenes
	
	dir.list_dir_begin()
	var file_name = dir.get_next()
	
	while file_name != "":
		if file_name.ends_with(".tscn"):
			scenes.append(dir_path + "/" + file_name)
		file_name = dir.get_next()
	
	dir.list_dir_end()
	return scenes

## Zwraca nazwę aktualnej sceny
static func get_current_scene_name() -> String:
	var current_scene = get_tree().current_scene
	if current_scene and current_scene.scene_file_path:
		return current_scene.scene_file_path.get_file().get_basename()
	return ""

## Zwraca ścieżkę do aktualnej sceny
static func get_current_scene_path() -> String:
	var current_scene = get_tree().current_scene
	if current_scene:
		return current_scene.scene_file_path
	return ""