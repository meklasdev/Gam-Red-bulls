using UnityEngine;

/// <summary>
/// Ogólny singleton dla komponentów MonoBehaviour.
/// Zapewnia istnienie tylko jednej instancji typu T w czasie działania gry.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;
	private static readonly object _lock = new object();
	public static bool IsQuitting { get; private set; }

	/// <summary>
	/// Globalny dostęp do instancji singletona.
	/// </summary>
	public static T Instance
	{
		get
		{
			if (IsQuitting) return null;
			lock (_lock)
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<T>();
					if (_instance == null)
					{
						var gameObject = new GameObject(typeof(T).Name);
						_instance = gameObject.AddComponent<T>();
						DontDestroyOnLoad(gameObject);
					}
				}
				return _instance;
			}
		}
	}

	protected virtual void Awake()
	{
		if (_instance == null)
		{
			_instance = this as T;
			DontDestroyOnLoad(gameObject);
		}
		else if (_instance != this)
		{
			Destroy(gameObject);
		}
	}

	protected virtual void OnApplicationQuit()
	{
		IsQuitting = true;
	}
}