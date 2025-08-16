using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExtremeRacing.Managers;

namespace ExtremeRacing.UI
{
	public class ShopUI : MonoBehaviour
	{
		public TextMeshProUGUI balanceText;
		public Button buySupercarBtn;
		public Button buyF1Btn;

		private void Start()
		{
			Refresh();
			if (buySupercarBtn) buySupercarBtn.onClick.AddListener(() => TryBuy(800, "Supercar_Basic"));
			if (buyF1Btn) buyF1Btn.onClick.AddListener(() => TryBuy(1200, "F1_Basic"));
		}

		private void TryBuy(int cost, string vehicleId)
		{
			if (EconomyManager.Instance.TrySpend(cost))
			{
				Gameplay.ProgressionSystem.Instance.UnlockVehicle(vehicleId);
				Refresh();
			}
		}

		private void Refresh()
		{
			if (balanceText) balanceText.text = $"$ {EconomyManager.Instance.GetBalance()}";
		}
	}
}