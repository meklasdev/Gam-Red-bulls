#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.IO;

namespace ExtremeRacing.Editor
{
	public static class AddressablesSetup
	{
		[MenuItem("ExtremeRacing/Addressables/Setup Default Groups")] 
		public static void Setup()
		{
			var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
			string scenesDir = "Assets/Scenes";
			var group = settings.FindGroup("Regions") ?? settings.CreateGroup("Regions", false, false, false, null);
			foreach (var file in Directory.GetFiles(scenesDir, "*.unity"))
			{
				string guid = AssetDatabase.AssetPathToGUID(file);
				if (string.IsNullOrEmpty(guid)) continue;
				var entry = settings.CreateOrMoveEntry(guid, group, false, false);
				entry.address = Path.GetFileNameWithoutExtension(file);
			}
			settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
			AssetDatabase.SaveAssets();
			Debug.Log("Addressables groups configured for region scenes.");
		}
	}
}
#endif