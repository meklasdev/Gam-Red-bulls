using UnityEngine;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Gameplay
{
	public enum ActivityType
	{
		Downhill,
		Motocross,
		Rally,
		Drift,
		F1
	}

	public class ActivityZone : MonoBehaviour
	{
		public ActivityType type;
		public string missionToCompleteOnWin;
		public float timeLimitSeconds = 180f;
		private float _timer;
		private bool _active;

		private void OnTriggerEnter(Collider other)
		{
			if (_active) return;
			if (!other.attachedRigidbody) return;
			StartActivity();
		}

		private void Update()
		{
			if (!_active) return;
			_timer += Time.deltaTime;
			if (_timer > timeLimitSeconds)
			{
				StopActivity(false);
			}
		}

		public void StartActivity()
		{
			_active = true;
			_timer = 0f;
			GameManager.Instance.SetState(GameState.Racing);
			Debug.Log($"Activity {type} started");
		}

		public void StopActivity(bool success)
		{
			_active = false;
			GameManager.Instance.SetState(GameState.Exploring);
			if (success && !string.IsNullOrEmpty(missionToCompleteOnWin))
			{
				var ms = FindObjectOfType<MissionSystem>();
				if (ms) ms.Complete(missionToCompleteOnWin);
			}
			Debug.Log($"Activity {type} finished. Success: {success}");
		}
	}
}