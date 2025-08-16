using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace ExtremeRacing.Networking
{
	public class RelayManager : MonoBehaviour
	{
		public string JoinCode { get; private set; }

		public async Task InitializeAsync()
		{
			if (UnityServices.State == ServicesInitializationState.Initialized) return;
			await UnityServices.InitializeAsync();
			if (!AuthenticationService.Instance.IsSignedIn)
			{
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
			}
		}

		public async Task<string> CreateRelayAsync(int maxConnections = 8)
		{
			await InitializeAsync();
			Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxConnections);
			JoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
			var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
			transport.SetRelayServerData(new RelayServerData(alloc, "dtls"));
			return JoinCode;
		}

		public async Task JoinRelayAsync(string joinCode)
		{
			await InitializeAsync();
			JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
			var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
			transport.SetRelayServerData(new RelayServerData(alloc, "dtls"));
		}
	}
}