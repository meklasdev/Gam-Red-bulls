#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace ExtremeRacing.Editor
{
	public static class RegionAutoBuilder
	{
		[MenuItem("ExtremeRacing/Scenes/Finalize All Regions (RC)")]
		public static void FinalizeAll()
		{
			string[] names = {
				"Region_GorskiSzczyt",
				"Region_PustynnyKanion",
				"Region_MiastoNocy",
				"Region_PortWyscigowy",
				"Region_TorMistrzow",
				"Region_ArenaEventowa"
			};
			foreach (var n in names) FinalizeRegion(n);
			Debug.Log("Regions finalized (checkpoints, rivals, cutscenes). You can tweak per scene.");
		}

		private static void FinalizeRegion(string sceneName)
		{
			string path = $"Assets/Scenes/{sceneName}.unity";
			if (!File.Exists(path)) return;
			var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

			// Root containers
			var root = new GameObject("RegionAutoRoot");
			var checkpointsRoot = new GameObject("Checkpoints");
			checkpointsRoot.transform.SetParent(root.transform);
			var wpRoot = new GameObject("Waypoints");
			wpRoot.transform.SetParent(root.transform);

			// Create circular path
			int gates = 24;
			float radius = 400f;
			var lmGO = new GameObject("LapManager");
			var lap = lmGO.AddComponent<ExtremeRacing.Gameplay.LapManager>();
			for (int i = 0; i < gates; i++)
			{
				float t = (i / (float)gates) * Mathf.PI * 2f;
				Vector3 p = new Vector3(Mathf.Cos(t) * radius, 0f, Mathf.Sin(t) * radius);
				var cp = new GameObject($"Checkpoint_{i}");
				cp.transform.SetParent(checkpointsRoot.transform);
				cp.transform.position = p;
				var gate = cp.AddComponent<ExtremeRacing.Gameplay.Checkpoint>();
				var trig = cp.AddComponent<SphereCollider>();
				trig.isTrigger = true;
				var gateScript = cp.AddComponent<ExtremeRacing.Gameplay.CheckpointGate>();
				gateScript.manager = lap;
				gateScript.index = i;
				lap.checkpoints.Add(cp.transform);
			}

			// Waypoint path (reuse checkpoints as waypoints)
			var pathGO = new GameObject("WaypointPath");
			pathGO.transform.SetParent(wpRoot.transform);
			var path = pathGO.AddComponent<ExtremeRacing.Rivals.WaypointPath>();
			path.waypoints = new Transform[gates];
			for (int i = 0; i < gates; i++) path.waypoints[i] = checkpointsRoot.transform.GetChild(i);

			// Rival spawner
			var spGO = new GameObject("RivalSpawner");
			spGO.transform.SetParent(root.transform);
			var sp = spGO.AddComponent<ExtremeRacing.Rivals.RivalSpawner>();
			sp.path = path;
			sp.count = 6;
			sp.spacing = 10f;

			// RegionSetup
			var rsGO = new GameObject("RegionSetup");
			var setup = rsGO.AddComponent<ExtremeRacing.Regions.RegionSetup>();
			setup.rivalSpawner = sp;
			setup.lapManager = lap;

			// Cutscene assets
			var csDir = "Assets/ScriptableObjects/Cutscenes";
			if (!Directory.Exists(csDir)) Directory.CreateDirectory(csDir);
			var startAsset = ScriptableObject.CreateInstance<ExtremeRacing.Cutscenes.CutsceneAsset>();
			startAsset.shots = new ExtremeRacing.Cutscenes.CutsceneShot[]
			{
				new ExtremeRacing.Cutscenes.CutsceneShot{ position = new Vector3(0,30,-radius*0.5f), eulerAngles=new Vector3(45,0,0), duration=2f, subtitle=$"{sceneName} start"}
			};
			AssetDatabase.CreateAsset(startAsset, $"{csDir}/{sceneName}_start.asset");
			var finishAsset = ScriptableObject.CreateInstance<ExtremeRacing.Cutscenes.CutsceneAsset>();
			finishAsset.shots = new ExtremeRacing.Cutscenes.CutsceneShot[]
			{
				new ExtremeRacing.Cutscenes.CutsceneShot{ position = new Vector3(0,30, radius*0.5f), eulerAngles=new Vector3(45,180,0), duration=2f, subtitle=$"{sceneName} finish"}
			};
			AssetDatabase.CreateAsset(finishAsset, $"{csDir}/{sceneName}_finish.asset");

			// Cutscene player and triggers
			var playerGO = new GameObject("CutscenePlayer");
			var player = playerGO.AddComponent<ExtremeRacing.Cutscenes.CutscenePlayer>();
			var startTrig = new GameObject("CutsceneTrigger_Start");
			startTrig.transform.position = new Vector3(0,0,-radius*0.49f);
			var colS = startTrig.AddComponent<SphereCollider>(); colS.isTrigger = true;
			var ctS = startTrig.AddComponent<ExtremeRacing.Cutscenes.CutsceneTrigger>();
			ctS.asset = startAsset; ctS.player = player;
			var endTrig = new GameObject("CutsceneTrigger_Finish");
			endTrig.transform.position = new Vector3(0,0, radius*0.49f);
			var colE = endTrig.AddComponent<SphereCollider>(); colE.isTrigger = true;
			var ctE = endTrig.AddComponent<ExtremeRacing.Cutscenes.CutsceneTrigger>();
			ctE.asset = finishAsset; ctE.player = player;

			EditorSceneManager.SaveScene(scene);
		}
	}
}
#endif