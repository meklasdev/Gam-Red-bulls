#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PrimitiveAssetBuilder
{
	public static string AutoDir => "Assets/Prefabs/Auto";

	[MenuItem("Tools/Create Primitive Prefabs")]
	public static void CreateAll()
	{
		Directory.CreateDirectory(AutoDir);
		var matBody = EnsureLitMat("Assets/Materials/Auto/Primitive_Body.mat", new Color(0.9f, 0.1f, 0.1f));
		var matWheel = EnsureLitMat("Assets/Materials/Auto/Primitive_Wheel.mat", new Color(0.08f, 0.08f, 0.08f));
		CreateCar(matBody, matWheel);
		CreateBmx(matBody, matWheel);
		CreateDrone(matBody, matWheel);
		CreateCharacter();
		AssetDatabase.SaveAssets();
		Debug.Log("Primitive prefabs created.");
	}

	private static Material EnsureLitMat(string path, Color color)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		var m = AssetDatabase.LoadAssetAtPath<Material>(path);
		if (m == null)
		{
			m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
			m.SetColor("_BaseColor", color);
			AssetDatabase.CreateAsset(m, path);
		}
		return m;
	}

	private static void CreateCar(Material body, Material wheel)
	{
		string path = Path.Combine(AutoDir, "Car_Primitive.prefab");
		if (File.Exists(path)) return;
		var root = new GameObject("Car_Primitive");
		var bodyGo = GameObject.CreatePrimitive(PrimitiveType.Cube); bodyGo.name = "Body"; bodyGo.transform.SetParent(root.transform);
		bodyGo.transform.localScale = new Vector3(2.0f, 0.5f, 4.0f); bodyGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
		bodyGo.GetComponent<MeshRenderer>().sharedMaterial = body;
		CreateWheel(root.transform, wheel, new Vector3(-0.9f, 0.4f, 1.6f));
		CreateWheel(root.transform, wheel, new Vector3(0.9f, 0.4f, 1.6f));
		CreateWheel(root.transform, wheel, new Vector3(-0.9f, 0.4f, -1.6f));
		CreateWheel(root.transform, wheel, new Vector3(0.9f, 0.4f, -1.6f));
		root.AddComponent<Rigidbody>();
		root.AddComponent<VehicleController>();
		root.AddComponent<CarPainter>();
		PrefabUtility.SaveAsPrefabAsset(root, path);
		Object.DestroyImmediate(root);
	}

	private static void CreateWheel(Transform parent, Material mat, Vector3 localPos)
	{
		var w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		w.name = "Wheel"; w.transform.SetParent(parent);
		w.transform.localScale = new Vector3(0.7f, 0.15f, 0.7f); w.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
		w.transform.localPosition = localPos;
		w.GetComponent<MeshRenderer>().sharedMaterial = mat;
	}

	private static void CreateBmx(Material frame, Material wheel)
	{
		string path = Path.Combine(AutoDir, "BMX_Primitive.prefab");
		if (File.Exists(path)) return;
		var root = new GameObject("BMX_Primitive");
		var frameGo = GameObject.CreatePrimitive(PrimitiveType.Capsule); frameGo.name = "Frame"; frameGo.transform.SetParent(root.transform);
		frameGo.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f); frameGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
		frameGo.GetComponent<MeshRenderer>().sharedMaterial = frame;
		CreateBikeWheel(root.transform, wheel, new Vector3(-0.9f, 0.5f, 0f));
		CreateBikeWheel(root.transform, wheel, new Vector3(0.9f, 0.5f, 0f));
		root.AddComponent<Rigidbody>();
		root.AddComponent<BicycleController>();
		PrefabUtility.SaveAsPrefabAsset(root, path);
		Object.DestroyImmediate(root);
	}

	private static void CreateBikeWheel(Transform parent, Material mat, Vector3 localPos)
	{
		var w = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		w.name = "Wheel"; w.transform.SetParent(parent);
		w.transform.localScale = new Vector3(0.9f, 0.05f, 0.9f); w.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
		w.transform.localPosition = localPos;
		w.GetComponent<MeshRenderer>().sharedMaterial = mat;
	}

	private static void CreateDrone(Material body, Material rotor)
	{
		string path = Path.Combine(AutoDir, "Drone_Primitive.prefab");
		if (File.Exists(path)) return;
		var root = new GameObject("Drone_Primitive");
		var core = GameObject.CreatePrimitive(PrimitiveType.Sphere); core.name = "Core"; core.transform.SetParent(root.transform);
		core.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f); core.transform.localPosition = new Vector3(0f, 0.6f, 0f);
		core.GetComponent<MeshRenderer>().sharedMaterial = body;
		CreateRotor(root.transform, rotor, new Vector3(0.8f, 0.7f, 0.8f));
		CreateRotor(root.transform, rotor, new Vector3(-0.8f, 0.7f, 0.8f));
		CreateRotor(root.transform, rotor, new Vector3(0.8f, 0.7f, -0.8f));
		CreateRotor(root.transform, rotor, new Vector3(-0.8f, 0.7f, -0.8f));
		var rb = root.AddComponent<Rigidbody>(); rb.useGravity = false;
		root.AddComponent<DroneController>();
		PrefabUtility.SaveAsPrefabAsset(root, path);
		Object.DestroyImmediate(root);
	}

	private static void CreateRotor(Transform parent, Material mat, Vector3 localPos)
	{
		var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder); arm.name = "Arm"; arm.transform.SetParent(parent);
		arm.transform.localScale = new Vector3(0.08f, 0.6f, 0.08f); arm.transform.localEulerAngles = new Vector3(0f, 0f, 45f);
		arm.transform.localPosition = (localPos + Vector3.zero) * 0.7f;
		arm.GetComponent<MeshRenderer>().sharedMaterial = mat;
		var rotor = GameObject.CreatePrimitive(PrimitiveType.Cylinder); rotor.name = "Rotor"; rotor.transform.SetParent(parent);
		rotor.transform.localScale = new Vector3(0.5f, 0.03f, 0.5f); rotor.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		rotor.transform.localPosition = localPos;
		rotor.GetComponent<MeshRenderer>().sharedMaterial = mat;
	}

	private static void CreateCharacter()
	{
		string path = Path.Combine(AutoDir, "Character_Primitive.prefab");
		if (File.Exists(path)) return;
		var root = new GameObject("Character_Primitive");
		var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name = "Body"; body.transform.SetParent(root.transform);
		var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); head.name = "Head"; head.transform.SetParent(root.transform);
		head.transform.localPosition = new Vector3(0f, 1.4f, 0f);
		root.AddComponent<CharacterController>();
		root.AddComponent<SimpleCharacterController>();
		PrefabUtility.SaveAsPrefabAsset(root, path);
		Object.DestroyImmediate(root);
	}
}
#endif