using UnityEngine;

/// <summary>
/// Sterowanie dronem i proste eventy przelotu przez pierścienie.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 10f;
	[SerializeField] private float ascendSpeed = 5f;
	[SerializeField] private float turnSpeed = 90f;

	private Rigidbody _rb;
	private MissionSystem _missionSystem;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody>();
		_missionSystem = FindObjectOfType<MissionSystem>();
	}

	private void Update()
	{
		// Prosty manualny tryb (dla testów w edytorze). Na mobile – podłącz pod UI.
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		float ascend = (Input.GetKey(KeyCode.E) ? 1f : 0f) + (Input.GetKey(KeyCode.Q) ? -1f : 0f);
		float yaw = (Input.GetKey(KeyCode.D) ? 1f : 0f) + (Input.GetKey(KeyCode.A) ? -1f : 0f);

		Vector3 vel = transform.forward * v * moveSpeed + Vector3.up * ascend * ascendSpeed + transform.right * h * moveSpeed;
		_rb.velocity = vel;
		transform.Rotate(Vector3.up, yaw * turnSpeed * Time.deltaTime, Space.World);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DroneRing"))
		{
			_missionSystem?.ReportRaceWin(); // Używamy jako zaliczenie checkpointu/wyzwania
		}
	}
}