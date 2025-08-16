using UnityEngine;

namespace ExtremeRacing.Optimization
{
	public class MobileOptimizer : MonoBehaviour
	{
		public Renderer[] renderers;
		public bool enableGPUInstancing = true;

		[ContextMenu("Optimize Renderers")]
		public void Optimize()
		{
			if (renderers == null) return;
			foreach (var r in renderers)
			{
				if (r == null) continue;
				foreach (var mat in r.sharedMaterials)
				{
					if (mat == null) continue;
					if (enableGPUInstancing) mat.enableInstancing = true;
				}
			}
		}
	}
}