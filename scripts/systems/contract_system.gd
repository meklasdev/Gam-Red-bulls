extends Node
class_name ContractSystem

## System kontraktów - zarządza popularnością, specjalizacjami i nielegalnych częściami
## Singleton dostępny globalnie jako ContractSystem

signal reputation_changed(old_value: int, new_value: int)
signal specialization_unlocked(specialization: String)
signal illegal_part_discovered(part_name: String)
signal contract_available(contract: Dictionary)
signal contract_completed(contract: Dictionary, rewards: Dictionary)

enum Specialization {
	STREET_RACING,    ## Wyścigi uliczne
	DRIFT,           ## Drift
	STUNT,           ## Stunty
	OFF_ROAD,        ## Jazda terenowa
	TIME_TRIAL,      ## Próby czasowe
	ILLEGAL_RACING   ## Nielegalne wyścigi
}

# Dane systemu kontraktów
var reputation := 0
var popularity := 0
var unlocked_specializations: Array[Specialization] = []
var discovered_illegal_parts: Array[String] = []
var active_contracts: Array[Dictionary] = []
var completed_contracts: Array[String] = []

# Progi odblokowań
var specialization_thresholds := {
	Specialization.STREET_RACING: 0,     # Dostępne od początku
	Specialization.DRIFT: 100,
	Specialization.STUNT: 250,
	Specialization.OFF_ROAD: 150,
	Specialization.TIME_TRIAL: 200,
	Specialization.ILLEGAL_RACING: 500
}

# Szablony kontraktów
var contract_templates := [
	{
		"id": "street_race_beginner",
		"title": "Wyścig dla Początkujących",
		"description": "Ukończ wyścig uliczny w top 3",
		"specialization": Specialization.STREET_RACING,
		"difficulty": 1,
		"requirements": {"reputation": 0},
		"rewards": {"money": 500, "reputation": 15, "popularity": 10},
		"objectives": ["Ukończ wyścig w top 3"]
	},
	{
		"id": "drift_challenge",
		"title": "Wyzwanie Driftowe",
		"description": "Zdobądź 3000 punktów za drift",
		"specialization": Specialization.DRIFT,
		"difficulty": 2,
		"requirements": {"reputation": 100},
		"rewards": {"money": 800, "reputation": 25, "popularity": 20},
		"objectives": ["Zdobądź 3000 punktów za drift"]
	},
	{
		"id": "stunt_spectacular",
		"title": "Spektakularny Stunt",
		"description": "Wykonaj kombinację 5 różnych stuntów",
		"specialization": Specialization.STUNT,
		"difficulty": 3,
		"requirements": {"reputation": 250},
		"rewards": {"money": 1200, "reputation": 40, "popularity": 35},
		"objectives": ["Wykonaj 5 różnych stuntów w jednej jeździe"]
	},
	{
		"id": "illegal_night_race",
		"title": "Nielegalny Nocny Wyścig",
		"description": "Wygraj nielegalny wyścig w nocy (wysokie ryzyko!)",
		"specialization": Specialization.ILLEGAL_RACING,
		"difficulty": 5,
		"requirements": {"reputation": 500, "time_of_day": "night"},
		"rewards": {"money": 3000, "reputation": 100, "popularity": 80, "illegal_part": "turbo_boost"},
		"objectives": ["Wygraj wyścig", "Unikaj policji"],
		"risks": ["Utrata reputacji przy porażce", "Możliwość aresztowania"]
	}
]

func _ready() -> void:
	print("ContractSystem: Inicjalizacja...")
	
	# Wczytaj dane z pliku
	_load_contract_data()
	
	# Odblokuj początkowe specjalizacje
	_check_specialization_unlocks()
	
	# Wygeneruj początkowe kontrakty
	_generate_initial_contracts()
	
	print("ContractSystem: Gotowy! Reputacja: ", reputation, ", Popularność: ", popularity)

## Inicjalizuje kontrakty dla aktualnej sceny
func init_contracts() -> void:
	print("ContractSystem: Inicjalizacja kontraktów dla sceny")
	_generate_scene_contracts()

## Dodaje reputację
func add_reputation(amount: int) -> void:
	var old_reputation = reputation
	reputation += amount
	
	print("ContractSystem: Reputacja ", old_reputation, " -> ", reputation, " (+", amount, ")")
	reputation_changed.emit(old_reputation, reputation)
	
	# Sprawdź odblokowywania
	_check_specialization_unlocks()
	
	# Zapisz dane
	_save_contract_data()

## Dodaje popularność
func add_popularity(amount: int) -> void:
	popularity += amount
	print("ContractSystem: Popularność +", amount, " (razem: ", popularity, ")")

## Sprawdza i odblokowuje nowe specjalizacje
func _check_specialization_unlocks() -> void:
	for spec in Specialization.values():
		if spec in unlocked_specializations:
			continue
		
		var threshold = specialization_thresholds[spec]
		if reputation >= threshold:
			unlocked_specializations.append(spec)
			var spec_name = _get_specialization_name(spec)
			print("ContractSystem: Odblokowano specjalizację: ", spec_name)
			specialization_unlocked.emit(spec_name)

## Zwraca nazwę specjalizacji
func _get_specialization_name(spec: Specialization) -> String:
	match spec:
		Specialization.STREET_RACING:
			return "Wyścigi Uliczne"
		Specialization.DRIFT:
			return "Drift"
		Specialization.STUNT:
			return "Stunty"
		Specialization.OFF_ROAD:
			return "Jazda Terenowa"
		Specialization.TIME_TRIAL:
			return "Próby Czasowe"
		Specialization.ILLEGAL_RACING:
			return "Nielegalne Wyścigi"
		_:
			return "Nieznana"

## Generuje początkowe kontrakty
func _generate_initial_contracts() -> void:
	print("ContractSystem: Generowanie początkowych kontraktów...")
	
	for template in contract_templates:
		if _is_contract_available(template):
			var contract = template.duplicate(true)
			contract["id"] = template["id"] + "_" + str(Time.get_time_dict_from_system()["unix"])
			active_contracts.append(contract)
			contract_available.emit(contract)

## Generuje kontrakty dla aktualnej sceny
func _generate_scene_contracts() -> void:
	var scene_name = SceneLoader.get_current_scene_name()
	print("ContractSystem: Generowanie kontraktów dla sceny: ", scene_name)
	
	# Dodaj kontrakty specyficzne dla sceny
	match scene_name:
		"gorski_szczyt":
			_add_mountain_contracts()
		"pustynny_kanion":
			_add_desert_contracts()
		"miasto_nocy":
			_add_city_contracts()
		"port_wyscigowy":
			_add_port_contracts()
		"tor_mistrzow":
			_add_championship_contracts()

## Dodaje kontrakty górskie
func _add_mountain_contracts() -> void:
	var mountain_contract = {
		"id": "mountain_climb_" + str(randi()),
		"title": "Górska Wspinaczka",
		"description": "Pokonaj trasę górską w czasie poniżej 3 minut",
		"specialization": Specialization.TIME_TRIAL,
		"difficulty": 2,
		"requirements": {"reputation": 50},
		"rewards": {"money": 700, "reputation": 20, "popularity": 15},
		"objectives": ["Ukończ trasę w czasie < 3:00"],
		"region": "gorski_szczyt"
	}
	
	if _is_contract_available(mountain_contract):
		active_contracts.append(mountain_contract)
		contract_available.emit(mountain_contract)

## Dodaje kontrakty pustynne
func _add_desert_contracts() -> void:
	var desert_contract = {
		"id": "desert_drift_" + str(randi()),
		"title": "Pustynny Drift",
		"description": "Wykonaj długi drift w kanionie",
		"specialization": Specialization.DRIFT,
		"difficulty": 2,
		"requirements": {"reputation": 100},
		"rewards": {"money": 900, "reputation": 30, "popularity": 25},
		"objectives": ["Utrzymaj drift przez 10 sekund"],
		"region": "pustynny_kanion"
	}
	
	if _is_contract_available(desert_contract):
		active_contracts.append(desert_contract)
		contract_available.emit(desert_contract)

## Dodaje kontrakty miejskie
func _add_city_contracts() -> void:
	# Sprawdź czy jest noc dla nielegalnych wyścigów
	var is_night = TimeOfDayManager and not TimeOfDayManager.is_daytime()
	
	if is_night and Specialization.ILLEGAL_RACING in unlocked_specializations:
		var illegal_contract = {
			"id": "illegal_street_race_" + str(randi()),
			"title": "Nielegalny Wyścig Uliczny",
			"description": "Wygraj nielegalny wyścig w centrum miasta",
			"specialization": Specialization.ILLEGAL_RACING,
			"difficulty": 4,
			"requirements": {"reputation": 400, "time_of_day": "night"},
			"rewards": {"money": 2500, "reputation": 80, "popularity": 60, "illegal_part": "nos_system"},
			"objectives": ["Wygraj wyścig", "Unikaj policji przez 2 minuty"],
			"risks": ["Utrata 50 reputacji przy porażce"],
			"region": "miasto_nocy"
		}
		
		if _is_contract_available(illegal_contract):
			active_contracts.append(illegal_contract)
			contract_available.emit(illegal_contract)

## Dodaje kontrakty portowe
func _add_port_contracts() -> void:
	var port_contract = {
		"id": "port_race_" + str(randi()),
		"title": "Wyścig Portowy",
		"description": "Ukończ wyścig między kontenerami",
		"specialization": Specialization.STREET_RACING,
		"difficulty": 3,
		"requirements": {"reputation": 200},
		"rewards": {"money": 1100, "reputation": 35, "popularity": 30},
		"objectives": ["Ukończ wyścig na podium"],
		"region": "port_wyscigowy"
	}
	
	if _is_contract_available(port_contract):
		active_contracts.append(port_contract)
		contract_available.emit(port_contract)

## Dodaje kontrakty mistrzowskie
func _add_championship_contracts() -> void:
	var championship_contract = {
		"id": "championship_qualifier_" + str(randi()),
		"title": "Kwalifikacje Mistrzowskie",
		"description": "Ustanów rekord toru na Torze Mistrzów",
		"specialization": Specialization.TIME_TRIAL,
		"difficulty": 5,
		"requirements": {"reputation": 600},
		"rewards": {"money": 5000, "reputation": 150, "popularity": 100},
		"objectives": ["Ustanów nowy rekord toru"],
		"region": "tor_mistrzow"
	}
	
	if _is_contract_available(championship_contract):
		active_contracts.append(championship_contract)
		contract_available.emit(championship_contract)

## Sprawdza czy kontrakt jest dostępny
func _is_contract_available(contract: Dictionary) -> bool:
	# Sprawdź wymagania reputacji
	if contract.has("requirements") and contract["requirements"].has("reputation"):
		if reputation < contract["requirements"]["reputation"]:
			return false
	
	# Sprawdź wymagania pory dnia
	if contract.has("requirements") and contract["requirements"].has("time_of_day"):
		var required_time = contract["requirements"]["time_of_day"]
		if required_time == "night" and TimeOfDayManager and TimeOfDayManager.is_daytime():
			return false
		elif required_time == "day" and TimeOfDayManager and not TimeOfDayManager.is_daytime():
			return false
	
	# Sprawdź czy specjalizacja jest odblokowana
	if contract.has("specialization"):
		if not contract["specialization"] in unlocked_specializations:
			return false
	
	return true

## Kończy kontrakt
func complete_contract(contract_id: String, success: bool = true) -> void:
	var contract = _get_contract_by_id(contract_id)
	if not contract:
		return
	
	print("ContractSystem: Kończenie kontraktu: ", contract["title"], " - ", "sukces" if success else "porażka")
	
	# Usuń z aktywnych
	active_contracts.erase(contract)
	
	if success:
		# Przyznaj nagrody
		_award_contract_rewards(contract)
		completed_contracts.append(contract_id)
		contract_completed.emit(contract, contract.get("rewards", {}))
	else:
		# Zastosuj kary za porażkę
		_apply_contract_penalties(contract)
	
	_save_contract_data()

## Przyznaje nagrody za kontrakt
func _award_contract_rewards(contract: Dictionary) -> void:
	var rewards = contract.get("rewards", {})
	
	if rewards.has("money"):
		GameManager.player_data.money += rewards["money"]
		print("ContractSystem: Przyznano ", rewards["money"], " pieniędzy")
	
	if rewards.has("reputation"):
		add_reputation(rewards["reputation"])
	
	if rewards.has("popularity"):
		add_popularity(rewards["popularity"])
	
	if rewards.has("illegal_part"):
		_discover_illegal_part(rewards["illegal_part"])

## Stosuje kary za porażkę kontraktu
func _apply_contract_penalties(contract: Dictionary) -> void:
	var risks = contract.get("risks", [])
	
	for risk in risks:
		if "Utrata" in risk and "reputacji" in risk:
			# Wyciągnij liczbę z tekstu ryzyka
			var regex = RegEx.new()
			regex.compile("\\d+")
			var result = regex.search(risk)
			if result:
				var penalty = int(result.get_string())
				add_reputation(-penalty)

## Odkrywa nielegalną część
func _discover_illegal_part(part_name: String) -> void:
	if part_name in discovered_illegal_parts:
		return
	
	discovered_illegal_parts.append(part_name)
	print("ContractSystem: Odkryto nielegalną część: ", part_name)
	illegal_part_discovered.emit(part_name)

## Zwraca kontrakt po ID
func _get_contract_by_id(contract_id: String) -> Dictionary:
	for contract in active_contracts:
		if contract["id"] == contract_id:
			return contract
	return {}

## Ładuje dane kontraktów z pliku
func _load_contract_data() -> void:
	var config = ConfigFile.new()
	var err = config.load("user://contracts.cfg")
	
	if err != OK:
		print("ContractSystem: Brak pliku danych kontraktów")
		return
	
	reputation = config.get_value("contracts", "reputation", 0)
	popularity = config.get_value("contracts", "popularity", 0)
	completed_contracts = config.get_value("contracts", "completed", [])
	discovered_illegal_parts = config.get_value("contracts", "illegal_parts", [])
	
	print("ContractSystem: Dane kontraktów załadowane")

## Zapisuje dane kontraktów do pliku
func _save_contract_data() -> void:
	var config = ConfigFile.new()
	config.set_value("contracts", "reputation", reputation)
	config.set_value("contracts", "popularity", popularity)
	config.set_value("contracts", "completed", completed_contracts)
	config.set_value("contracts", "illegal_parts", discovered_illegal_parts)
	
	var err = config.save("user://contracts.cfg")
	if err == OK:
		print("ContractSystem: Dane kontraktów zapisane")
	else:
		print("ContractSystem: Błąd zapisu danych kontraktów: ", err)

## Zwraca listę aktywnych kontraktów
func get_active_contracts() -> Array[Dictionary]:
	return active_contracts

## Zwraca listę odblokowanych specjalizacji
func get_unlocked_specializations() -> Array[Specialization]:
	return unlocked_specializations

## Zwraca listę odkrytych nielegalnych części
func get_discovered_illegal_parts() -> Array[String]:
	return discovered_illegal_parts

## Zwraca aktualną reputację
func get_reputation() -> int:
	return reputation

## Zwraca aktualną popularność
func get_popularity() -> int:
	return popularity