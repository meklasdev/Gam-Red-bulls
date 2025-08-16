using System.Linq;
using UnityEngine;
using TMPro;
using ExtremeRacing.Multiplayer;

namespace ExtremeRacing.UI
{
	public class ScoreboardUI : MonoBehaviour
	{
		public SimpleLobby lobby;
		public TextMeshProUGUI scoreboardText;
		public float refreshInterval = 0.5f;
		private float _timer;

		private void Update()
		{
			_timer += Time.deltaTime;
			if (_timer >= refreshInterval)
			{
				_timer = 0f;
				if (lobby != null && scoreboardText != null)
				{
					var lines = lobby.Scores.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key}: {kv.Value}");
					scoreboardText.text = string.Join("\n", lines);
				}
			}
		}
	}
}