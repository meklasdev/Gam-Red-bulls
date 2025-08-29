extends Node
class_name WeatherManager

## Menedżer pogody - kontroluje efekty pogodowe, cząsteczki i wpływ na rozgrywkę
## Singleton dostępny globalnie jako WeatherManager

signal weather_changed(old_weather: Weather, new_weather: Weather)
signal weather_intensity_changed(intensity: float)

enum Weather {
	CLEAR,      ## Pogodnie
	RAIN,       ## Deszcz
	SNOW,       ## Śnieg
	SANDSTORM,  ## Burza piaskowa
	FOG         ## Mgła
}

var current_weather := Weather.CLEAR
var weather_intensity := 0.0  ## 0.0 - 1.0, siła pogody
var transition_speed := 1.0   ## Prędkość przejścia między pogodami

# Referencje do systemów cząsteczek
var rain_particles: GPUParticles3D
var snow_particles: GPUParticles3D
var sandstorm_particles: GPUParticles3D
var fog_particles: GPUParticles3D

# Referencje do środowiska
var world_environment: Environment
var wind_audio: AudioStreamPlayer3D

# Ustawienia wpływu pogody na rozgrywkę
var weather_effects := {
	Weather.CLEAR: {"grip_multiplier": 1.0, "visibility": 1.0, "audio_volume": 0.0},
	Weather.RAIN: {"grip_multiplier": 0.7, "visibility": 0.8, "audio_volume": 0.6},
	Weather.SNOW: {"grip_multiplier": 0.5, "visibility": 0.6, "audio_volume": 0.4},
	Weather.SANDSTORM: {"grip_multiplier": 0.8, "visibility": 0.3, "audio_volume": 0.8},
	Weather.FOG: {"grip_multiplier": 1.0, "visibility": 0.4, "audio_volume": 0.2}
}

func _ready() -> void:
	print("WeatherManager: Inicjalizacja...")
	set_process(true)

func _process(delta: float) -> void:
	# Płynne przejścia intensywności pogody
	_update_weather_effects(delta)

## Ustawia nową pogodę
func set_weather(new_weather: Weather, intensity: float = 1.0) -> void:
	if new_weather == current_weather:
		weather_intensity = clamp(intensity, 0.0, 1.0)
		weather_intensity_changed.emit(weather_intensity)
		return
	
	var old_weather = current_weather
	current_weather = new_weather
	weather_intensity = clamp(intensity, 0.0, 1.0)
	
	print("WeatherManager: Zmiana pogody z ", Weather.keys()[old_weather], " na ", Weather.keys()[new_weather])
	
	_apply_weather_change()
	weather_changed.emit(old_weather, new_weather)
	weather_intensity_changed.emit(weather_intensity)

## Ustawia intensywność aktualnej pogody
func set_weather_intensity(intensity: float) -> void:
	weather_intensity = clamp(intensity, 0.0, 1.0)
	weather_intensity_changed.emit(weather_intensity)

## Natychmiast stosuje aktualną pogodę
func apply_current_weather_immediate() -> void:
	_apply_weather_change()

## Losowo ustawia pogodę
func set_random_weather() -> void:
	var weather_types = Weather.values()
	var random_weather = weather_types[randi() % weather_types.size()]
	var random_intensity = randf_range(0.3, 1.0)
	set_weather(random_weather, random_intensity)

## Rejestruje system cząsteczek deszczu
func register_rain_particles(particles: GPUParticles3D) -> void:
	rain_particles = particles
	print("WeatherManager: Zarejestrowano cząsteczki deszczu")

## Rejestruje system cząsteczek śniegu
func register_snow_particles(particles: GPUParticles3D) -> void:
	snow_particles = particles
	print("WeatherManager: Zarejestrowano cząsteczki śniegu")

## Rejestruje system cząsteczek burzy piaskowej
func register_sandstorm_particles(particles: GPUParticles3D) -> void:
	sandstorm_particles = particles
	print("WeatherManager: Zarejestrowano cząsteczki burzy piaskowej")

## Rejestruje system cząsteczek mgły
func register_fog_particles(particles: GPUParticles3D) -> void:
	fog_particles = particles
	print("WeatherManager: Zarejestrowano cząsteczki mgły")

## Rejestruje środowisko świata
func register_world_environment(env: Environment) -> void:
	world_environment = env
	print("WeatherManager: Zarejestrowano środowisko świata")

## Rejestruje dźwięk wiatru
func register_wind_audio(audio: AudioStreamPlayer3D) -> void:
	wind_audio = audio
	print("WeatherManager: Zarejestrowano dźwięk wiatru")

## Zwraca aktualną pogodę
func get_current_weather() -> Weather:
	return current_weather

## Zwraca intensywność aktualnej pogody
func get_weather_intensity() -> float:
	return weather_intensity

## Zwraca nazwę aktualnej pogody
func get_weather_name() -> String:
	match current_weather:
		Weather.CLEAR:
			return "Pogodnie"
		Weather.RAIN:
			return "Deszcz"
		Weather.SNOW:
			return "Śnieg"
		Weather.SANDSTORM:
			return "Burza piaskowa"
		Weather.FOG:
			return "Mgła"
		_:
			return "Nieznana"

## Zwraca mnożnik przyczepności dla aktualnej pogody
func get_grip_multiplier() -> float:
	var base_multiplier = weather_effects[current_weather]["grip_multiplier"]
	return lerp(1.0, base_multiplier, weather_intensity)

## Zwraca mnożnik widoczności dla aktualnej pogody
func get_visibility_multiplier() -> float:
	var base_multiplier = weather_effects[current_weather]["visibility"]
	return lerp(1.0, base_multiplier, weather_intensity)

## Zwraca czy pogoda wpływa na prowadzenie
func affects_driving() -> bool:
	return current_weather != Weather.CLEAR and weather_intensity > 0.1

## Stosuje zmiany pogody do wszystkich systemów
func _apply_weather_change() -> void:
	_update_particle_systems()
	_update_environment()
	_update_audio()

## Aktualizuje systemy cząsteczek
func _update_particle_systems() -> void:
	# Wyłącz wszystkie systemy cząsteczek
	_set_particles_active(rain_particles, false)
	_set_particles_active(snow_particles, false)
	_set_particles_active(sandstorm_particles, false)
	_set_particles_active(fog_particles, false)
	
	# Włącz odpowiedni system dla aktualnej pogody
	match current_weather:
		Weather.RAIN:
			_set_particles_active(rain_particles, true)
			_set_particles_intensity(rain_particles, weather_intensity)
		Weather.SNOW:
			_set_particles_active(snow_particles, true)
			_set_particles_intensity(snow_particles, weather_intensity)
		Weather.SANDSTORM:
			_set_particles_active(sandstorm_particles, true)
			_set_particles_intensity(sandstorm_particles, weather_intensity)
		Weather.FOG:
			_set_particles_active(fog_particles, true)
			_set_particles_intensity(fog_particles, weather_intensity)

## Ustawia aktywność systemu cząsteczek
func _set_particles_active(particles: GPUParticles3D, active: bool) -> void:
	if not particles:
		return
	
	particles.visible = active
	particles.emitting = active

## Ustawia intensywność systemu cząsteczek
func _set_particles_intensity(particles: GPUParticles3D, intensity: float) -> void:
	if not particles or not particles.process_material:
		return
	
	var material = particles.process_material as ParticleProcessMaterial
	if material:
		# Dostosuj ilość cząsteczek na podstawie intensywności
		particles.amount = int(lerp(0, 1000, intensity))
		
		# Dostosuj prędkość emisji
		material.emission_rate_per_second = lerp(0.0, 100.0, intensity)

## Aktualizuje ustawienia środowiska
func _update_environment() -> void:
	if not world_environment:
		return
	
	match current_weather:
		Weather.CLEAR:
			world_environment.fog_enabled = false
		
		Weather.RAIN:
			world_environment.fog_enabled = true
			world_environment.fog_light_color = Color(0.7, 0.7, 0.8)
			world_environment.fog_density = lerp(0.0, 0.02, weather_intensity)
		
		Weather.SNOW:
			world_environment.fog_enabled = true
			world_environment.fog_light_color = Color(0.9, 0.9, 1.0)
			world_environment.fog_density = lerp(0.0, 0.03, weather_intensity)
		
		Weather.SANDSTORM:
			world_environment.fog_enabled = true
			world_environment.fog_light_color = Color(0.8, 0.6, 0.4)
			world_environment.fog_density = lerp(0.0, 0.05, weather_intensity)
		
		Weather.FOG:
			world_environment.fog_enabled = true
			world_environment.fog_light_color = Color(0.8, 0.8, 0.8)
			world_environment.fog_density = lerp(0.0, 0.08, weather_intensity)

## Aktualizuje dźwięki pogody
func _update_audio() -> void:
	if not wind_audio:
		return
	
	var target_volume = weather_effects[current_weather]["audio_volume"] * weather_intensity
	wind_audio.volume_db = linear_to_db(target_volume)
	wind_audio.playing = target_volume > 0.1

## Aktualizuje efekty pogody w czasie rzeczywistym
func _update_weather_effects(delta: float) -> void:
	# Tutaj można dodać animacje przejść między pogodami
	pass