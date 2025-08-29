extends Control
class_name HUD

## HUD gry - wyświetla informacje o pojeździe, misji i stanie gry
## Zawiera prędkościomierz, minimap, nitro, drift score itp.

signal pause_requested()
signal menu_requested()

@onready var speed_label: Label = $SpeedContainer/SpeedLabel
@onready var speed_units_label: Label = $SpeedContainer/SpeedUnitsLabel
@onready var rpm_bar: ProgressBar = $RPMContainer/RPMBar
@onready var gear_label: Label = $RPMContainer/GearLabel

@onready var nitro_bar: ProgressBar = $NitroContainer/NitroBar
@onready var nitro_label: Label = $NitroContainer/NitroLabel

@onready var drift_score_label: Label = $DriftContainer/DriftScoreLabel
@onready var drift_multiplier_label: Label = $DriftContainer/DriftMultiplierLabel

@onready var minimap_viewport: SubViewport = $MinimapContainer/MinimapViewport
@onready var minimap_camera: Camera3D = $MinimapContainer/MinimapViewport/MinimapCamera

@onready var mission_panel: Panel = $MissionPanel
@onready var mission_title_label: Label = $MissionPanel/MissionContainer/MissionTitleLabel
@onready var mission_progress_bar: ProgressBar = $MissionPanel/MissionContainer/MissionProgressBar
@onready var objective_labels: VBoxContainer = $MissionPanel/MissionContainer/ObjectiveLabels

@onready var time_label: Label = $TimeContainer/TimeLabel
@onready var weather_label: Label = $TimeContainer/WeatherLabel

@onready var pause_menu: Control = $PauseMenu

# Stan HUD
var current_vehicle: Node3D = null
var is_paused := false
var drift_score := 0
var drift_multiplier := 1.0

func _ready() -> void:
	print("HUD: Inicjalizacja")
	
	# Ukryj menu pauzy
	pause_menu.hide()
	
	# Połącz sygnały systemów
	_connect_system_signals()
	
	# Konfiguruj minimap
	_setup_minimap()

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("pause"):
		toggle_pause()

## Łączy sygnały systemów
func _connect_system_signals() -> void:
	# GameManager
	if GameManager:
		GameManager.game_state_changed.connect(_on_game_state_changed)
	
	# TimeOfDayManager
	if TimeOfDayManager:
		TimeOfDayManager.time_changed.connect(_on_time_changed)
	
	# WeatherManager  
	if WeatherManager:
		WeatherManager.weather_changed.connect(_on_weather_changed)
	
	# MissionSystem
	if MissionSystem:
		MissionSystem.mission_started.connect(_on_mission_started)
		MissionSystem.mission_progress_updated.connect(_on_mission_progress_updated)
		MissionSystem.objective_completed.connect(_on_objective_completed)

## Konfiguruje minimap
func _setup_minimap() -> void:
	if minimap_camera:
		minimap_camera.projection = PROJECTION_ORTHOGONAL
		minimap_camera.size = 100.0
		minimap_camera.position = Vector3(0, 50, 0)
		minimap_camera.look_at(Vector3.ZERO, Vector3.FORWARD)

## Rejestruje pojazd do śledzenia
func register_vehicle(vehicle: Node3D) -> void:
	current_vehicle = vehicle
	print("HUD: Zarejestrowano pojazd: ", vehicle.name)
	
	# Połącz sygnały pojazdu jeśli to samochód
	if vehicle is CarController:
		var car = vehicle as CarController
		car.speed_changed.connect(_on_vehicle_speed_changed)
		car.gear_changed.connect(_on_vehicle_gear_changed)
		car.drift_started.connect(_on_drift_started)
		car.drift_ended.connect(_on_drift_ended)
	
	# Ustaw kamerę minimapy nad pojazdem
	if minimap_camera and vehicle:
		minimap_camera.position = vehicle.global_position + Vector3(0, 50, 0)

func _process(delta: float) -> void:
	if not current_vehicle:
		return
	
	# Aktualizuj pozycję kamery minimapy
	if minimap_camera:
		var target_pos = current_vehicle.global_position + Vector3(0, 50, 0)
		minimap_camera.global_position = target_pos

## Aktualizuje prędkościomierz
func _on_vehicle_speed_changed(speed_kmh: float) -> void:
	if speed_label:
		speed_label.text = str(int(speed_kmh))
	
	# Aktualizuj RPM jeśli dostępne
	if current_vehicle and current_vehicle.has_method("get_rpm") and rpm_bar:
		var rpm = current_vehicle.get_rpm()
		var max_rpm = 6000.0  # Domyślne max RPM
		if current_vehicle.has_method("get") and current_vehicle.get("max_rpm"):
			max_rpm = current_vehicle.max_rpm
		
		rpm_bar.value = (rpm / max_rpm) * 100.0

## Aktualizuje wskaźnik biegu
func _on_vehicle_gear_changed(gear: int) -> void:
	if gear_label:
		if gear == 0:
			gear_label.text = "N"
		else:
			gear_label.text = str(gear)

## Aktualizuje nitro bar
func update_nitro(percentage: float) -> void:
	if nitro_bar:
		nitro_bar.value = percentage
	
	if nitro_label:
		nitro_label.text = "NITRO " + str(int(percentage)) + "%"

## Callback rozpoczęcia driftu
func _on_drift_started() -> void:
	drift_score = 0
	drift_multiplier = 1.0
	_update_drift_display()

## Callback zakończenia driftu
func _on_drift_ended(final_score: int) -> void:
	drift_score = final_score
	drift_multiplier = 1.0
	_update_drift_display()
	
	# Animacja znikania wyniku
	var tween = create_tween()
	tween.tween_delay(2.0)
	tween.tween_callback(func(): drift_score = 0; _update_drift_display())

## Aktualizuje wyświetlanie driftu
func _update_drift_display() -> void:
	if drift_score_label:
		if drift_score > 0:
			drift_score_label.text = "DRIFT: " + str(drift_score)
			drift_score_label.show()
		else:
			drift_score_label.hide()
	
	if drift_multiplier_label:
		if drift_multiplier > 1.0:
			drift_multiplier_label.text = "x" + str(drift_multiplier, 1)
			drift_multiplier_label.show()
		else:
			drift_multiplier_label.hide()

## Callback zmiany czasu
func _on_time_changed(current_time: float) -> void:
	if time_label and TimeOfDayManager:
		time_label.text = TimeOfDayManager.get_time_of_day_string()

## Callback zmiany pogody
func _on_weather_changed(old_weather, new_weather) -> void:
	if weather_label and WeatherManager:
		weather_label.text = WeatherManager.get_weather_name()

## Callback rozpoczęcia misji
func _on_mission_started(mission) -> void:
	if mission_panel:
		mission_panel.show()
	
	if mission_title_label:
		mission_title_label.text = mission.title
	
	if mission_progress_bar:
		mission_progress_bar.value = 0
	
	# Wyczyść poprzednie cele
	if objective_labels:
		for child in objective_labels.get_children():
			child.queue_free()
		
		# Dodaj nowe cele
		for i in range(mission.objectives.size()):
			var label = Label.new()
			label.text = "☐ " + mission.objectives[i]
			label.name = "objective_" + str(i)
			objective_labels.add_child(label)

## Callback postępu misji
func _on_mission_progress_updated(mission, progress: float) -> void:
	if mission_progress_bar:
		mission_progress_bar.value = progress * 100.0

## Callback ukończenia celu
func _on_objective_completed(mission, objective_index: int) -> void:
	if objective_labels:
		var label = objective_labels.get_node_or_null("objective_" + str(objective_index))
		if label:
			label.text = "☑ " + mission.objectives[objective_index]
			label.modulate = Color.GREEN

## Callback zmiany stanu gry
func _on_game_state_changed(new_state: GameManager.GameState) -> void:
	match new_state:
		GameManager.GameState.PLAYING, GameManager.GameState.MULTIPLAYER:
			show()
		GameManager.GameState.PAUSED:
			# HUD pozostaje widoczny podczas pauzy
			pass
		GameManager.GameState.MAIN_MENU, GameManager.GameState.LOADING:
			hide()

## Przełącza pauzę
func toggle_pause() -> void:
	if GameManager:
		if GameManager.get_current_state() == GameManager.GameState.PAUSED:
			resume_game()
		else:
			pause_game()

## Pauzuje grę
func pause_game() -> void:
	if GameManager:
		GameManager.pause_game()
	
	is_paused = true
	pause_menu.show()

## Wznawia grę
func resume_game() -> void:
	if GameManager:
		GameManager.resume_game()
	
	is_paused = false
	pause_menu.hide()

## Powraca do menu głównego
func return_to_main_menu() -> void:
	if GameManager:
		GameManager.return_to_main_menu()

## Pokazuje komunikat
func show_message(text: String, duration: float = 3.0) -> void:
	var message_label = Label.new()
	message_label.text = text
	message_label.anchors_preset = Control.PRESET_CENTER
	message_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	message_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	
	add_child(message_label)
	
	# Animacja pojawiania się
	message_label.modulate.a = 0.0
	var tween = create_tween()
	tween.tween_property(message_label, "modulate:a", 1.0, 0.3)
	tween.tween_delay(duration)
	tween.tween_property(message_label, "modulate:a", 0.0, 0.3)
	tween.tween_callback(message_label.queue_free)

## Aktualizuje wynik driftu w czasie rzeczywistym
func update_drift_score(score: int, multiplier: float = 1.0) -> void:
	drift_score = score
	drift_multiplier = multiplier
	_update_drift_display()

## Pokazuje powiadomienie o osiągnięciu
func show_achievement(title: String, description: String) -> void:
	print("HUD: Osiągnięcie - ", title, ": ", description)
	show_message("🏆 " + title + "\n" + description, 4.0)