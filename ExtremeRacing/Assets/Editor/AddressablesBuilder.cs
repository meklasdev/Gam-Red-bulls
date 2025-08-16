#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

namespace ExtremeRacing.Editor
{
	public static class AddressablesBuilder
	{
		[MenuItem("ExtremeRacing/Addressables/Build Content")]
		public static void BuildContent()
		{
			var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
			AddressableAssetSettings.BuildPlayerContent();
			UnityEngine.Debug.Log("Addressables content built.");
		}
	}
}
#endif