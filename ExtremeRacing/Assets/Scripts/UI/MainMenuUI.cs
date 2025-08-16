using UnityEngine;
using UnityEngine.UI;
using ExtremeRacing.Managers;
using ExtremeRacing.Multiplayer;

namespace ExtremeRacing.UI
{
	public class MainMenuUI : MonoBehaviour
	{
		public Button btnHost;
		public Button btnJoin;
		public Button btnGorski;
		public Button btnPustynny;
		public Button btnMiasto;
		public Button btnPort;
		public Button btnTor;
		public SimpleLobby lobby;

		private void Start()
		{
			if (btnHost != null) btnHost.onClick.AddListener(() => lobby?.Host());
			if (btnJoin != null) btnJoin.onClick.AddListener(() => lobby?.Join());
			if (btnGorski != null) btnGorski.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.SceneGorski));
			if (btnPustynny != null) btnPustynny.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.ScenePustynny));
			if (btnMiasto != null) btnMiasto.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.SceneMiasto));
			if (btnPort != null) btnPort.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.ScenePort));
			if (btnTor != null) btnTor.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.SceneTor));
		}
	}
}