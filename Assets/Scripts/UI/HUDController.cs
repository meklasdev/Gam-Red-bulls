using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD – prędkość, paliwo, okrążenia, drift.
/// </summary>
public class HUDController : MonoBehaviour
{
	[SerializeField] private Text speedText;
	[SerializeField] private Text lapText;
	[SerializeField] private Text fuelText;
	[SerializeField] private Text driftText;
	[SerializeField] private Rigidbody playerRb;
	[SerializeField] private FuelSystem fuelSystem;
	[SerializeField] private RaceManager raceManager;
	[SerializeField] private DriftScoring driftScoring;

	private void Awake()
	{
		if (driftScoring != null)
		{
			driftScoring.OnDriftTick += (points, dist) => { if (driftText != null) driftText.text = $"DRIFT: {points:0} ({dist:0.0}m)"; };
		}
		if (raceManager != null)
		{
			raceManager.OnLapCompleted += (lap, time) => UpdateLap(lap);
		}
	}

	private void Update()
	{
		if (playerRb != null && speedText != null)
		{
			speedText.text = $"{(playerRb.velocity.magnitude * 3.6f):0} km/h";
		}
		if (fuelSystem != null && fuelText != null)
		{
			fuelText.text = $"Paliwo: {fuelSystem.Fuel:0.0} / {fuelSystem.Capacity:0.0} L";
		}
	}

	private void UpdateLap(int currentLap)
	{
		if (lapText != null) lapText.text = $"Lap {currentLap}";
	}
}