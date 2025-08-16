using System.Collections.Generic;
using UnityEngine;
using ExtremeRacing.Managers;

namespace ExtremeRacing.Gameplay
{
	public class LapManager : MonoBehaviour
	{
		public List<Transform> checkpoints = new List<Transform>();
		public int totalLaps = 3;
		public float lapStartTime;
		private int _currentCheckpoint = 0;
		private int _currentLap = 1;
		private bool _active;

		public void Begin()
		{
			if (checkpoints.Count < 2) { Debug.LogWarning("LapManager: not enough checkpoints"); return; }
			_active = true;
			_currentCheckpoint = 0;
			_currentLap = 1;
			lapStartTime = Time.time;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!_active) return;
			if (checkpoints.Count == 0) return;
			Transform cp = checkpoints[_currentCheckpoint];
			if (other.transform == cp)
			{
				_currentCheckpoint = (_currentCheckpoint + 1) % checkpoints.Count;
				if (_currentCheckpoint == 0)
				{
					float lapTime = Time.time - lapStartTime;
					lapStartTime = Time.time;
					_currentLap++;
					if (_currentLap > totalLaps)
					{
						FinishRace(lapTime);
					}
				}
			}
		}

		private void FinishRace(float lastLapTime)
		{
			_active = false;
			GameManager.Instance.SetState(GameState.Exploring);
			FindObjectOfType<ResultsUI>()?.Show("Race", true, 500);
		}
	}
}