using System;
using UnityEngine;

namespace ExtremeRacing.Cutscenes
{
	[Serializable]
	public class CutsceneShot
	{
		public Vector3 position;
		public Vector3 eulerAngles;
		public float duration = 2f;
		public string subtitle;
	}

	[CreateAssetMenu(menuName = "ExtremeRacing/Cutscene", fileName = "Cutscene")]
	public class CutsceneAsset : ScriptableObject
	{
		public CutsceneShot[] shots;
	}
}