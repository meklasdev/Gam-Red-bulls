using UnityEngine;
using TMPro;
using ExtremeRacing.Vehicles;

namespace ExtremeRacing.UI
{
	public class HUDController : MonoBehaviour
	{
		public TextMeshProUGUI speedText;
		public TextMeshProUGUI statusText;
		public VehicleController playerVehicle;

		private void Update()
		{
			if (playerVehicle != null && speedText != null)
			{
				speedText.text = $"{playerVehicle.GetSpeedKmh():0} km/h";
			}
		}

		public void SetStatus(string msg)
		{
			if (statusText != null) statusText.text = msg;
		}
	}
}