using UnityEngine;

/// <summary>
/// Prosty third-person controller dla postaci (do roamingu po mapie).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float rotationSpeed = 360f;
	[SerializeField] private float gravity = -9.81f;

	private CharacterController _cc;
	private Vector3 _vel;

	private void Awake()
	{
		_cc = GetComponent<CharacterController>();
	}

	private void Update()
	{
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		Vector3 input = new Vector3(h, 0f, v);
		if (input.sqrMagnitude > 0.01f)
		{
			Quaternion targetRot = Quaternion.LookRotation(input);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
		}
		Vector3 move = transform.forward * input.magnitude * moveSpeed;
		_vel.y += gravity * Time.deltaTime;
		_cc.Move((move + _vel) * Time.deltaTime);
		if (_cc.isGrounded) _vel.y = -0.5f;
	}
}