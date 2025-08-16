using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRacing.Procedural
{
	public class ProceduralTrackGenerator : MonoBehaviour
	{
		public int waypointCount = 200;
		public float step = 10f;
		public float noiseScale = 50f;
		public float width = 6f;
		public Material trackMaterial;
		private MeshFilter _meshFilter;
		private MeshRenderer _meshRenderer;

		private void Awake()
		{
			_meshFilter = gameObject.AddComponent<MeshFilter>();
			_meshRenderer = gameObject.AddComponent<MeshRenderer>();
			if (trackMaterial != null) _meshRenderer.sharedMaterial = trackMaterial;
		}

		[ContextMenu("Generate Track")]
		public void Generate()
		{
			var verts = new List<Vector3>();
			var tris = new List<int>();
			Vector3 dir = Vector3.forward;
			Vector3 pos = Vector3.zero;
			for (int i = 0; i < waypointCount; i++)
			{
				float angle = (Mathf.PerlinNoise(i * 0.1f, 0.5f) - 0.5f) * noiseScale;
				dir = Quaternion.Euler(0, angle, 0) * dir;
				pos += dir.normalized * step;
				Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
				verts.Add(pos - right * width * 0.5f);
				verts.Add(pos + right * width * 0.5f);
				if (i > 0)
				{
					int baseIdx = i * 2;
					tris.Add(baseIdx - 2);
					tris.Add(baseIdx - 1);
					tris.Add(baseIdx + 0);
					tris.Add(baseIdx - 1);
					tris.Add(baseIdx + 1);
					tris.Add(baseIdx + 0);
				}
			}

			var mesh = new Mesh();
			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.SetVertices(verts);
			mesh.SetTriangles(tris, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			_meshFilter.sharedMesh = mesh;
		}
	}
}