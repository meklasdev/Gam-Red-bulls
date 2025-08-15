using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI garażu: wybór i spawn aktywnego pojazdu.
/// </summary>
public class GarageUI : MonoBehaviour
{
	[SerializeField] private Transform listRoot;
	[SerializeField] private Button itemTemplate;
	[SerializeField] private List<VehicleDefinition> allVehicles;
	[SerializeField] private Transform spawnPoint;

	private void Start()
	{
		RefreshList();
	}

	public void RefreshList()
	{
		foreach (Transform c in listRoot) Destroy(c.gameObject);
		foreach (var v in allVehicles)
		{
			var btn = Instantiate(itemTemplate, listRoot);
			btn.gameObject.SetActive(true);
			btn.GetComponentInChildren<Text>().text = v.displayName;
			bool unlocked = ProfileManager.Instance.Profile.unlockedVehicleIds.Contains(v.vehicleId);
			btn.onClick.AddListener(() => OnSelectVehicle(v, unlocked));
		}
	}

	private void OnSelectVehicle(VehicleDefinition v, bool unlocked)
	{
		if (!unlocked) return;
		ProfileManager.Instance.Profile.activeVehicleId = v.vehicleId;
		ProfileManager.Instance.Save();
		Spawn(v);
	}

	private void Spawn(VehicleDefinition v)
	{
		if (v.prefab == null || spawnPoint == null) return;
		Instantiate(v.prefab, spawnPoint.position, spawnPoint.rotation);
	}
}