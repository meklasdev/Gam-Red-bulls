using UnityEngine;
using TMPro;
using ExtremeRacing.Gameplay;

namespace ExtremeRacing.UI
{
	public class ContractsUI : MonoBehaviour
	{
		public ContractSystem contractSystem;
		public TextMeshProUGUI listText;

		private void OnEnable()
		{
			Refresh();
		}

		public void Refresh()
		{
			if (contractSystem == null || listText == null) return;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (var c in contractSystem.Contracts)
			{
				sb.AppendLine($"[{(c.active ? "Active" : "Done")}] {c.client}: {c.objective} (payout {c.payout})");
			}
			listText.text = sb.ToString();
		}
	}
}