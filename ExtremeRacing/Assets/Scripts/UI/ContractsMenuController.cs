using UnityEngine;
using UnityEngine.UI;
using ExtremeRacing.Gameplay;

namespace ExtremeRacing.UI
{
	public class ContractsMenuController : MonoBehaviour
	{
		public ContractSystem contractSystem;
		public ContractsUI contractsUI;
		public Button completeFirstActiveBtn;

		private void Start()
		{
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
							break;
						}
					}
					contractsUI.Refresh();
				});
			}
		}
	}
}