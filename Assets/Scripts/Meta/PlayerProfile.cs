using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dane profilu gracza: kredyty, odblokowane pojazdy, aktywny pojazd.
/// </summary>
[Serializable]
public class PlayerProfile
{
	public int credits = 0;
	public string activeVehicleId = "";
	public List<string> unlockedVehicleIds = new List<string>();

	public static PlayerProfile NewDefault()
	{
		return new PlayerProfile { credits = 1000 };
	}
}