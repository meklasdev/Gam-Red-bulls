using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI kontraktów – podgląd i odbiór nagród po odblokowaniu.
/// </summary>
public class ContractsUI : MonoBehaviour
{
	[SerializeField] private ContractSystem contractSystem;
	[SerializeField] private Transform listRoot;
	[SerializeField] private Button itemTemplate;
	[SerializeField] private Text popularityText;

	private void OnEnable()
	{
		contractSystem.OnContractUnlocked += _ => Refresh();
		contractSystem.OnPopularityChanged += _ => Refresh();
		Refresh();
	}

	private void OnDisable()
	{
		contractSystem.OnContractUnlocked -= _ => Refresh();
		contractSystem.OnPopularityChanged -= _ => Refresh();
	}

	public void Refresh()
	{
		if (popularityText != null) popularityText.text = $"Popularność: {contractSystem.Popularity}";
		foreach (Transform c in listRoot) Destroy(c.gameObject);
		var field = typeof(ContractSystem).GetField("allContracts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var list = field.GetValue(contractSystem) as System.Collections.IEnumerable;
		foreach (var item in list)
		{
			var def = item as dynamic;
			var btn = Instantiate(itemTemplate, listRoot);
			btn.gameObject.SetActive(true);
			string name = def.sponsorName;
			int reward = def.rewardCredits;
			bool unlocked = def.unlocked;
			btn.GetComponentInChildren<Text>().text = unlocked ? $"{name} – Odbierz {reward}" : $"{name} – Zablokowany";
			btn.interactable = unlocked;
			btn.onClick.AddListener(() => ClaimReward(reward, def));
		}
	}

	private void ClaimReward(int reward, dynamic def)
	{
		ProfileManager.Instance.AddCredits(reward);
		def.unlocked = false; // jednorazowa nagroda
		Refresh();
	}
}