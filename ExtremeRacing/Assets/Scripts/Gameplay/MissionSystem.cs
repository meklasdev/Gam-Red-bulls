using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRacing.Gameplay
{
	[Serializable]
	public class Mission
	{
		public string id;
		public string title;
		public string description;
		public bool completed;
		public int reward;
	}

	public class MissionSystem : MonoBehaviour
	{
		[SerializeField] private List<Mission> _missions = new List<Mission>
		{
			new Mission{ id="sandstorm_win", title="Wygraj w burzy piaskowej", description="Ukończ wyścig w Region_PustynnyKanion podczas burzy piaskowej", reward=500 },
			new Mission{ id="drift_500m", title="Drift 500 m", description="Utrzymaj drift łącznie przez 500 metrów", reward=300 },
			new Mission{ id="loot_crates", title="Zbierz 5 skrzynek", description="Znajdź skrzynki z łupem w otwartym świecie", reward=200 },
		};

		public IReadOnlyList<Mission> Missions => _missions;

		public void Complete(string missionId)
		{
			var m = _missions.Find(x => x.id == missionId);
			if (m != null && !m.completed)
			{
				m.completed = true;
				Debug.Log($"Mission completed: {m.title}. Reward: {m.reward}");
			}
		}
	}
}