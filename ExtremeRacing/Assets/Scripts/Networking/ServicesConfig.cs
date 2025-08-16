using UnityEngine;

namespace ExtremeRacing.Networking
{
	[CreateAssetMenu(menuName = "ExtremeRacing/Services Config", fileName = "ServicesConfig")]
	public class ServicesConfig : ScriptableObject
	{
		public string environmentName = "production";
		public string cloudProjectId;
		public string relayEnvironmentId;
	}
}