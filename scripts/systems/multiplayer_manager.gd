extends Node
class_name MultiplayerManager

## Menedżer multiplayer - obsługuje połączenia P2P i klient-serwer
## Singleton dostępny globalnie jako MultiplayerManager

signal player_connected(id: int, player_info: Dictionary)
signal player_disconnected(id: int)
signal connection_established()
signal connection_failed(error: String)
signal server_started(port: int)
signal server_stopped()

enum NetworkMode {
	OFFLINE,        ## Tryb offline
	P2P_HOST,       ## Host P2P
	P2P_CLIENT,     ## Klient P2P
	DEDICATED_SERVER, ## Serwer dedykowany
	CLIENT          ## Klient do serwera
}

var current_mode := NetworkMode.OFFLINE
var max_players := 8
var default_port := 7777
var server_name := "Gam Red Bulls Server"

# Informacje o graczach
var players := {}  ## Dictionary[int, Dictionary] - ID gracza -> dane gracza
var local_player_info := {
	"name": "Gracz",
	"vehicle": "car_basic",
	"ready": false
}

# Peer network
var peer: ENetMultiplayerPeer

func _ready() -> void:
	print("MultiplayerManager: Inicjalizacja...")
	
	# Połącz sygnały multiplayer
	multiplayer.peer_connected.connect(_on_peer_connected)
	multiplayer.peer_disconnected.connect(_on_peer_disconnected)
	multiplayer.connected_to_server.connect(_on_connected_to_server)
	multiplayer.connection_failed.connect(_on_connection_failed)
	multiplayer.server_disconnected.connect(_on_server_disconnected)

## Startuje serwer P2P
func start_p2p_host(port: int = default_port) -> bool:
	print("MultiplayerManager: Startowanie hosta P2P na porcie ", port)
	
	peer = ENetMultiplayerPeer.new()
	var error = peer.create_server(port, max_players)
	
	if error != OK:
		print("MultiplayerManager: Błąd tworzenia serwera: ", error)
		connection_failed.emit("Nie można uruchomić serwera na porcie " + str(port))
		return false
	
	multiplayer.multiplayer_peer = peer
	current_mode = NetworkMode.P2P_HOST
	
	# Dodaj lokalnego gracza
	players[1] = local_player_info.duplicate()
	
	server_started.emit(port)
	print("MultiplayerManager: Host P2P uruchomiony")
	return true

## Łączy się z hostem P2P
func join_p2p_game(address: String, port: int = default_port) -> bool:
	print("MultiplayerManager: Łączenie z P2P: ", address, ":", port)
	
	peer = ENetMultiplayerPeer.new()
	var error = peer.create_client(address, port)
	
	if error != OK:
		print("MultiplayerManager: Błąd tworzenia klienta: ", error)
		connection_failed.emit("Nie można połączyć się z " + address + ":" + str(port))
		return false
	
	multiplayer.multiplayer_peer = peer
	current_mode = NetworkMode.P2P_CLIENT
	
	print("MultiplayerManager: Próba połączenia z P2P...")
	return true

## Startuje serwer dedykowany
func start_dedicated_server(port: int = default_port) -> bool:
	print("MultiplayerManager: Startowanie serwera dedykowanego na porcie ", port)
	
	peer = ENetMultiplayerPeer.new()
	var error = peer.create_server(port, max_players)
	
	if error != OK:
		print("MultiplayerManager: Błąd tworzenia serwera dedykowanego: ", error)
		connection_failed.emit("Nie można uruchomić serwera dedykowanego na porcie " + str(port))
		return false
	
	multiplayer.multiplayer_peer = peer
	current_mode = NetworkMode.DEDICATED_SERVER
	
	server_started.emit(port)
	print("MultiplayerManager: Serwer dedykowany uruchomiony")
	return true

## Łączy się z serwerem dedykowanym
func join_server(address: String, port: int = default_port) -> bool:
	print("MultiplayerManager: Łączenie z serwerem: ", address, ":", port)
	
	peer = ENetMultiplayerPeer.new()
	var error = peer.create_client(address, port)
	
	if error != OK:
		print("MultiplayerManager: Błąd łączenia z serwerem: ", error)
		connection_failed.emit("Nie można połączyć się z serwerem " + address + ":" + str(port))
		return false
	
	multiplayer.multiplayer_peer = peer
	current_mode = NetworkMode.CLIENT
	
	print("MultiplayerManager: Próba połączenia z serwerem...")
	return true

## Rozłącza się i wraca do trybu offline
func disconnect() -> void:
	print("MultiplayerManager: Rozłączanie...")
	
	if peer:
		peer.close()
		peer = null
	
	multiplayer.multiplayer_peer = null
	current_mode = NetworkMode.OFFLINE
	players.clear()
	
	server_stopped.emit()
	print("MultiplayerManager: Rozłączono")

## Ustawia informacje o lokalnym graczu
func set_local_player_info(info: Dictionary) -> void:
	local_player_info = info
	
	# Jeśli jesteśmy połączeni, wyślij aktualizację
	if is_connected():
		_rpc_update_player_info.rpc(multiplayer.get_unique_id(), local_player_info)

## Zwraca informacje o graczu
func get_player_info(player_id: int) -> Dictionary:
	return players.get(player_id, {})

## Zwraca listę wszystkich graczy
func get_all_players() -> Dictionary:
	return players

## Zwraca liczbę połączonych graczy
func get_player_count() -> int:
	return players.size()

## Sprawdza czy jesteśmy połączeni
func is_connected() -> bool:
	return current_mode != NetworkMode.OFFLINE and multiplayer.multiplayer_peer != null

## Sprawdza czy jesteśmy hostem
func is_host() -> bool:
	return current_mode in [NetworkMode.P2P_HOST, NetworkMode.DEDICATED_SERVER]

## Sprawdza czy jesteśmy klientem
func is_client() -> bool:
	return current_mode in [NetworkMode.P2P_CLIENT, NetworkMode.CLIENT]

## Sprawdza czy wszyscy gracze są gotowi
func all_players_ready() -> bool:
	for player_info in players.values():
		if not player_info.get("ready", false):
			return false
	return players.size() > 0

## Ustawia status gotowości lokalnego gracza
func set_ready(ready: bool) -> void:
	local_player_info.ready = ready
	if is_connected():
		_rpc_update_player_info.rpc(multiplayer.get_unique_id(), local_player_info)

## Wyrzuca gracza (tylko host)
@rpc("call_local", "reliable")
func kick_player(player_id: int) -> void:
	if not is_host():
		return
	
	print("MultiplayerManager: Wyrzucanie gracza ", player_id)
	if peer:
		peer.disconnect_peer(player_id)

## RPC: Aktualizuje informacje o graczu
@rpc("any_peer", "reliable")
func _rpc_update_player_info(player_id: int, info: Dictionary) -> void:
	players[player_id] = info
	print("MultiplayerManager: Zaktualizowano info gracza ", player_id, ": ", info)

## RPC: Wysyła wiadomość czatu
@rpc("any_peer", "reliable")
func _rpc_chat_message(player_id: int, message: String) -> void:
	var player_name = players.get(player_id, {}).get("name", "Nieznany")
	print("Chat [", player_name, "]: ", message)
	# Tutaj można dodać sygnał dla UI czatu

## Wysyła wiadomość czatu
func send_chat_message(message: String) -> void:
	if not is_connected():
		return
	
	_rpc_chat_message.rpc(multiplayer.get_unique_id(), message)

## Callback: Gracz się połączył
func _on_peer_connected(id: int) -> void:
	print("MultiplayerManager: Gracz ", id, " się połączył")
	
	# Jeśli jesteśmy hostem, wyślij informacje o wszystkich graczach
	if is_host():
		# Wyślij info o wszystkich istniejących graczach nowemu graczowi
		for player_id in players:
			_rpc_update_player_info.rpc_id(id, player_id, players[player_id])
		
		# Wyślij info o nowym graczu wszystkim
		_rpc_update_player_info.rpc(id, {})

## Callback: Gracz się rozłączył
func _on_peer_disconnected(id: int) -> void:
	print("MultiplayerManager: Gracz ", id, " się rozłączył")
	
	var player_info = players.get(id, {})
	players.erase(id)
	
	player_disconnected.emit(id)

## Callback: Połączono z serwerem
func _on_connected_to_server() -> void:
	print("MultiplayerManager: Połączono z serwerem")
	
	# Wyślij nasze informacje do serwera
	var our_id = multiplayer.get_unique_id()
	players[our_id] = local_player_info.duplicate()
	_rpc_update_player_info.rpc(our_id, local_player_info)
	
	connection_established.emit()

## Callback: Nie udało się połączyć
func _on_connection_failed() -> void:
	print("MultiplayerManager: Nie udało się połączyć")
	disconnect()
	connection_failed.emit("Nie udało się nawiązać połączenia")

## Callback: Serwer się rozłączył
func _on_server_disconnected() -> void:
	print("MultiplayerManager: Serwer się rozłączył")
	disconnect()
	connection_failed.emit("Serwer się rozłączył")

## Zwraca aktualny tryb sieciowy
func get_current_mode() -> NetworkMode:
	return current_mode

## Ustawia maksymalną liczbę graczy
func set_max_players(count: int) -> void:
	max_players = clamp(count, 2, 16)

## Zwraca maksymalną liczbę graczy
func get_max_players() -> int:
	return max_players