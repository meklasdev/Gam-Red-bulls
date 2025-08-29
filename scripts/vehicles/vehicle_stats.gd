extends Resource
class_name VehicleStats

## Zasób zawierający statystyki pojazdów
## Używany do konfiguracji różnych typów pojazdów

@export_group("Podstawowe")
@export var vehicle_name: String = ""
@export var vehicle_type: String = ""  # "car", "bike", "moto"
@export var description: String = ""
@export var unlock_level: int = 1
@export var price: int = 0

@export_group("Wydajność")
@export var max_speed: float = 100.0       ## Maksymalna prędkość (km/h)
@export var acceleration: float = 1.0       ## Przyspieszenie (mnożnik)
@export var handling: float = 1.0           ## Prowadzenie (mnożnik)
@export var braking: float = 1.0            ## Hamowanie (mnożnik)
@export var durability: float = 1.0         ## Wytrzymałość (mnożnik)

@export_group("Specjalne")
@export var drift_capability: float = 1.0   ## Zdolność do driftu
@export var off_road_capability: float = 1.0 ## Zdolność jazdy w terenie
@export var nitro_capacity: float = 100.0   ## Pojemność nitro
@export var fuel_efficiency: float = 1.0    ## Efektywność paliwowa

@export_group("Wizualne")
@export var vehicle_mesh: PackedScene       ## Scena z modelem pojazdu
@export var engine_sound: AudioStream       ## Dźwięk silnika
@export var tire_sound: AudioStream         ## Dźwięk opon
@export var horn_sound: AudioStream         ## Dźwięk klaksonu

## Zwraca ogólną ocenę pojazdu (0-100)
func get_overall_rating() -> float:
	var rating = (max_speed / 200.0 * 25.0 +
				 acceleration * 25.0 +
				 handling * 25.0 +
				 braking * 25.0)
	return clamp(rating, 0.0, 100.0)

## Zwraca kategorię pojazdu na podstawie statystyk
func get_vehicle_category() -> String:
	var rating = get_overall_rating()
	
	if rating >= 80:
		return "Supercar"
	elif rating >= 60:
		return "Sport"
	elif rating >= 40:
		return "Standard"
	else:
		return "Beginner"

## Sprawdza czy pojazd jest odblokowany
func is_unlocked(player_level: int) -> bool:
	return player_level >= unlock_level

## Zwraca koszt naprawy
func get_repair_cost() -> int:
	return int(price * 0.1)  # 10% ceny pojazdu

## Zwraca słabą stronę pojazdu
func get_weakness() -> String:
	var stats = {
		"Prędkość": max_speed / 200.0,
		"Przyspieszenie": acceleration,
		"Prowadzenie": handling,
		"Hamowanie": braking
	}
	
	var min_stat = ""
	var min_value = 999.0
	
	for stat_name in stats:
		if stats[stat_name] < min_value:
			min_value = stats[stat_name]
			min_stat = stat_name
	
	return min_stat

## Zwraca mocną stronę pojazdu
func get_strength() -> String:
	var stats = {
		"Prędkość": max_speed / 200.0,
		"Przyspieszenie": acceleration,
		"Prowadzenie": handling,
		"Hamowanie": braking
	}
	
	var max_stat = ""
	var max_value = 0.0
	
	for stat_name in stats:
		if stats[stat_name] > max_value:
			max_value = stats[stat_name]
			max_stat = stat_name
	
	return max_stat