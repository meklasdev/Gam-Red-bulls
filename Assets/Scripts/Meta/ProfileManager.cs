using System.IO;
using UnityEngine;

/// <summary>
/// Menedżer profilu – zapis/odczyt pliku JSON.
/// </summary>
public class ProfileManager : Singleton<ProfileManager>
{
	[SerializeField] private string fileName = "profile.json";
	public PlayerProfile Profile { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Load();
	}

	public void Load()
	{
		string path = Path.Combine(Application.persistentDataPath, fileName);
		if (File.Exists(path))
		{
			Profile = JsonUtility.FromJson<PlayerProfile>(File.ReadAllText(path));
		}
		else
		{
			Profile = PlayerProfile.NewDefault();
			Save();
		}
	}

	public void Save()
	{
		string path = Path.Combine(Application.persistentDataPath, fileName);
		File.WriteAllText(path, JsonUtility.ToJson(Profile));
	}

	public void AddCredits(int amount)
	{
		Profile.credits = Mathf.Max(0, Profile.credits + amount);
		Save();
	}
}