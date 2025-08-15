using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI głównego menu: przyciski trybów i załadowanie regionów.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
	[SerializeField] private Button fullModeButton;
	[SerializeField] private Button careerModeButton;
	[SerializeField] private Button sandboxModeButton;

	[SerializeField] private Button gorskiSzczytButton;
	[SerializeField] private Button pustynnyKanionButton;
	[SerializeField] private Button miastoNocyButton;
	[SerializeField] private Button portWyscigowyButton;
	[SerializeField] private Button torMistrzowButton;

	private void Awake()
	{
		if (fullModeButton != null) fullModeButton.onClick.AddListener(() => SelectMode("Full"));
		if (careerModeButton != null) careerModeButton.onClick.AddListener(() => SelectMode("Career"));
		if (sandboxModeButton != null) sandboxModeButton.onClick.AddListener(() => SelectMode("Sandbox"));

		if (gorskiSzczytButton != null) gorskiSzczytButton.onClick.AddListener(() => LoadRegion(SceneNames.RegionGorskiSzczyt));
		if (pustynnyKanionButton != null) pustynnyKanionButton.onClick.AddListener(() => LoadRegion(SceneNames.RegionPustynnyKanion));
		if (miastoNocyButton != null) miastoNocyButton.onClick.AddListener(() => LoadRegion(SceneNames.RegionMiastoNocy));
		if (portWyscigowyButton != null) portWyscigowyButton.onClick.AddListener(() => LoadRegion(SceneNames.RegionPortWyscigowy));
		if (torMistrzowButton != null) torMistrzowButton.onClick.AddListener(() => LoadRegion(SceneNames.RegionTorMistrzow));
	}

	private void SelectMode(string mode)
	{
		// Tutaj można zapisać wybór trybu w PlayerPrefs lub GameManager
		PlayerPrefs.SetString("SelectedMode", mode);
	}

	private void LoadRegion(string sceneName)
	{
		GameManager.Instance?.StartGameInRegion(sceneName);
	}
}