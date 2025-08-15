using UnityEngine;

/// <summary>
/// Prosty system zakładów: obstawianie wyniku wyścigu względem AI.
/// </summary>
public class BettingSystem : MonoBehaviour
{
	[SerializeField] private int playerCredits = 0;
	[SerializeField] private float odds = 1.5f; // kurs
	[SerializeField] private int currentBetAmount = 0;
	[SerializeField] private bool betOnPlayerWin = true;

	public void PlaceBet(int amount, bool onPlayer)
	{
		if (amount <= 0 || amount > playerCredits) return;
		currentBetAmount = amount;
		betOnPlayerWin = onPlayer;
		playerCredits -= amount;
	}

	public void ResolveBet(bool playerWon)
	{
		if (currentBetAmount <= 0) return;
		bool success = (playerWon && betOnPlayerWin) || (!playerWon && !betOnPlayerWin);
		if (success)
		{
			int payout = Mathf.RoundToInt(currentBetAmount * odds);
			playerCredits += payout;
		}
		currentBetAmount = 0;
	}

	public int GetCredits() => playerCredits;
	public void AddCredits(int amount) => playerCredits += Mathf.Max(0, amount);
}