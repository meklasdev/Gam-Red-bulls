#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MapAutoBuilder
{
	[MenuItem("Tools/Build Simple Map From Textures")]
	public static void Build()
	{
		var root = new GameObject("GeneratedMap");

		// Create Terrain
		var td = new GameObject("Terrain");
		td.transform.SetParent(root.transform);
		var terrainData = new TerrainData();
		terrainData.heightmapResolution = 513;
		terrainData.alphamapResolution = 512;
		terrainData.baseMapResolution = 512;
		terrainData.size = new Vector3(512, 60, 512);
		var terrain = td.AddComponent<Terrain>();
		var collider = td.AddComponent<TerrainCollider>();
		terrain.terrainData = terrainData; collider.terrainData = terrainData;

		// Load textures and create layers
		string texDir = "Assets/ExternalAssets/Textures";
		string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { texDir });
		var texs = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Select(p => AssetDatabase.LoadAssetAtPath<Texture2D>(p)).Where(t => t != null).ToList();
		var layers = texs.Take(4).Select(t => CreateLayer(t)).ToArray();
		if (layers.Length == 0)
		{
			// Fallback single-color layer
			var clr = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			clr.SetPixels(new[] { new Color(0.2f, 0.6f, 0.2f, 1f), new Color(0.2f, 0.6f, 0.2f, 1f), new Color(0.2f, 0.6f, 0.2f, 1f), new Color(0.2f, 0.6f, 0.2f, 1f) });
			clr.Apply();
			layers = new[] { CreateLayer(clr) };
		}
		terrainData.terrainLayers = layers;

		// Paint first layer across whole map
		float[,,] splat = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, layers.Length];
		for (int y = 0; y < terrainData.alphamapHeight; y++)
		{
			for (int x = 0; x < terrainData.alphamapWidth; x++)
			{
				for (int l = 0; l < layers.Length; l++) splat[x, y, l] = l == 0 ? 1f : 0f;
			}
		}
		terrainData.SetAlphamaps(0, 0, splat);

		// Simple course objects (ramps and gates)
		CreateRamp(root.transform, new Vector3(0, 2, 50), new Vector3(6, 0.5f, 12), 20f);
		CreateRamp(root.transform, new Vector3(30, 2, 120), new Vector3(6, 0.5f, 12), -15f);
		CreateGate(root.transform, new Vector3(-20, 3, 200));

		EditorSceneManager.MarkAllScenesDirty();
		Debug.Log("Built simple map. Assign better textures to Assets/ExternalAssets/Textures for improved look.");
	}

	private static TerrainLayer CreateLayer(Texture2D tex)
	{
		var layer = new TerrainLayer();
		layer.diffuseTexture = tex;
		layer.tileSize = new Vector2(8, 8);
		return layer;
	}

	private static void CreateRamp(Transform parent, Vector3 pos, Vector3 scale, float tiltDeg)
	{
		var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.name = "Ramp";
		go.transform.SetParent(parent);
		go.transform.position = pos;
		go.transform.localScale = scale;
		go.transform.rotation = Quaternion.Euler(tiltDeg, 0f, 0f);
	}

	private static void CreateGate(Transform parent, Vector3 pos)
	{
		var left = GameObject.CreatePrimitive(PrimitiveType.Cube);
		var right = GameObject.CreatePrimitive(PrimitiveType.Cube);
		var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
		left.name = "Gate_L"; right.name = "Gate_R"; top.name = "Gate_T";
		left.transform.SetParent(parent); right.transform.SetParent(parent); top.transform.SetParent(parent);
		left.transform.position = pos + new Vector3(-3, 0, 0);
		right.transform.position = pos + new Vector3(3, 0, 0);
		top.transform.position = pos + new Vector3(0, 2, 0);
		left.transform.localScale = new Vector3(0.3f, 4f, 0.3f);
		right.transform.localScale = new Vector3(0.3f, 4f, 0.3f);
		top.transform.localScale = new Vector3(6.6f, 0.3f, 0.3f);
	}
}
#endif