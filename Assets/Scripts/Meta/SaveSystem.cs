using UnityEngine;

/// <summary>
/// Prosty system zapisu opartego na PlayerPrefs.
/// </summary>
public static class SaveSystem
{
	public static void SaveInt(string key, int value) => PlayerPrefs.SetInt(key, value);
	public static int LoadInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);

	public static void SaveFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
	public static float LoadFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);

	public static void SaveString(string key, string value) => PlayerPrefs.SetString(key, value);
	public static string LoadString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);

	public static void SaveBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
	public static bool LoadBool(string key, bool defaultValue = false) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

	public static void Flush() => PlayerPrefs.Save();
}