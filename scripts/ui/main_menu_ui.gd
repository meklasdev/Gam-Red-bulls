extends Control
class_name MainMenuUI

## UI menu głównego - obsługuje nawigację między trybami gry
## Pozwala wybierać regiony i rozpoczynać różne tryby rozgrywki

@onready var region_select_dialog: AcceptDialog = $RegionSelectDialog
@onready var region_list: ItemList = $RegionSelectDialog/RegionContainer/RegionList

# Lista dostępnych regionów
var regions := {
	"Górski Szczyt": "res://scenes/regions/gorski_szczyt.tscn",
	"Pustynny Kanion": "res://scenes/regions/pustynny_kanion.tscn", 
	"Miasto Nocy": "res://scenes/regions/miasto_nocy.tscn",
	"Port Wyścigowy": "res://scenes/regions/port_wyscigowy.tscn",
	"Tor Mistrzów": "res://scenes/regions/tor_mistrzow.tscn"
}

var current_mode := ""

func _ready() -> void:
	print("MainMenuUI: Inicjalizacja menu głównego")
	
	# Wypełnij listę regionów
	_populate_region_list()
	
	# Połącz sygnały GameManager
	if GameManager:
		GameManager.game_state_changed.connect(_on_game_state_changed)

## Wypełnia listę regionów
func _populate_region_list() -> void:
	region_list.clear()
	
	for region_name in regions.keys():
		region_list.add_item(region_name)
	
	print("MainMenuUI: Załadowano ", regions.size(), " regionów")

## Callback: Przycisk Start
func _on_start_button_pressed() -> void:
	print("MainMenuUI: Rozpoczynanie gry")
	current_mode = "start"
	_show_region_selection()

## Callback: Przycisk Sandbox
func _on_sandbox_button_pressed() -> void:
	print("MainMenuUI: Tryb Sandbox")
	current_mode = "sandbox"
	_show_region_selection()

## Callback: Przycisk Kariera
func _on_career_button_pressed() -> void:
	print("MainMenuUI: Tryb Kariera")
	current_mode = "career"
	_show_region_selection()

## Callback: Przycisk Multiplayer
func _on_multiplayer_button_pressed() -> void:
	print("MainMenuUI: Tryb Multiplayer")
	_start_multiplayer()

## Callback: Przycisk Ustawienia
func _on_settings_button_pressed() -> void:
	print("MainMenuUI: Otwieranie ustawień")
	# Tutaj można dodać okno ustawień
	_show_settings_placeholder()

## Callback: Przycisk Wyjście
func _on_quit_button_pressed() -> void:
	print("MainMenuUI: Zamykanie gry")
	GameManager.quit_game()

## Callback: Wybór regionu
func _on_region_selected(index: int) -> void:
	var region_name = region_list.get_item_text(index)
	var region_path = regions[region_name]
	
	print("MainMenuUI: Wybrano region: ", region_name, " (", region_path, ")")
	
	region_select_dialog.hide()
	
	# Rozpocznij grę w wybranym regionie
	match current_mode:
		"start", "sandbox", "career":
			_start_single_player(region_path)
		_:
			print("MainMenuUI: Nieznany tryb: ", current_mode)

## Pokazuje okno wyboru regionu
func _show_region_selection() -> void:
	region_select_dialog.popup_centered()

## Rozpoczyna tryb single player
func _start_single_player(region_path: String) -> void:
	print("MainMenuUI: Rozpoczynanie single player w: ", region_path)
	
	if GameManager:
		GameManager.start_game(region_path)
	else:
		push_error("MainMenuUI: GameManager nie jest dostępny!")

## Rozpoczyna tryb multiplayer
func _start_multiplayer() -> void:
	print("MainMenuUI: Rozpoczynanie multiplayer")
	
	if GameManager:
		GameManager.start_multiplayer()
	else:
		push_error("MainMenuUI: GameManager nie jest dostępny!")

## Pokazuje placeholder ustawień
func _show_settings_placeholder() -> void:
	var dialog = AcceptDialog.new()
	dialog.title = "Ustawienia"
	dialog.dialog_text = "Menu ustawień będzie dostępne w przyszłej wersji.\n\nDostępne ustawienia:\n- Grafika\n- Dźwięk\n- Sterowanie\n- Gameplay"
	dialog.initial_position = Window.WINDOW_INITIAL_POSITION_CENTER_ON_SCREEN
	
	add_child(dialog)
	dialog.popup_centered()
	
	# Usuń dialog po zamknięciu
	dialog.connect("confirmed", func(): dialog.queue_free())
	dialog.connect("canceled", func(): dialog.queue_free())

## Callback: Zmiana stanu gry
func _on_game_state_changed(new_state: GameManager.GameState) -> void:
	match new_state:
		GameManager.GameState.MAIN_MENU:
			show()
		GameManager.GameState.LOADING:
			hide()
		GameManager.GameState.PLAYING, GameManager.GameState.MULTIPLAYER:
			hide()
		GameManager.GameState.PAUSED:
			# Menu pozostaje ukryte podczas pauzy
			pass

func _input(event: InputEvent) -> void:
	# ESC w menu głównym zamyka grę
	if event.is_action_pressed("ui_cancel") and visible:
		_on_quit_button_pressed()