#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.Editor
{
	public static class PrototypePrefabsCreator
	{
		[MenuItem("ExtremeRacing/Prefabs/Create Prototypes")]
		public static void CreatePrototypes()
		{
			CreateVehiclePrefab("Car_Prototype");
			CreateVehiclePrefab("Motocross_Prototype", isBike:true);
			CreateNPCPrefab();
			CreateCratePrefab();
		}

		private static void CreateVehiclePrefab(string name, bool isBike = false)
		{
			string path = $"Assets/Prefabs/{name}.prefab";
			if (System.IO.File.Exists(path)) return;
			var go = new GameObject(name);
			var rb = go.AddComponent<Rigidbody>();
			rb.mass = isBike ? 150f : 1200f;
			var vc = go.AddComponent<VehicleController>();
			vc.controlType = isBike ? VehicleType.Bike : VehicleType.Supercar;
			if (!isBike)
			{
				var fl = new GameObject("WheelFL").AddComponent<WheelCollider>();
				var fr = new GameObject("WheelFR").AddComponent<WheelCollider>();
				var rl = new GameObject("WheelRL").AddComponent<WheelCollider>();
				var rr = new GameObject("WheelRR").AddComponent<WheelCollider>();
				fl.transform.SetParent(go.transform); fl.transform.localPosition = new Vector3(-0.8f, -0.2f, 1.2f);
				fr.transform.SetParent(go.transform); fr.transform.localPosition = new Vector3(0.8f, -0.2f, 1.2f);
				rl.transform.SetParent(go.transform); rl.transform.localPosition = new Vector3(-0.8f, -0.2f, -1.2f);
				rr.transform.SetParent(go.transform); rr.transform.localPosition = new Vector3(0.8f, -0.2f, -1.2f);
				vc.wheelFL = fl; vc.wheelFR = fr; vc.wheelRL = rl; vc.wheelRR = rr;
			}
			go.AddComponent<NetworkObject>();
			PrefabUtility.SaveAsPrefabAsset(go, path);
			Object.DestroyImmediate(go);
		}

		private static void CreateNPCPrefab()
		{
			string path = "Assets/Prefabs/NPCDriver.prefab";
			if (System.IO.File.Exists(path)) return;
			var go = new GameObject("NPCDriver");
			go.AddComponent<Rigidbody>();
			go.AddComponent<VehicleController>();
			go.AddComponent<ExtremeRacing.NPC.NPCDriver>();
			PrefabUtility.SaveAsPrefabAsset(go, path);
			Object.DestroyImmediate(go);
		}

		private static void CreateCratePrefab()
		{
			string path = "Assets/Prefabs/LootCrate.prefab";
			if (System.IO.File.Exists(path)) return;
			var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = "LootCrate";
			go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			go.AddComponent<Rigidbody>();
			PrefabUtility.SaveAsPrefabAsset(go, path);
			Object.DestroyImmediate(go);
		}
	}
}
#endif