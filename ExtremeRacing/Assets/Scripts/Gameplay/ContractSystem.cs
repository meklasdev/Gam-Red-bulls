using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRacing.Gameplay
{
	[Serializable]
	public class Contract
	{
		public string id;
		public string client;
		public string objective;
		public int payout;
		public bool active;
	}

	public class ContractSystem : MonoBehaviour
	{
		[SerializeField] private List<Contract> _contracts = new List<Contract>();
		public IReadOnlyList<Contract> Contracts => _contracts;

		private void Start()
		{
			GenerateDailyContracts();
		}

		public void GenerateDailyContracts()
		{
			_contracts.Clear();
			_contracts.Add(new Contract{ id = "cntr_rally_01", client = "Rally Team", objective = "Wygraj rajd w Region_MiastoNocy", payout = 700, active = true });
			_contracts.Add(new Contract{ id = "cntr_drift_01", client = "Drift Club", objective = "Zdobądź 10 000 pkt driftu", payout = 450, active = true });
			_contracts.Add(new Contract{ id = "cntr_f1_01", client = "F1 Crew", objective = "Top 3 na Region_TorMistrzow", payout = 1000, active = true });
		}

		public void CompleteContract(string id)
		{
			var c = _contracts.Find(x => x.id == id && x.active);
			if (c != null)
			{
				c.active = false;
				Debug.Log($"Contract completed: {c.id} payout {c.payout}");
			}
		}
	}
}