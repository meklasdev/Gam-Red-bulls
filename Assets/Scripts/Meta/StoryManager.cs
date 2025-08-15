using UnityEngine;

/// <summary>
/// Prosty manager fabuły: rywale, finał po wykonaniu kamieni milowych.
/// </summary>
public class StoryManager : MonoBehaviour
{
	[SerializeField] private MissionSystem missionSystem;
	[SerializeField] private int milestonesRequired = 5;
	[SerializeField] private bool finaleUnlocked;

	private int _completed;

	private void Awake()
	{
		if (missionSystem == null) missionSystem = FindObjectOfType<MissionSystem>();
		if (missionSystem != null)
		{
			missionSystem.OnMissionCompleted += _ => { _completed++; Evaluate(); };
		}
	}

	private void Evaluate()
	{
		if (!finaleUnlocked && _completed >= milestonesRequired)
		{
			finaleUnlocked = true;
			// odblokuj specjalny event/pojazd
			ProfileManager.Instance.AddCredits(500);
		}
	}
}