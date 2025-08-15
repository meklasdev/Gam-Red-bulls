using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sklep: zakup pojazdów za kredyty.
/// </summary>
public class ShopUI : MonoBehaviour
{
	[SerializeField] private Transform listRoot;
	[SerializeField] private Button itemTemplate;
	[SerializeField] private VehicleDefinition[] catalog;
	[SerializeField] private Text creditsText;

	private void OnEnable()
	{
		Refresh();
	}

	public void Refresh()
	{
		creditsText.text = $"Kredyty: {ProfileManager.Instance.Profile.credits}";
		foreach (Transform c in listRoot) Destroy(c.gameObject);
		foreach (var def in catalog)
		{
			var btn = Instantiate(itemTemplate, listRoot);
			btn.gameObject.SetActive(true);
			btn.GetComponentInChildren<Text>().text = $"{def.displayName} – {def.cost}";
			bool owned = ProfileManager.Instance.Profile.unlockedVehicleIds.Contains(def.vehicleId);
			btn.interactable = !owned && ProfileManager.Instance.Profile.credits >= def.cost;
			btn.onClick.AddListener(() => Buy(def));
		}
	}

	private void Buy(VehicleDefinition def)
	{
		var profile = ProfileManager.Instance.Profile;
		if (profile.credits < def.cost) return;
		profile.credits -= def.cost;
		if (!profile.unlockedVehicleIds.Contains(def.vehicleId)) profile.unlockedVehicleIds.Add(def.vehicleId);
		ProfileManager.Instance.Save();
		Refresh();
	}
}