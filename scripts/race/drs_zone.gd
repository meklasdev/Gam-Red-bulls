extends Area3D
class_name DRSZone

## Strefa DRS (Drag Reduction System) - redukuje opór aerodynamiczny w wyznaczonych strefach
## Zwiększa maksymalną prędkość pojazdu w strefie

signal drs_activated(vehicle: Node3D, zone: DRSZone)
signal drs_deactivated(vehicle: Node3D, zone: DRSZone)
signal drs_zone_entered(vehicle: Node3D, zone: DRSZone)
signal drs_zone_exited(vehicle: Node3D, zone: DRSZone)

@export_group("DRS Settings")
@export var zone_id := 0                          ## ID strefy DRS
@export var zone_name := "DRS Zone"               ## Nazwa strefy
@export var speed_boost_percentage := 15.0        ## Zwiększenie prędkości (%)
@export var activation_delay := 1.0               ## Opóźnienie aktywacji (sekundy)
@export var cooldown_time := 5.0                  ## Czas odnowienia po opuszczeniu

@export_group("Activation Requirements")
@export var min_speed_kmh := 80.0                 ## Minimalna prędkość do aktywacji
@export var requires_following := true            ## Czy wymaga bycia za innym pojazdem
@export var following_distance := 30.0            ## Maksymalny dystans do pojazdu z przodu
@export var requires_detection_zone := false      ## Czy wymaga przejścia przez strefę wykrywania

@export_group("Visual Settings")
@export var show_zone_boundaries := true          ## Pokazuj granice strefy
@export var active_color := Color.GREEN           ## Kolor aktywnej strefy
@export var inactive_color := Color.YELLOW        ## Kolor nieaktywnej strefy
@export var disabled_color := Color.RED           ## Kolor wyłączonej strefy

@export_group("Audio")
@export var activation_sound: AudioStream         ## Dźwięk aktywacji DRS
@export var deactivation_sound: AudioStream       ## Dźwięk dezaktywacji DRS

# Stan strefy
var vehicles_in_zone: Array[Node3D] = []
var active_vehicles: Array[Node3D] = []          # Pojazdy z aktywnym DRS
var vehicle_timers := {}                         # Timery aktywacji dla pojazdów
var vehicle_cooldowns := {}                      # Cooldowny dla pojazdów

# Komponenty
@onready var mesh_instance: MeshInstance3D = $MeshInstance3D
@onready var collision_shape: CollisionShape3D = $CollisionShape3D
@onready var audio_player: AudioStreamPlayer3D = $AudioStreamPlayer3D
@onready var label_3d: Label3D = $Label3D
@onready var particles: GPUParticles3D = $DRSParticles

# Strefa wykrywania (opcjonalna)
var detection_zone: Area3D = null

func _ready() -> void:
	print("DRSZone: Inicjalizacja strefy DRS ", zone_id, " - ", zone_name)
	
	# Połącz sygnały
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	
	# Konfiguruj komponenty
	_setup_visuals()
	_setup_audio()
	_setup_detection_zone()
	
	# Dodaj do grupy
	add_to_group("drs_zones")

## Konfiguruje wygląd strefy
func _setup_visuals() -> void:
	if mesh_instance:
		_update_zone_color(inactive_color)
	
	if label_3d:
		label_3d.text = zone_name
		label_3d.visible = show_zone_boundaries
	
	# Konfiguruj cząsteczki
	if particles:
		particles.emitting = false

## Konfiguruje dźwięk
func _setup_audio() -> void:
	if not audio_player:
		audio_player = AudioStreamPlayer3D.new()
		add_child(audio_player)

## Konfiguruje strefę wykrywania
func _setup_detection_zone() -> void:
	if requires_detection_zone:
		# Utwórz strefę wykrywania przed główną strefą DRS
		detection_zone = Area3D.new()
		var detection_shape = CollisionShape3D.new()
		var box_shape = BoxShape3D.new()
		
		box_shape.size = Vector3(collision_shape.shape.size.x, 
								collision_shape.shape.size.y, 
								collision_shape.shape.size.z * 2)
		
		detection_shape.shape = box_shape
		detection_zone.add_child(detection_shape)
		
		# Umieść przed strefą DRS
		detection_zone.position = Vector3(0, 0, -collision_shape.shape.size.z)
		add_child(detection_zone)
		
		print("DRSZone: Utworzono strefę wykrywania")

func _process(delta: float) -> void:
	_update_vehicle_timers(delta)
	_update_vehicle_cooldowns(delta)
	_check_drs_conditions()

## Aktualizuje timery aktywacji
func _update_vehicle_timers(delta: float) -> void:
	for vehicle in vehicle_timers:
		vehicle_timers[vehicle] -= delta
		
		if vehicle_timers[vehicle] <= 0.0:
			_activate_drs(vehicle)
			vehicle_timers.erase(vehicle)

## Aktualizuje cooldowny
func _update_vehicle_cooldowns(delta: float) -> void:
	var to_remove = []
	
	for vehicle in vehicle_cooldowns:
		vehicle_cooldowns[vehicle] -= delta
		if vehicle_cooldowns[vehicle] <= 0.0:
			to_remove.append(vehicle)
	
	for vehicle in to_remove:
		vehicle_cooldowns.erase(vehicle)

## Sprawdza warunki DRS dla wszystkich pojazdów w strefie
func _check_drs_conditions() -> void:
	for vehicle in vehicles_in_zone:
		if vehicle in active_vehicles:
			continue  # DRS już aktywny
		
		if vehicle in vehicle_timers:
			continue  # Timer już działa
		
		if vehicle in vehicle_cooldowns:
			continue  # Cooldown aktywny
		
		if _can_activate_drs(vehicle):
			_start_activation_timer(vehicle)

## Sprawdza czy pojazd może aktywować DRS
func _can_activate_drs(vehicle: Node3D) -> bool:
	# Sprawdź prędkość
	if not _check_speed_requirement(vehicle):
		return false
	
	# Sprawdź czy jest za innym pojazdem (jeśli wymagane)
	if requires_following and not _check_following_requirement(vehicle):
		return false
	
	# Sprawdź strefę wykrywania (jeśli wymagana)
	if requires_detection_zone and not _check_detection_zone_requirement(vehicle):
		return false
	
	return true

## Sprawdza wymaganie prędkości
func _check_speed_requirement(vehicle: Node3D) -> bool:
	if not vehicle.has_method("get_speed_kmh"):
		return false
	
	return vehicle.get_speed_kmh() >= min_speed_kmh

## Sprawdza wymaganie podążania za pojazdem
func _check_following_requirement(vehicle: Node3D) -> bool:
	var vehicles_ahead = _find_vehicles_ahead(vehicle)
	
	for ahead_vehicle in vehicles_ahead:
		var distance = vehicle.global_position.distance_to(ahead_vehicle.global_position)
		if distance <= following_distance:
			return true
	
	return false

## Znajduje pojazdy z przodu
func _find_vehicles_ahead(vehicle: Node3D) -> Array[Node3D]:
	var ahead_vehicles: Array[Node3D] = []
	var all_vehicles = get_tree().get_nodes_in_group("vehicles")
	
	for other_vehicle in all_vehicles:
		if other_vehicle == vehicle:
			continue
		
		# Sprawdź czy jest z przodu
		var direction_to_other = (other_vehicle.global_position - vehicle.global_position).normalized()
		var vehicle_forward = -vehicle.global_transform.basis.z
		
		if direction_to_other.dot(vehicle_forward) > 0.5:  # Kąt mniejszy niż 60 stopni
			ahead_vehicles.append(other_vehicle)
	
	return ahead_vehicles

## Sprawdza wymaganie strefy wykrywania
func _check_detection_zone_requirement(vehicle: Node3D) -> bool:
	if not detection_zone:
		return true
	
	# Sprawdź czy pojazd przeszedł przez strefę wykrywania
	# To wymagałoby dodatkowego trackingu, uproszczenie:
	return true

## Rozpoczyna timer aktywacji
func _start_activation_timer(vehicle: Node3D) -> void:
	vehicle_timers[vehicle] = activation_delay
	print("DRSZone: Rozpoczęto timer aktywacji DRS dla ", vehicle.name)

## Aktywuje DRS dla pojazdu
func _activate_drs(vehicle: Node3D) -> void:
	if vehicle in active_vehicles:
		return
	
	active_vehicles.append(vehicle)
	
	# Zastosuj boost prędkości
	_apply_speed_boost(vehicle, true)
	
	# Efekty wizualne i dźwiękowe
	_play_activation_sound()
	_update_zone_color(active_color)
	
	if particles:
		particles.emitting = true
	
	print("DRSZone: Aktywowano DRS dla ", vehicle.name)
	drs_activated.emit(vehicle, self)

## Deaktywuje DRS dla pojazdu
func _deactivate_drs(vehicle: Node3D) -> void:
	if not vehicle in active_vehicles:
		return
	
	active_vehicles.erase(vehicle)
	
	# Usuń boost prędkości
	_apply_speed_boost(vehicle, false)
	
	# Ustaw cooldown
	vehicle_cooldowns[vehicle] = cooldown_time
	
	# Efekty wizualne i dźwiękowe
	_play_deactivation_sound()
	
	if active_vehicles.is_empty():
		_update_zone_color(inactive_color)
		if particles:
			particles.emitting = false
	
	print("DRSZone: Deaktywowano DRS dla ", vehicle.name)
	drs_deactivated.emit(vehicle, self)

## Stosuje boost prędkości
func _apply_speed_boost(vehicle: Node3D, enable: bool) -> void:
	# Dla CarController
	if vehicle is CarController:
		var car = vehicle as CarController
		if enable:
			car.engine_force_base *= (1.0 + speed_boost_percentage / 100.0)
		else:
			car.engine_force_base /= (1.0 + speed_boost_percentage / 100.0)
	
	# Dla innych typów pojazdów można dodać podobną logikę
	elif vehicle.has_method("set_drs_active"):
		vehicle.set_drs_active(enable, speed_boost_percentage)

## Callback wejścia do strefy
func _on_body_entered(body: Node3D) -> void:
	if not _is_vehicle(body):
		return
	
	if body in vehicles_in_zone:
		return
	
	vehicles_in_zone.append(body)
	print("DRSZone: Pojazd ", body.name, " wszedł do strefy DRS")
	drs_zone_entered.emit(body, self)

## Callback wyjścia ze strefy
func _on_body_exited(body: Node3D) -> void:
	if not _is_vehicle(body):
		return
	
	if body in vehicles_in_zone:
		vehicles_in_zone.erase(body)
		
		# Deaktywuj DRS jeśli był aktywny
		if body in active_vehicles:
			_deactivate_drs(body)
		
		# Usuń timer jeśli był aktywny
		if body in vehicle_timers:
			vehicle_timers.erase(body)
		
		print("DRSZone: Pojazd ", body.name, " opuścił strefę DRS")
		drs_zone_exited.emit(body, self)

## Sprawdza czy obiekt to pojazd
func _is_vehicle(body: Node3D) -> bool:
	return body.is_in_group("vehicles") or \
		   body is CarController or \
		   body is BikeController or \
		   body is MotoController

## Odtwarza dźwięk aktywacji
func _play_activation_sound() -> void:
	if audio_player and activation_sound:
		audio_player.stream = activation_sound
		audio_player.play()

## Odtwarza dźwięk deaktywacji
func _play_deactivation_sound() -> void:
	if audio_player and deactivation_sound:
		audio_player.stream = deactivation_sound
		audio_player.play()

## Aktualizuje kolor strefy
func _update_zone_color(color: Color) -> void:
	if mesh_instance and mesh_instance.get_surface_override_material(0):
		var material = mesh_instance.get_surface_override_material(0)
		material.albedo_color = color

## Zwraca listę pojazdów z aktywnym DRS
func get_active_vehicles() -> Array[Node3D]:
	return active_vehicles.duplicate()

## Zwraca listę pojazdów w strefie
func get_vehicles_in_zone() -> Array[Node3D]:
	return vehicles_in_zone.duplicate()

## Sprawdza czy pojazd ma aktywny DRS
func is_drs_active(vehicle: Node3D) -> bool:
	return vehicle in active_vehicles

## Sprawdza czy pojazd jest w strefie
func is_vehicle_in_zone(vehicle: Node3D) -> bool:
	return vehicle in vehicles_in_zone

## Wymusza aktywację DRS (dla debugowania)
func force_activate_drs(vehicle: Node3D) -> void:
	if vehicle in vehicles_in_zone:
		_activate_drs(vehicle)

## Wymusza deaktywację DRS
func force_deactivate_drs(vehicle: Node3D) -> void:
	if vehicle in active_vehicles:
		_deactivate_drs(vehicle)

## Resetuje strefę DRS
func reset_zone() -> void:
	# Deaktywuj DRS dla wszystkich pojazdów
	for vehicle in active_vehicles.duplicate():
		_deactivate_drs(vehicle)
	
	# Wyczyść wszystkie timery i cooldowny
	vehicle_timers.clear()
	vehicle_cooldowns.clear()
	
	_update_zone_color(inactive_color)
	
	if particles:
		particles.emitting = false
	
	print("DRSZone: Zresetowano strefę DRS")

## Ustawia aktywność strefy
func set_zone_active(active: bool) -> void:
	monitoring = active
	monitorable = active
	
	if not active:
		reset_zone()
		_update_zone_color(disabled_color)
	else:
		_update_zone_color(inactive_color)

## Zwraca informacje o strefie
func get_zone_info() -> Dictionary:
	return {
		"id": zone_id,
		"name": zone_name,
		"position": global_position,
		"speed_boost": speed_boost_percentage,
		"vehicles_in_zone": vehicles_in_zone.size(),
		"active_vehicles": active_vehicles.size(),
		"min_speed": min_speed_kmh,
		"requires_following": requires_following
	}