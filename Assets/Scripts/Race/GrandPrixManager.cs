using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menedżer Grand Prix: obsługa kwalifikacji i serii wyścigów na torze F1.
/// </summary>
public class GrandPrixManager : MonoBehaviour
{
    [SerializeField] private RaceManager qualifyingRace; // pojedyncze okrążenie kwalifikacyjne
    [SerializeField] private List<RaceManager> mainRaces = new List<RaceManager>(); // kolejność wyścigów

    private int _currentRaceIndex;

    private void Start()
    {
        if (qualifyingRace != null)
        {
            qualifyingRace.OnRaceCompleted += OnQualifyingCompleted;
            qualifyingRace.gameObject.SetActive(true);
        }
        else
        {
            StartNextRace();
        }
    }

    private void OnQualifyingCompleted(List<float> lapTimes)
    {
        qualifyingRace.OnRaceCompleted -= OnQualifyingCompleted;
        qualifyingRace.gameObject.SetActive(false);
        // Docelowo: ustalenie pozycji startowych na podstawie czasu okrążenia
        StartNextRace();
    }

    private void StartNextRace()
    {
        if (_currentRaceIndex >= mainRaces.Count)
        {
            // Cała seria zakończona
            return;
        }

        var race = mainRaces[_currentRaceIndex];
        race.OnRaceCompleted += OnRaceFinished;
        race.gameObject.SetActive(true);
    }

    private void OnRaceFinished(List<float> lapTimes)
    {
        var race = mainRaces[_currentRaceIndex];
        race.OnRaceCompleted -= OnRaceFinished;
        race.gameObject.SetActive(false);

        _currentRaceIndex++;
        StartNextRace();
    }
}
