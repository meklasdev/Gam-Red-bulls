using UnityEngine;

/// <summary>
/// Strefa DRS – dodaje tymczasowy boost siły napędowej dla pojazdu w strefie.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DRSZone : MonoBehaviour
{
	[SerializeField] private float boostForce = 2000f;
	[SerializeField] private float maxSpeedKmh = 320f;

	private void Awake()
	{
		var col = GetComponent<Collider>();
		col.isTrigger = true;
	}

	private void OnTriggerStay(Collider other)
	{
		var rb = other.attachedRigidbody;
		if (rb == null) return;
		float speed = rb.velocity.magnitude * 3.6f;
		if (speed < maxSpeedKmh)
		{
			rb.AddForce(other.transform.forward * boostForce * Time.deltaTime, ForceMode.Acceleration);
		}
	}
}