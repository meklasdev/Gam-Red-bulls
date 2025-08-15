using UnityEngine;

/// <summary>
/// DJ Red Bull Live / 64 Bars – prosty system eventów muzycznych i misji freestyle.
/// </summary>
public class DJEventManager : MonoBehaviour
{
	[SerializeField] private AudioSource musicSource;
	[SerializeField] private AudioClip[] mixClips;
	[SerializeField] private MissionSystem missionSystem;

	public void PlayRandomMix()
	{
		if (musicSource == null || mixClips == null || mixClips.Length == 0) return;
		musicSource.clip = mixClips[Random.Range(0, mixClips.Length)];
		musicSource.loop = false;
		musicSource.Play();
		missionSystem?.ReportTrickPerformed("dj_mix");
	}

	public void TriggerFreestyle()
	{
		missionSystem?.ReportTrickPerformed("freestyle");
	}
}