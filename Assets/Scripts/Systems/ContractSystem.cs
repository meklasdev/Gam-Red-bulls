using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System kontraktów i popularności.
/// </summary>
public class ContractSystem : MonoBehaviour
{
	[Serializable]
	public class ContractDefinition
	{
		public string id;
		public string sponsorName;
		public int requiredPopularity;
		public int rewardCredits;
		[TextArea] public string description;

		[NonSerialized] public bool unlocked;
	}

	[SerializeField] private int startingPopularity = 0;
	[SerializeField] private List<ContractDefinition> allContracts = new List<ContractDefinition>();

	public int Popularity { get; private set; }

	public delegate void PopularityChangedHandler(int newPopularity);
	public event PopularityChangedHandler OnPopularityChanged;

	public delegate void ContractUnlockedHandler(ContractDefinition contract);
	public event ContractUnlockedHandler OnContractUnlocked;

	public void InitializeContracts()
	{
		Popularity = startingPopularity;
		EvaluateUnlocks();
	}

	public void AddPopularity(int amount)
	{
		Popularity = Mathf.Max(0, Popularity + amount);
		OnPopularityChanged?.Invoke(Popularity);
		EvaluateUnlocks();
	}

	private void EvaluateUnlocks()
	{
		foreach (var c in allContracts)
		{
			if (!c.unlocked && Popularity >= c.requiredPopularity)
			{
				c.unlocked = true;
				OnContractUnlocked?.Invoke(c);
			}
		}
	}
}