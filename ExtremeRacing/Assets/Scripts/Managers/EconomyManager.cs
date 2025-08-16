using UnityEngine;

namespace ExtremeRacing.Managers
{
	public class EconomyManager : MonoBehaviour
	{
		public static EconomyManager Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
			if (SaveSystem.Data == null) SaveSystem.Load();
		}

		public int GetBalance() => SaveSystem.Data.currency;

		public bool TrySpend(int amount)
		{
			if (SaveSystem.Data.currency < amount) return false;
			SaveSystem.Data.currency -= amount;
			SaveSystem.Save();
			return true;
		}

		public void Add(int amount)
		{
			SaveSystem.Data.currency += amount;
			SaveSystem.Save();
		}
	}
}