using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralny menedżer gry: stan, ładowanie scen, integracja systemów.
/// </summary>
public class GameManager : Singleton<GameManager>
{
	public enum GameState
	{
		MainMenu,
		Playing,
		Paused
	}

	[SerializeField] private SceneLoader sceneLoader;
	[SerializeField] private string mainMenuSceneName = "MainMenu";

	public GameState State { get; private set; } = GameState.MainMenu;

	protected override void Awake()
	{
		base.Awake();
		if (sceneLoader == null) sceneLoader = FindObjectOfType<SceneLoader>();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	/// <summary>
	/// Start gry – przejdź do sceny menu.
	/// </summary>
	private void Start()
	{
		if (SceneManager.GetActiveScene().name != mainMenuSceneName)
		{
			LoadMainMenu();
		}
	}

	public void LoadMainMenu()
	{
		State = GameState.MainMenu;
		LoadScene(mainMenuSceneName);
	}

	public void StartGameInRegion(string regionSceneName)
	{
		State = GameState.Playing;
		LoadScene(regionSceneName);
	}

	public void PauseGame()
	{
		if (State != GameState.Playing) return;
		State = GameState.Paused;
		Time.timeScale = 0f;
	}

	public void ResumeGame()
	{
		if (State != GameState.Paused) return;
		State = GameState.Playing;
		Time.timeScale = 1f;
	}

	private void LoadScene(string sceneName)
	{
		if (sceneLoader != null)
		{
			sceneLoader.LoadSceneByName(sceneName);
		}
		else
		{
			SceneManager.LoadScene(sceneName);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Inicjalizacja systemów po załadowaniu sceny
		var weather = FindObjectOfType<WeatherManager>();
		var timeOfDay = FindObjectOfType<TimeOfDayManager>();
		var missionSystem = FindObjectOfType<MissionSystem>();
		var contractSystem = FindObjectOfType<ContractSystem>();

		if (weather != null) weather.ApplyCurrentWeatherImmediate();
		if (timeOfDay != null) timeOfDay.ApplySettingsImmediate();
		if (missionSystem != null) missionSystem.PrepareSceneMissions();
		if (contractSystem != null) contractSystem.InitializeContracts();
	}
}