using UnityEngine;

namespace ExtremeRacing.Infrastructure
{
	public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T _instance;
		private static readonly object _lock = new object();
		private static bool _quitting;

		public static T Instance
		{
			get
			{
				if (_quitting) return null;
				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = FindObjectOfType<T>();
						if (_instance == null)
						{
							var go = new GameObject(typeof(T).Name);
							_instance = go.AddComponent<T>();
							DontDestroyOnLoad(go);
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
			_quitting = true;
		}
	}
}