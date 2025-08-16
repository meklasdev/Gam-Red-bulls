using UnityEngine;

namespace ExtremeRacing.Optimization
{
	[RequireComponent(typeof(LODGroup))]
	public class LODSetup : MonoBehaviour
	{
		public float lod0 = 0.4f;
		public float lod1 = 0.15f;
		public float lod2 = 0.03f;

		[ContextMenu("Generate LODs")]
		public void GenerateLODs()
		{
			var group = GetComponent<LODGroup>();
			var renderers = GetComponentsInChildren<Renderer>();
			if (renderers.Length == 0) return;
			var lods = new LOD[3];
			lods[0] = new LOD(lod0, renderers);
			lods[1] = new LOD(lod1, renderers);
			lods[2] = new LOD(lod2, renderers);
			group.SetLODs(lods);
			group.RecalculateBounds();
		}
	}
}