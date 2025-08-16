using UnityEngine;
using TMPro;
using ExtremeRacing.Gameplay;
using ExtremeRacing.Managers;

namespace ExtremeRacing.UI
{
	public class MissionSelectUI : MonoBehaviour
	{
		public TextMeshProUGUI listText;
		public MissionSystem missionSystem;

		private void OnEnable()
		{
			Refresh();
		}

		public void Refresh()
		{
			if (missionSystem == null || listText == null) return;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (var m in missionSystem.Missions)
			{
				sb.AppendLine($"- {m.title} {(m.completed ? "(done)" : "")}");
			}
			listText.text = sb.ToString();
		}

		public void StartGorski() => GameManager.Instance.LoadRegionGorski();
		public void StartPustynny() => GameManager.Instance.LoadRegionPustynny();
		public void StartMiasto() => GameManager.Instance.LoadRegionMiasto();
		public void StartPort() => GameManager.Instance.LoadRegionPort();
		public void StartTor() => GameManager.Instance.LoadRegionTor();
		public void StartArena() => GameManager.Instance.LoadRegionArena();
	}
}