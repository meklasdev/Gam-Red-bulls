extends Node
class_name TimeOfDayManager

## Menedżer pory dnia - kontroluje oświetlenie, kolory i cykl dzień/noc
## Singleton dostępny globalnie jako TimeOfDayManager

signal time_changed(current_time: float)
signal day_night_cycle_changed(is_day: bool)

@export var day_length_seconds := 600.0  ## Długość dnia w sekundach (10 minut)
@export var auto_cycle := true           ## Czy automatycznie zmieniać porę dnia

var current_time := 0.5  ## 0.0 = północ, 0.5 = południe, 1.0 = północ
var is_day := true
var cycle_speed := 1.0

# Referencje do obiektów oświetlenia (ustawiane przez sceny)
var sun_light: DirectionalLight3D
var moon_light: DirectionalLight3D
var world_environment: Environment

# Ustawienia kolorów dla różnych pór dnia
var sunrise_color := Color(1.0, 0.8, 0.6)
var day_color := Color(1.0, 1.0, 0.9)
var sunset_color := Color(1.0, 0.6, 0.4)
var night_color := Color(0.3, 0.4, 0.8)

# Ustawienia intensywności światła
var day_intensity := 1.0
var night_intensity := 0.1

func _ready() -> void:
	print("TimeOfDayManager: Inicjalizacja...")
	set_process(auto_cycle)

func _process(delta: float) -> void:
	if not auto_cycle:
		return
	
	# Aktualizuj czas
	var time_increment = delta / day_length_seconds * cycle_speed
	current_time = fposmod(current_time + time_increment, 1.0)
	
	# Zastosuj zmiany oświetlenia
	_update_lighting()
	
	# Sprawdź zmianę dnia/nocy
	var new_is_day = current_time > 0.25 and current_time < 0.75
	if new_is_day != is_day:
		is_day = new_is_day
		day_night_cycle_changed.emit(is_day)
	
	time_changed.emit(current_time)

## Ustawia aktualną porę dnia (0.0 - 1.0)
func set_time(time: float) -> void:
	current_time = clamp(time, 0.0, 1.0)
	_update_lighting()
	
	var new_is_day = current_time > 0.25 and current_time < 0.75
	if new_is_day != is_day:
		is_day = new_is_day
		day_night_cycle_changed.emit(is_day)
	
	time_changed.emit(current_time)

## Ustawia porę dnia na określoną godzinę (0-24)
func set_hour(hour: float) -> void:
	set_time(hour / 24.0)

## Zwraca aktualną godzinę (0-24)
func get_hour() -> float:
	return current_time * 24.0

## Zwraca aktualną porę dnia jako string
func get_time_of_day_string() -> String:
	var hour = int(get_hour())
	var minute = int((get_hour() - hour) * 60)
	return "%02d:%02d" % [hour, minute]

## Zwraca nazwę aktualnej pory dnia
func get_period_name() -> String:
	var hour = get_hour()
	
	if hour >= 6.0 and hour < 12.0:
		return "Ranek"
	elif hour >= 12.0 and hour < 18.0:
		return "Dzień"
	elif hour >= 18.0 and hour < 22.0:
		return "Wieczór"
	else:
		return "Noc"

## Natychmiast stosuje aktualne ustawienia oświetlenia
func apply_now() -> void:
	_update_lighting()

## Włącza/wyłącza automatyczny cykl dnia
func set_auto_cycle(enabled: bool) -> void:
	auto_cycle = enabled
	set_process(enabled)

## Ustawia prędkość cyklu dnia (1.0 = normalna)
func set_cycle_speed(speed: float) -> void:
	cycle_speed = max(0.0, speed)

## Rejestruje światło słońca
func register_sun_light(light: DirectionalLight3D) -> void:
	sun_light = light
	print("TimeOfDayManager: Zarejestrowano światło słońca")

## Rejestruje światło księżyca
func register_moon_light(light: DirectionalLight3D) -> void:
	moon_light = light
	print("TimeOfDayManager: Zarejestrowano światło księżyca")

## Rejestruje środowisko świata
func register_world_environment(env: Environment) -> void:
	world_environment = env
	print("TimeOfDayManager: Zarejestrowano środowisko świata")

## Aktualizuje oświetlenie na podstawie aktualnej pory dnia
func _update_lighting() -> void:
	if not sun_light:
		return
	
	# Oblicz kąt słońca (obrót wokół osi X)
	var sun_angle = lerp(-10.0, 170.0, current_time)
	sun_light.rotation_degrees.x = sun_angle
	
	# Oblicz kolor i intensywność światła
	var light_color: Color
	var light_intensity: float
	
	if current_time < 0.25:  # Noc (0:00 - 6:00)
		light_color = night_color
		light_intensity = night_intensity
	elif current_time < 0.35:  # Wschód słońca (6:00 - 8:24)
		var t = (current_time - 0.25) / 0.1
		light_color = night_color.lerp(sunrise_color, t)
		light_intensity = lerp(night_intensity, day_intensity, t)
	elif current_time < 0.65:  # Dzień (8:24 - 15:36)
		var t = (current_time - 0.35) / 0.3
		light_color = sunrise_color.lerp(day_color, t)
		light_intensity = day_intensity
	elif current_time < 0.75:  # Zachód słońca (15:36 - 18:00)
		var t = (current_time - 0.65) / 0.1
		light_color = day_color.lerp(sunset_color, t)
		light_intensity = lerp(day_intensity, night_intensity, t)
	else:  # Noc (18:00 - 24:00)
		var t = (current_time - 0.75) / 0.25
		light_color = sunset_color.lerp(night_color, t)
		light_intensity = night_intensity
	
	# Zastosuj ustawienia do światła słońca
	sun_light.light_color = light_color
	sun_light.light_energy = light_intensity
	
	# Ustaw światło księżyca (przeciwne do słońca)
	if moon_light:
		moon_light.rotation_degrees.x = sun_angle + 180.0
		moon_light.light_color = night_color
		moon_light.light_energy = night_intensity * 0.5
		moon_light.visible = not is_day
	
	# Aktualizuj środowisko (mgła, kolor tła itp.)
	if world_environment:
		_update_environment_settings(light_color, light_intensity)

## Aktualizuje ustawienia środowiska
func _update_environment_settings(light_color: Color, light_intensity: float) -> void:
	if not world_environment:
		return
	
	# Ustaw kolor tła nieba
	if world_environment.sky and world_environment.sky.sky_material:
		var sky_material = world_environment.sky.sky_material as ProceduralSkyMaterial
		if sky_material:
			sky_material.sky_top_color = light_color
			sky_material.sky_horizon_color = light_color.darkened(0.3)
	
	# Dostosuj mgłę
	if world_environment.fog_enabled:
		world_environment.fog_light_color = light_color
		world_environment.fog_density = lerp(0.01, 0.005, light_intensity)

## Zwraca aktualny czas jako float (0.0 - 1.0)
func get_current_time() -> float:
	return current_time

## Sprawdza czy jest dzień
func is_daytime() -> bool:
	return is_day

## Sprawdza czy jest noc
func is_nighttime() -> bool:
	return not is_day