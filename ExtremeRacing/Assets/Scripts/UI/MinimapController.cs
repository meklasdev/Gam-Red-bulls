using UnityEngine;

namespace ExtremeRacing.UI
{
	public class MinimapController : MonoBehaviour
	{
		public Transform target;
		public Camera minimapCamera;
		public float height = 80f;

		private void LateUpdate()
		{
			if (target == null || minimapCamera == null) return;
			Vector3 pos = target.position;
			pos.y += height;
			minimapCamera.transform.position = pos;
			minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		}
	}
}