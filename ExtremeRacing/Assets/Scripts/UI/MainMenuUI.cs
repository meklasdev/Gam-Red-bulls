using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExtremeRacing.Managers;
using ExtremeRacing.Multiplayer;
using System.Threading.Tasks;

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
		public Button btnArena;
		public SimpleLobby lobby;
		public ExtremeRacing.Networking.RelayManager relay;
		public TextMeshProUGUI joinCodeText;
		public TMP_InputField joinCodeInput;

		private void Start()
		{
			if (btnHost != null) btnHost.onClick.AddListener(async () => await HostWithRelay());
			if (btnJoin != null) btnJoin.onClick.AddListener(async () => await JoinWithRelay());
			if (btnGorski != null) btnGorski.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.SceneGorski));
			if (btnPustynny != null) btnPustynny.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.ScenePustynny));
			if (btnMiasto != null) btnMiasto.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.SceneMiasto));
			if (btnPort != null) btnPort.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.ScenePort));
			if (btnTor != null) btnTor.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.SceneTor));
			if (btnArena != null) btnArena.onClick.AddListener(() => GameManager.Instance.LoadScene(GameManager.SceneArena));
		}

		private async Task HostWithRelay()
		{
			if (relay == null) { lobby?.Host(); return; }
			string code = await relay.CreateRelayAsync();
			if (joinCodeText) joinCodeText.text = code;
			lobby?.Host();
		}

		private async Task JoinWithRelay()
		{
			if (relay == null) { lobby?.Join(); return; }
			if (joinCodeInput && !string.IsNullOrEmpty(joinCodeInput.text))
			{
				await relay.JoinRelayAsync(joinCodeInput.text.Trim());
			}
			lobby?.Join();
		}
	}
}