#if UNITY_EDITOR
using UnityEditor;
using System;

public static class BuildCI
{
	public static void BuildAndroid()
	{
		var buildPlayerOptions = new BuildPlayerOptions
		{
			scenes = GetScenes(),
			locationPathName = "Builds/Android/ExtremeRacing.apk",
			target = BuildTarget.Android,
			options = BuildOptions.None
		};
		BuildPipeline.BuildPlayer(buildPlayerOptions);
	}

	public static void BuildiOS()
	{
		var buildPlayerOptions = new BuildPlayerOptions
		{
			scenes = GetScenes(),
			locationPathName = "Builds/iOS",
			target = BuildTarget.iOS,
			options = BuildOptions.None
		};
		BuildPipeline.BuildPlayer(buildPlayerOptions);
	}

	private static string[] GetScenes()
	{
		var scenes = new string[EditorBuildSettings.scenes.Length];
		for (int i = 0; i < scenes.Length; i++) scenes[i] = EditorBuildSettings.scenes[i].path;
		return scenes;
	}
}
#endif