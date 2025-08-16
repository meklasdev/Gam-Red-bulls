using UnityEngine;

namespace ExtremeRacing.UI
{
	[RequireComponent(typeof(RectTransform))]
	public class SafeAreaFitter : MonoBehaviour
	{
		private RectTransform _rt;
		private Rect _last;

		private void Awake()
		{
			_rt = GetComponent<RectTransform>();
			ApplySafeArea();
		}

		private void Update()
		{
			if (_last != Screen.safeArea) ApplySafeArea();
		}

		private void ApplySafeArea()
		{
			_last = Screen.safeArea;
			Vector2 anchorMin = _last.position;
			Vector2 anchorMax = _last.position + _last.size;
			anchorMin.x /= Screen.width;
			anchorMin.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;
			_rt.anchorMin = anchorMin;
			_rt.anchorMax = anchorMax;
		}
	}
}