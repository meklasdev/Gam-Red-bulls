using System.Collections;
using UnityEngine;

namespace ExtremeRacing.Managers
{
	public class AssetBundleLoader : MonoBehaviour
	{
		public IEnumerator LoadBundle(string bundleName)
		{
			string path = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles", bundleName);
			var req = AssetBundle.LoadFromFileAsync(path);
			yield return req;
			if (req.assetBundle == null)
			{
				Debug.LogError($"Failed to load AssetBundle: {bundleName}");
			}
			else
			{
				Debug.Log($"AssetBundle loaded: {bundleName}");
			}
		}
	}
}