extends Node3D
class_name MultiplayerScene

## Scena testowa multiplayer - zarządza połączeniami i synchronizacją pojazdów
## Obsługuje spawning graczy, synchronizację pozycji i podstawową rozgrywkę

@onready var spawn_points: Node3D = $SpawnPoints
@onready var networked_vehicles: Node3D = $NetworkedVehicles

@onready var host_button: Button = $UI/MultiplayerUI/UIContainer/HostButton
@onready var join_button: Button = $UI/MultiplayerUI/UIContainer/JoinContainer/JoinButton
@onready var ip_input: LineEdit = $UI/MultiplayerUI/UIContainer/JoinContainer/IPInput
@onready var disconnect_button: Button = $UI/MultiplayerUI/UIContainer/DisconnectButton
@onready var status_label: Label = $UI/MultiplayerUI/UIContainer/StatusLabel
@onready var players_label: Label = $UI/MultiplayerUI/UIContainer/PlayersLabel

# Prefab pojazdu sieciowego
var networked_vehicle_scene := preload("res://scenes/vehicles/networked_car.tscn")

# Spawned vehicles
var player_vehicles := {}  ## Dictionary[int, Node3D] - player_id -> vehicle

func _ready() -> void:
	print("MultiplayerScene: Inicjalizacja sceny multiplayer")
	
	# Połącz sygnały MultiplayerManager
	if MultiplayerManager:
		MultiplayerManager.player_connected.connect(_on_player_connected)
		MultiplayerManager.player_disconnected.connect(_on_player_disconnected)
		MultiplayerManager.connection_established.connect(_on_connection_established)
		MultiplayerManager.connection_failed.connect(_on_connection_failed)
		MultiplayerManager.server_started.connect(_on_server_started)
		MultiplayerManager.server_stopped.connect(_on_server_stopped)
	
	_update_ui()

## Aktualizuje interfejs użytkownika
func _update_ui() -> void:
	if not MultiplayerManager:
		return
	
	var is_connected = MultiplayerManager.is_connected()
	var is_host = MultiplayerManager.is_host()
	var player_count = MultiplayerManager.get_player_count()
	var max_players = MultiplayerManager.get_max_players()
	
	# Aktualizuj przyciski
	host_button.disabled = is_connected
	join_button.disabled = is_connected
	ip_input.editable = not is_connected
	disconnect_button.disabled = not is_connected
	
	# Aktualizuj status
	match MultiplayerManager.get_current_mode():
		MultiplayerManager.NetworkMode.OFFLINE:
			status_label.text = "Status: Offline"
		MultiplayerManager.NetworkMode.P2P_HOST:
			status_label.text = "Status: Host P2P"
		MultiplayerManager.NetworkMode.P2P_CLIENT:
			status_label.text = "Status: Klient P2P"
		MultiplayerManager.NetworkMode.DEDICATED_SERVER:
			status_label.text = "Status: Serwer dedykowany"
		MultiplayerManager.NetworkMode.CLIENT:
			status_label.text = "Status: Klient serwera"
	
	players_label.text = "Gracze: " + str(player_count) + "/" + str(max_players)

## Callback: Przycisk Host
func _on_host_button_pressed() -> void:
	print("MultiplayerScene: Hostowanie gry...")
	
	if MultiplayerManager:
		var success = MultiplayerManager.start_p2p_host()
		if success:
			_spawn_local_player()

## Callback: Przycisk Join
func _on_join_button_pressed() -> void:
	var ip = ip_input.text.strip_edges()
	if ip.is_empty():
		ip = "127.0.0.1"
	
	print("MultiplayerScene: Dołączanie do gry: ", ip)
	
	if MultiplayerManager:
		MultiplayerManager.join_p2p_game(ip)

## Callback: Przycisk Disconnect
func _on_disconnect_button_pressed() -> void:
	print("MultiplayerScene: Rozłączanie...")
	
	# Usuń wszystkie pojazdy
	_cleanup_vehicles()
	
	if MultiplayerManager:
		MultiplayerManager.disconnect()

## Callback: Przycisk Back
func _on_back_button_pressed() -> void:
	print("MultiplayerScene: Powrót do menu głównego")
	
	# Rozłącz jeśli połączony
	if MultiplayerManager and MultiplayerManager.is_connected():
		_on_disconnect_button_pressed()
	
	# Wróć do menu
	if GameManager:
		GameManager.return_to_main_menu()

## Callback: Gracz się połączył
func _on_player_connected(player_id: int, player_info: Dictionary) -> void:
	print("MultiplayerScene: Gracz ", player_id, " się połączył: ", player_info)
	
	# Spawn pojazdu dla nowego gracza
	_spawn_player_vehicle.rpc(player_id, player_info)
	
	_update_ui()

## Callback: Gracz się rozłączył
func _on_player_disconnected(player_id: int) -> void:
	print("MultiplayerScene: Gracz ", player_id, " się rozłączył")
	
	# Usuń pojazd gracza
	_remove_player_vehicle(player_id)
	
	_update_ui()

## Callback: Połączenie nawiązane
func _on_connection_established() -> void:
	print("MultiplayerScene: Połączenie nawiązane")
	
	# Spawn lokalnego gracza
	_spawn_local_player()
	
	_update_ui()

## Callback: Połączenie nieudane
func _on_connection_failed(error: String) -> void:
	print("MultiplayerScene: Błąd połączenia: ", error)
	status_label.text = "Status: Błąd - " + error
	_update_ui()

## Callback: Serwer uruchomiony
func _on_server_started(port: int) -> void:
	print("MultiplayerScene: Serwer uruchomiony na porcie ", port)
	_update_ui()

## Callback: Serwer zatrzymany
func _on_server_stopped() -> void:
	print("MultiplayerScene: Serwer zatrzymany")
	_cleanup_vehicles()
	_update_ui()

## Tworzy pojazd dla lokalnego gracza
func _spawn_local_player() -> void:
	if not multiplayer.has_multiplayer_peer():
		return
	
	var player_id = multiplayer.get_unique_id()
	var player_info = MultiplayerManager.local_player_info if MultiplayerManager else {}
	
	print("MultiplayerScene: Spawn lokalnego gracza: ", player_id)
	_spawn_player_vehicle.rpc(player_id, player_info)

## RPC: Tworzy pojazd gracza
@rpc("any_peer", "call_local", "reliable")
func _spawn_player_vehicle(player_id: int, player_info: Dictionary) -> void:
	if player_id in player_vehicles:
		return  # Pojazd już istnieje
	
	# Znajdź wolny punkt spawnu
	var spawn_point = _get_free_spawn_point()
	if not spawn_point:
		print("MultiplayerScene: Brak wolnych punktów spawnu!")
		return
	
	# Utwórz pojazd
	var vehicle = networked_vehicle_scene.instantiate()
	vehicle.name = "Player_" + str(player_id)
	
	# Ustaw pozycję
	vehicle.global_position = spawn_point.global_position
	vehicle.global_rotation = spawn_point.global_rotation
	
	# Konfiguruj sieć
	if vehicle.has_method("set_multiplayer_authority"):
		vehicle.set_multiplayer_authority(player_id)
	
	# Dodaj do sceny
	networked_vehicles.add_child(vehicle)
	player_vehicles[player_id] = vehicle
	
	# Konfiguruj kamerę dla lokalnego gracza
	if player_id == multiplayer.get_unique_id():
		_setup_local_player_camera(vehicle)
	
	print("MultiplayerScene: Utworzono pojazd dla gracza ", player_id, " w pozycji ", spawn_point.global_position)

## Usuwa pojazd gracza
func _remove_player_vehicle(player_id: int) -> void:
	if not player_id in player_vehicles:
		return
	
	var vehicle = player_vehicles[player_id]
	vehicle.queue_free()
	player_vehicles.erase(player_id)
	
	print("MultiplayerScene: Usunięto pojazd gracza ", player_id)

## Znajduje wolny punkt spawnu
func _get_free_spawn_point() -> Marker3D:
	var spawn_children = spawn_points.get_children()
	
	# Sprawdź wszystkie punkty spawnu
	for spawn_point in spawn_children:
		if spawn_point is Marker3D:
			# Sprawdź czy punkt jest wolny
			if _is_spawn_point_free(spawn_point):
				return spawn_point
	
	# Jeśli wszystkie zajęte, zwróć pierwszy
	if not spawn_children.is_empty():
		return spawn_children[0]
	
	return null

## Sprawdza czy punkt spawnu jest wolny
func _is_spawn_point_free(spawn_point: Marker3D) -> bool:
	var spawn_position = spawn_point.global_position
	
	# Sprawdź czy jakiś pojazd jest w pobliżu
	for vehicle in player_vehicles.values():
		if vehicle and vehicle.global_position.distance_to(spawn_position) < 5.0:
			return false
	
	return true

## Konfiguruje kamerę lokalnego gracza
func _setup_local_player_camera(vehicle: Node3D) -> void:
	# Znajdź kamerę w pojeździe
	var camera = vehicle.get_node_or_null("Camera3D")
	if camera and camera is Camera3D:
		camera.current = true
		print("MultiplayerScene: Ustawiono kamerę dla lokalnego gracza")

## Czyści wszystkie pojazdy
func _cleanup_vehicles() -> void:
	for player_id in player_vehicles:
		var vehicle = player_vehicles[player_id]
		if vehicle:
			vehicle.queue_free()
	
	player_vehicles.clear()
	print("MultiplayerScene: Wyczyszczono wszystkie pojazdy")

## Zwraca pojazd lokalnego gracza
func get_local_player_vehicle() -> Node3D:
	if not multiplayer.has_multiplayer_peer():
		return null
	
	var player_id = multiplayer.get_unique_id()
	return player_vehicles.get(player_id)

## Zwraca wszystkie pojazdy graczy
func get_all_player_vehicles() -> Array[Node3D]:
	var vehicles: Array[Node3D] = []
	for vehicle in player_vehicles.values():
		if vehicle:
			vehicles.append(vehicle)
	return vehicles

## Zwraca liczbę graczy w grze
func get_player_count() -> int:
	return player_vehicles.size()

## Sprawdza czy lokalny gracz ma pojazd
func has_local_player() -> bool:
	return get_local_player_vehicle() != null