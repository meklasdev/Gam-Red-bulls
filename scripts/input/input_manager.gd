extends Node
class_name InputManager

## Menedżer wejścia - obsługuje input z klawiatury, pada i dotyku
## Singleton dostępny globalnie jako InputManager

signal input_device_changed(device_type: InputDevice)

enum InputDevice {
	KEYBOARD_MOUSE,  ## Klawiatura i mysz
	GAMEPAD,        ## Pad/kontroler
	TOUCH           ## Ekran dotykowy
}

var current_device := InputDevice.KEYBOARD_MOUSE
var gamepad_connected := false
var touch_enabled := false

# Wirtualne przyciski mobilne
var virtual_buttons := {}
var virtual_joystick_left: Vector2
var virtual_joystick_right: Vector2

# Ustawienia czułości
var steering_sensitivity := 1.0
var touch_sensitivity := 1.0
var gamepad_deadzone := 0.2

func _ready() -> void:
	print("InputManager: Inicjalizacja...")
	
	# Sprawdź czy jesteśmy na urządzeniu mobilnym
	touch_enabled = OS.has_feature("mobile")
	
	# Połącz sygnały pada
	Input.joy_connection_changed.connect(_on_joy_connection_changed)
	
	# Sprawdź aktualnie podłączone pady
	_update_gamepad_status()
	
	print("InputManager: Gotowy! Urządzenie: ", InputDevice.keys()[current_device])

func _input(event: InputEvent) -> void:
	# Automatyczna detekcja typu urządzenia wejściowego
	if event is InputEventKey or event is InputEventMouseButton or event is InputEventMouseMotion:
		_set_input_device(InputDevice.KEYBOARD_MOUSE)
	elif event is InputEventJoypadButton or event is InputEventJoypadMotion:
		_set_input_device(InputDevice.GAMEPAD)
	elif event is InputEventScreenTouch or event is InputEventScreenDrag:
		_set_input_device(InputDevice.TOUCH)

## Zwraca siłę naciśnięcia akcji z uwzględnieniem deadzone dla pada
func get_action_strength_with_deadzone(action: String) -> float:
	var strength = Input.get_action_strength(action)
	
	if current_device == InputDevice.GAMEPAD and abs(strength) < gamepad_deadzone:
		return 0.0
	
	return strength

## Zwraca wektor sterowania (lewo/prawo, przód/tył)
func get_movement_vector() -> Vector2:
	var vector = Vector2.ZERO
	
	match current_device:
		InputDevice.KEYBOARD_MOUSE:
			vector.x = Input.get_action_strength("steer_right") - Input.get_action_strength("steer_left")
			vector.y = Input.get_action_strength("move_forward") - Input.get_action_strength("move_backward")
		
		InputDevice.GAMEPAD:
			# Użyj analogowych drążków
			vector.x = Input.get_joy_axis(0, JOY_AXIS_LEFT_X)
			vector.y = -Input.get_joy_axis(0, JOY_AXIS_LEFT_Y)  # Odwróć Y dla intuicyjności
			
			# Zastosuj deadzone
			if abs(vector.x) < gamepad_deadzone:
				vector.x = 0.0
			if abs(vector.y) < gamepad_deadzone:
				vector.y = 0.0
		
		InputDevice.TOUCH:
			# Użyj wirtualnego joysticka
			vector = virtual_joystick_left
	
	return vector * steering_sensitivity

## Zwraca wektor kamery/drona (dla trybu drona)
func get_camera_vector() -> Vector2:
	var vector = Vector2.ZERO
	
	match current_device:
		InputDevice.KEYBOARD_MOUSE:
			vector.x = Input.get_action_strength("drone_right") - Input.get_action_strength("drone_left")
			vector.y = Input.get_action_strength("drone_up") - Input.get_action_strength("drone_down")
		
		InputDevice.GAMEPAD:
			# Użyj prawego drążka
			vector.x = Input.get_joy_axis(0, JOY_AXIS_RIGHT_X)
			vector.y = -Input.get_joy_axis(0, JOY_AXIS_RIGHT_Y)
			
			# Zastosuj deadzone
			if abs(vector.x) < gamepad_deadzone:
				vector.x = 0.0
			if abs(vector.y) < gamepad_deadzone:
				vector.y = 0.0
		
		InputDevice.TOUCH:
			# Użyj prawego wirtualnego joysticka
			vector = virtual_joystick_right
	
	return vector

## Sprawdza czy akcja jest wciśnięta (z obsługą wirtualnych przycisków)
func is_action_pressed(action: String) -> bool:
	if current_device == InputDevice.TOUCH and action in virtual_buttons:
		return virtual_buttons[action]
	
	return Input.is_action_pressed(action)

## Sprawdza czy akcja została właśnie wciśnięta
func is_action_just_pressed(action: String) -> bool:
	if current_device == InputDevice.TOUCH and action in virtual_buttons:
		# Dla touch implementujemy prostą logikę "just pressed"
		return virtual_buttons.get(action + "_just_pressed", false)
	
	return Input.is_action_just_pressed(action)

## Ustawia stan wirtualnego przycisku (dla UI mobilnego)
func set_virtual_button(action: String, pressed: bool) -> void:
	var was_pressed = virtual_buttons.get(action, false)
	virtual_buttons[action] = pressed
	virtual_buttons[action + "_just_pressed"] = pressed and not was_pressed

## Ustawia pozycję wirtualnego joysticka
func set_virtual_joystick(stick: String, value: Vector2) -> void:
	match stick:
		"left":
			virtual_joystick_left = value
		"right":
			virtual_joystick_right = value

## Zwraca aktualny typ urządzenia wejściowego
func get_current_device() -> InputDevice:
	return current_device

## Sprawdza czy pad jest podłączony
func is_gamepad_connected() -> bool:
	return gamepad_connected

## Sprawdza czy jesteśmy na urządzeniu dotykowym
func is_touch_device() -> bool:
	return touch_enabled

## Ustawia czułość sterowania
func set_steering_sensitivity(value: float) -> void:
	steering_sensitivity = clamp(value, 0.1, 3.0)

## Ustawia deadzone pada
func set_gamepad_deadzone(value: float) -> void:
	gamepad_deadzone = clamp(value, 0.0, 0.5)

## Callback przy zmianie stanu pada
func _on_joy_connection_changed(device: int, connected: bool) -> void:
	print("InputManager: Pad ", device, " ", "podłączony" if connected else "odłączony")
	_update_gamepad_status()

## Aktualizuje status pada
func _update_gamepad_status() -> void:
	var pads = Input.get_connected_joypads()
	gamepad_connected = pads.size() > 0
	
	if gamepad_connected and current_device == InputDevice.KEYBOARD_MOUSE:
		# Automatycznie przełącz na pad jeśli został podłączony
		_set_input_device(InputDevice.GAMEPAD)

## Zmienia aktualny typ urządzenia wejściowego
func _set_input_device(device: InputDevice) -> void:
	if current_device == device:
		return
	
	print("InputManager: Zmiana urządzenia wejściowego na: ", InputDevice.keys()[device])
	current_device = device
	input_device_changed.emit(device)

## Czyści stan wirtualnych przycisków
func clear_virtual_inputs() -> void:
	virtual_buttons.clear()
	virtual_joystick_left = Vector2.ZERO
	virtual_joystick_right = Vector2.ZERO