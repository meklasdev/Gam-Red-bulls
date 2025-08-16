using UnityEngine;
using UnityEngine.UI;

public class ResultsUI : MonoBehaviour
{
	public GameObject root;
	public Text titleText;
	public Text statusText;
	public Text rewardText;

	public void Show(string title, bool success, int reward)
	{
		if (root) root.SetActive(true);
		if (titleText) titleText.text = title;
		if (statusText) statusText.text = success ? "Sukces!" : "Porażka";
		if (rewardText) rewardText.text = $"Nagroda: {reward}";
	}

	public void Hide()
	{
		if (root) root.SetActive(false);
	}
}