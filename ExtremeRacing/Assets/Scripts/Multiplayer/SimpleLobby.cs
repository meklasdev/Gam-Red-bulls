using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ExtremeRacing.Multiplayer
{
	public class SimpleLobby : MonoBehaviour
	{
		private readonly Dictionary<ulong, int> _playerScores = new Dictionary<ulong, int>();

		public void Host()
		{
			if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
			{
				NetworkManager.Singleton.StartHost();
				RegisterCallbacks();
			}
		}

		public void Join()
		{
			if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
			{
				NetworkManager.Singleton.StartClient();
				RegisterCallbacks();
			}
		}

		private void RegisterCallbacks()
		{
			NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
		}

		private void OnClientConnected(ulong clientId)
		{
			_playerScores[clientId] = 0;
			Debug.Log($"Client connected {clientId}");
		}

		private void OnClientDisconnected(ulong clientId)
		{
			_playerScores.Remove(clientId);
			Debug.Log($"Client disconnected {clientId}");
		}

		public void AddScore(ulong clientId, int score)
		{
			if (_playerScores.ContainsKey(clientId))
			{
				_playerScores[clientId] += score;
			}
		}

		public IReadOnlyDictionary<ulong, int> Scores => _playerScores;
	}
}