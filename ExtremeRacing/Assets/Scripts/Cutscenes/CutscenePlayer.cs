using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ExtremeRacing.Cutscenes
{
	public class CutscenePlayer : MonoBehaviour
	{
		public Camera cutsceneCamera;
		public Text subtitleText;
		public float fadeTime = 0.5f;

		public IEnumerator Play(CutsceneAsset asset)
		{
			if (asset == null || asset.shots == null || asset.shots.Length == 0) yield break;
			if (cutsceneCamera == null)
			{
				var go = new GameObject("CutsceneCamera");
				cutsceneCamera = go.AddComponent<Camera>();
				cutsceneCamera.depth = 10;
			}
			cutsceneCamera.enabled = true;
			for (int i = 0; i < asset.shots.Length; i++)
			{
				var s = asset.shots[i];
				cutsceneCamera.transform.position = s.position;
				cutsceneCamera.transform.rotation = Quaternion.Euler(s.eulerAngles);
				if (subtitleText) subtitleText.text = s.subtitle;
				yield return new WaitForSeconds(s.duration);
			}
			if (subtitleText) subtitleText.text = string.Empty;
			cutsceneCamera.enabled = false;
		}
	}
}