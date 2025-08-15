#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ExternalAssetAutoSetup
{
	[MenuItem("Tools/Auto Setup External Assets")]
	public static void AutoSetup()
	{
		string modelsDir = "Assets/ExternalAssets/Models";
		string prefabsDir = "Assets/Prefabs/Auto";
		string materialsDir = "Assets/Materials/Auto";
		Directory.CreateDirectory(prefabsDir);
		Directory.CreateDirectory(materialsDir);

		Material defaultMat = EnsureMaterial(materialsDir + "/URP_Default.mat", new Color(0.8f, 0.1f, 0.1f));
		Material defaultMetal = EnsureMaterial(materialsDir + "/URP_Metal.mat", new Color(0.6f, 0.6f, 0.7f));

		string[] fbxGuids = AssetDatabase.FindAssets("t:model", new[] { modelsDir });
		foreach (string guid in fbxGuids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".obj", System.StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".glb", System.StringComparison.OrdinalIgnoreCase))
				continue;

			GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (model == null) continue;

			GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
			if (instance == null) continue;
			instance.name = Path.GetFileNameWithoutExtension(path);

			// Assign default URP materials if missing
			foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
			{
				var mats = renderer.sharedMaterials;
				for (int i = 0; i < mats.Length; i++)
				{
					if (mats[i] == null) mats[i] = defaultMat;
				}
				renderer.sharedMaterials = mats;
			}

			SetupByName(instance);

			string prefabPath = Path.Combine(prefabsDir, instance.name + ".prefab");
			PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
			Object.DestroyImmediate(instance);
		}

		EditorSceneManager.MarkAllScenesDirty();
		AssetDatabase.SaveAssets();
		Debug.Log("External assets auto-setup complete.");
	}

	private static Material EnsureMaterial(string path, Color color)
	{
		var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
		if (mat == null)
		{
			mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
			mat.SetColor("_BaseColor", color);
			AssetDatabase.CreateAsset(mat, path);
		}
		return mat;
	}

	private static void SetupByName(GameObject go)
	{
		string n = go.name.ToLowerInvariant();
		if (n.Contains("drone")) { SetupDrone(go); return; }
		if (n.Contains("bmx") || n.Contains("bicycle") || n.Contains("bike")) { SetupBicycle(go); return; }
		if (n.Contains("car") || n.Contains("vehicle") || n.Contains("auto")) { SetupCar(go); return; }
		if (n.Contains("character") || n.Contains("humanoid") || n.Contains("person")) { SetupCharacter(go); return; }
		// Default: try car
		SetupCar(go);
	}

	private static void EnsureMeshColliderOrBox(GameObject go, bool convex)
	{
		var mesh = go.GetComponentInChildren<MeshFilter>();
		if (mesh != null)
		{
			var mc = go.GetComponentInChildren<MeshCollider>();
			if (mc == null) mc = mesh.gameObject.AddComponent<MeshCollider>();
			mc.sharedMesh = mesh.sharedMesh;
			mc.convex = convex;
		}
		else
		{
			var bc = go.AddComponent<BoxCollider>(); bc.isTrigger = false;
		}
	}

	private static void SetupCar(GameObject go)
	{
		if (go.GetComponent<Rigidbody>() == null) go.AddComponent<Rigidbody>();
		EnsureMeshColliderOrBox(go, true);
		if (go.GetComponent<VehicleController>() == null) go.AddComponent<VehicleController>();
		if (go.GetComponent<FuelSystem>() == null) go.AddComponent<FuelSystem>();
		if (go.GetComponent<TyreWearSystem>() == null) go.AddComponent<TyreWearSystem>();
		if (go.GetComponent<CarPainter>() == null) go.AddComponent<CarPainter>();
	}

	private static void SetupBicycle(GameObject go)
	{
		if (go.GetComponent<Rigidbody>() == null) go.AddComponent<Rigidbody>();
		EnsureMeshColliderOrBox(go, true);
		if (go.GetComponent<BicycleController>() == null) go.AddComponent<BicycleController>();
		if (go.GetComponent<StuntManager>() == null) go.AddComponent<StuntManager>();
	}

	private static void SetupDrone(GameObject go)
	{
		var rb = go.GetComponent<Rigidbody>();
		if (rb == null) rb = go.AddComponent<Rigidbody>();
		rb.useGravity = false;
		EnsureMeshColliderOrBox(go, true);
		if (go.GetComponent<DroneController>() == null) go.AddComponent<DroneController>();
	}

	private static void SetupCharacter(GameObject go)
	{
		if (go.GetComponent<CharacterController>() == null) go.AddComponent<CharacterController>();
		if (go.GetComponent<SimpleCharacterController>() == null) go.AddComponent<SimpleCharacterController>();
	}
}
#endif