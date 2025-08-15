using UnityEngine;

/// <summary>
/// Ustawia odległości obcinania (culling) per warstwa na kamerze.
/// </summary>
[RequireComponent(typeof(Camera))]
public class AdaptiveCulling : MonoBehaviour
{
	[System.Serializable]
	public struct LayerDistance
	{
		public string layerName;
		public float distance;
	}

	[SerializeField] private LayerDistance[] layers;

	private void OnEnable()
	{
		var cam = GetComponent<Camera>();
		float[] distances = new float[32];
		for (int i = 0; i < layers.Length; i++)
		{
			int layer = LayerMask.NameToLayer(layers[i].layerName);
			if (layer >= 0) distances[layer] = layers[i].distance;
		}
		cam.layerCullDistances = distances;
	}
}