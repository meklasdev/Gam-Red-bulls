using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System misji: definicje, postęp i ukończenia.
/// </summary>
public class MissionSystem : MonoBehaviour
{
	public enum MissionType
	{
		WinRace,
		DriftDistance,
		CollectCrates,
		PerformTricks
	}

	[Serializable]
	public class MissionDefinition
	{
		public string id;
		public MissionType type;
		public float targetValue;
		[TextArea] public string description;

		[NonSerialized] public float currentValue;
		[NonSerialized] public bool isCompleted;
	}

	[SerializeField] private List<MissionDefinition> sceneMissions = new List<MissionDefinition>();
	private Dictionary<string, MissionDefinition> _idToMission = new Dictionary<string, MissionDefinition>();

	public delegate void MissionProgressHandler(MissionDefinition mission);
	public event MissionProgressHandler OnMissionProgress;
	public event MissionProgressHandler OnMissionCompleted;

	public void PrepareSceneMissions()
	{
		_idToMission.Clear();
		foreach (var m in sceneMissions)
		{
			m.currentValue = 0f;
			m.isCompleted = false;
			if (!string.IsNullOrEmpty(m.id)) _idToMission[m.id] = m;
		}
	}

	public void StartMissionById(string id)
	{
		// Rezerwuj miejsce na rozszerzenia – obecnie misje pasywne zaczynają się automatycznie
	}

	public void ReportRaceWin()
	{
		UpdateMissions(MissionType.WinRace, 1f);
	}

	public void ReportDriftMeters(float meters)
	{
		UpdateMissions(MissionType.DriftDistance, meters);
	}

	public void ReportCrateCollected()
	{
		UpdateMissions(MissionType.CollectCrates, 1f);
	}

	public void ReportTrickPerformed(string trickName)
	{
		UpdateMissions(MissionType.PerformTricks, 1f);
	}

	private void UpdateMissions(MissionType type, float delta)
	{
		foreach (var mission in sceneMissions)
		{
			if (mission.isCompleted || mission.type != type) continue;
			mission.currentValue += delta;
			OnMissionProgress?.Invoke(mission);
			if (mission.currentValue >= mission.targetValue)
			{
				mission.isCompleted = true;
				OnMissionCompleted?.Invoke(mission);
			}
		}
	}
}