#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ExtremeRacing.Editor
{
	public static class SceneWiringTool
	{
		[MenuItem("ExtremeRacing/Scenes/Wire Player/HUD/Minimap")]
		public static void Wire()
		{
			var hud = Object.FindObjectOfType<ExtremeRacing.UI.HUDController>();
			var minimap = Object.FindObjectOfType<ExtremeRacing.UI.MinimapController>();
			var spawner = Object.FindObjectOfType<ExtremeRacing.Managers.PlayerSpawnManager>();
			if (!spawner)
			{
				var go = new GameObject("PlayerSpawnManager");
				spawner = go.AddComponent<ExtremeRacing.Managers.PlayerSpawnManager>();
			}
			spawner.hud = hud;
			spawner.minimap = minimap;
			EditorUtility.SetDirty(spawner);
			Debug.Log("Scene wiring complete.");
		}
	}
}
#endif