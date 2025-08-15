using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menedżer wyścigu: checkpointy, okrążenia, czas okrążeń, zakończenie.
/// </summary>
public class RaceManager : MonoBehaviour
{
	[SerializeField] private Transform startFinishLine;
	[SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();
	[SerializeField] private int totalLaps = 3;
	[SerializeField] private Transform player;

	private int _currentLap = 1;
	private int _nextCheckpointIndex = 0;
	private float _lapStartTime;
	private readonly List<float> _lapTimes = new List<float>();
	private bool _raceFinished;

	public event Action<int, float> OnLapCompleted; // (lapIndex, lapTime)
	public event Action<List<float>> OnRaceCompleted;

	private void Awake()
	{
		for (int i = 0; i < checkpoints.Count; i++)
		{
			if (checkpoints[i] == null) continue;
			checkpoints[i].Initialize(this, i);
		}
	}

	private void Start()
	{
		_lapStartTime = Time.time;
	}

	public void PassCheckpoint(int index, Transform passer)
	{
		if (_raceFinished || passer != player) return;
		if (index != _nextCheckpointIndex) return; // nie po kolei

		_nextCheckpointIndex++;
		if (_nextCheckpointIndex >= checkpoints.Count)
		{
			CompleteLap();
		}
	}

	private void CompleteLap()
	{
		float lapTime = Time.time - _lapStartTime;
		_lapTimes.Add(lapTime);
		OnLapCompleted?.Invoke(_currentLap, lapTime);

		if (_currentLap >= totalLaps)
		{
			_raceFinished = true;
			OnRaceCompleted?.Invoke(_lapTimes);
			return;
		}

		_currentLap++;
		_nextCheckpointIndex = 0;
		_lapStartTime = Time.time;
	}
}