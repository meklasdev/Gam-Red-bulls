using UnityEngine;

namespace ExtremeRacing.Cutscenes
{
	public class CutsceneTrigger : MonoBehaviour
	{
		public CutsceneAsset asset;
		public CutscenePlayer player;
		public bool once = true;
		private bool _played;

		private void OnTriggerEnter(Collider other)
		{
			if (_played && once) return;
			if (!other.attachedRigidbody) return;
			if (player == null) player = FindObjectOfType<CutscenePlayer>();
			if (player && asset)
			{
				_played = true;
				StartCoroutine(player.Play(asset));
			}
		}
	}
}