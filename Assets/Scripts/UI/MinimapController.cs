using UnityEngine;

/// <summary>
/// Minimap – kamera z góry podążająca za graczem.
/// </summary>
public class MinimapController : MonoBehaviour
{
	[SerializeField] private Transform target;
	[SerializeField] private float height = 50f;
	[SerializeField] private float followLerp = 10f;
	[SerializeField] private bool rotateWithTarget = true;

	private void LateUpdate()
	{
		if (target == null) return;
		Vector3 desired = target.position + Vector3.up * height;
		transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followLerp);
		if (rotateWithTarget)
		{
			transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
		}
		else
		{
			transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		}
	}
}