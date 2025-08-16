using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExtremeRacing.Infrastructure;

namespace ExtremeRacing.Managers
{
	public enum GameState
	{
		MainMenu,
		Exploring,
		Racing,
		Paused
	}

	public class GameManager : Singleton<GameManager>
	{
		public const string SceneMainMenu = "MainMenu";
		public const string SceneGorski = "Region_GorskiSzczyt";
		public const string ScenePustynny = "Region_PustynnyKanion";
		public const string SceneMiasto = "Region_MiastoNocy";
		public const string ScenePort = "Region_PortWyscigowy";
		public const string SceneTor = "Region_TorMistrzow";

		[SerializeField] private GameState _state = GameState.MainMenu;
		public GameState State => _state;

		[SerializeField] private int _targetFps = 60;

		protected override void Awake()
		{
			base.Awake();
			Application.targetFrameRate = _targetFps;
		}

		public void SetState(GameState newState)
		{
			_state = newState;
			Time.timeScale = _state == GameState.Paused ? 0f : 1f;
		}

		public void LoadScene(string sceneName)
		{
			StartCoroutine(LoadSceneCoroutine(sceneName));
		}

		private IEnumerator LoadSceneCoroutine(string sceneName)
		{
			var async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
			while (!async.isDone)
			{
				yield return null;
			}
			_state = sceneName == SceneMainMenu ? GameState.MainMenu : GameState.Exploring;
		}
	}
}