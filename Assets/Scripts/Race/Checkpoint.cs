using UnityEngine;

/// <summary>
/// Checkpoint trasy – wykrywa przejazd gracza i zgłasza do RaceManager.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
	private RaceManager _raceManager;
	private int _index;

	public void Initialize(RaceManager manager, int index)
	{
		_raceManager = manager;
		_index = index;
		var col = GetComponent<Collider>();
		col.isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		_raceManager?.PassCheckpoint(_index, other.transform);
	}
}