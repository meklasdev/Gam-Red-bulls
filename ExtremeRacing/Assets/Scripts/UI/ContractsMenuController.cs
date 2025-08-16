using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExtremeRacing.Gameplay;
using ExtremeRacing.Managers;

namespace ExtremeRacing.UI
{
	public class ContractsMenuController : MonoBehaviour
	{
		public ContractSystem contractSystem;
		public ContractsUI contractsUI;
		public Button completeFirstActiveBtn;
		public TextMeshProUGUI balanceText;

		private void Start()
		{
			RefreshBalance();
			if (completeFirstActiveBtn)
			{
				completeFirstActiveBtn.onClick.AddListener(() =>
				{
					var list = contractSystem.Contracts;
					foreach (var c in list)
					{
						if (c.active)
						{
							contractSystem.CompleteContract(c.id);
							EconomyManager.Instance.Add(c.payout);
							break;
						}
					}
					contractsUI.Refresh();
					RefreshBalance();
				});
			}
		}

		private void RefreshBalance()
		{
			if (balanceText) balanceText.text = $"$ {EconomyManager.Instance.GetBalance()}";
		}
	}
}