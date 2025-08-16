using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace ExtremeRacing.Addressables
{
	public class RegionLoader : MonoBehaviour
	{
		public async Task LoadRegionAsync(string address)
		{
			AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Single);
			await handle.Task;
			if (handle.Status != AsyncOperationStatus.Succeeded)
			{
				Debug.LogError($"Failed to load region: {address}");
			}
		}
	}
}