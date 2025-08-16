using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ExtremeRacing.Multiplayer
{
	public class NetworkMatchManager : NetworkBehaviour
	{
		public enum MatchState { Idle, Countdown, Running, Finished }
		public NetworkVariable<int> Countdown = new NetworkVariable<int>(0);
		public NetworkVariable<int> ScoresTotal = new NetworkVariable<int>(0);
		public MatchState State { get; private set; } = MatchState.Idle;

		private readonly Dictionary<ulong, int> _scores = new Dictionary<ulong, int>();

		[ServerRpc(RequireOwnership=false)]
		public void StartMatchServerRpc()
		{
			if (!IsServer) return;
			StartCoroutine(CountdownRoutine());
		}

		private System.Collections.IEnumerator CountdownRoutine()
		{
			State = MatchState.Countdown;
			for (int i = 3; i >= 0; i--)
			{
				Countdown.Value = i;
				yield return new WaitForSeconds(1f);
			}
			State = MatchState.Running;
		}

		[ServerRpc(RequireOwnership=false)]
		public void AddScoreServerRpc(ulong clientId, int delta)
		{
			if (!_scores.ContainsKey(clientId)) _scores[clientId] = 0;
			_scores[clientId] += delta;
			ScoresTotal.Value = 0;
			foreach (var kv in _scores) ScoresTotal.Value += kv.Value;
		}
	}
}