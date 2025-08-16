using UnityEngine;
using UnityEngine.UI;

namespace ExtremeRacing.UI
{
	public class ShopUI : MonoBehaviour
	{
		public Text balanceText;
		public Button buySupercarBtn;
		public Button buyF1Btn;
		private int _balance = 1000;

		private void Start()
		{
			Refresh();
			if (buySupercarBtn) buySupercarBtn.onClick.AddListener(() => TryBuy(800));
			if (buyF1Btn) buyF1Btn.onClick.AddListener(() => TryBuy(1200));
		}

		private void TryBuy(int cost)
		{
			if (_balance >= cost)
			{
				_balance -= cost;
				Refresh();
			}
		}

		private void Refresh()
		{
			if (balanceText) balanceText.text = $"$ {_balance}";
		}
	}
}