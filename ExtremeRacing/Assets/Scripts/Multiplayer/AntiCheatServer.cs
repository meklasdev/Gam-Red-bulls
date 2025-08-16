using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.Multiplayer
{
	public class AntiCheatServer : NetworkBehaviour
	{
		class Track
		{
			public Vector3 lastPos;
			public float lastTime;
			public float strikes;
		}

		private readonly Dictionary<ulong, Track> _tracks = new Dictionary<ulong, Track>();
		public float maxSpeedMultiplier = 1.2f;
		public float maxTeleportMeters = 30f;
		public float strikeLimit = 3f;

		private void Update()
		{
			if (!IsServer) return;
			foreach (var no in FindObjectsOfType<NetworkObject>())
			{
				if (!no.IsSpawned || !no.IsOwner) continue;
				var vc = no.GetComponent<VehicleController>();
				if (vc == null || vc.spec == null) continue;
				var id = no.OwnerClientId;
				if (!_tracks.TryGetValue(id, out var t))
				{
					t = new Track { lastPos = no.transform.position, lastTime = Time.time, strikes = 0f };
					_tracks[id] = t;
					continue;
				}
				float dt = Mathf.Max(0.001f, Time.time - t.lastTime);
				float dist = Vector3.Distance(no.transform.position, t.lastPos);
				float speed = (dist / dt) * 3.6f;
				if (dist > maxTeleportMeters || speed > vc.spec.maxSpeedKmh * maxSpeedMultiplier)
				{
					t.strikes += 1f;
					if (t.strikes >= strikeLimit)
					{
						KickClient(id);
					}
				}
				t.lastPos = no.transform.position;
				t.lastTime = Time.time;
			}
		}

		private void KickClient(ulong clientId)
		{
			Debug.LogWarning($"AntiCheat: Kicking client {clientId}");
			NetworkManager.DisconnectClient(clientId);
		}
	}
}