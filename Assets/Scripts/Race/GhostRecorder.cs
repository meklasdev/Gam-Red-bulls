using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Rejestrator i odtwarzacz ghosta dla time trial.
/// </summary>
public class GhostRecorder : MonoBehaviour
{
	[SerializeField] private Transform target;
	[SerializeField] private float recordHz = 20f;
	[SerializeField] private string fileName = "best_ghost.json";

	private float _timer;
	private List<Point> _points = new List<Point>();

	[System.Serializable]
	private struct Point { public Vector3 p; public Quaternion r; }

	private void Update()
	{
		if (target == null) return;
		_timer += Time.deltaTime;
		float step = 1f / Mathf.Max(1f, recordHz);
		if (_timer >= step)
		{
			_points.Add(new Point { p = target.position, r = target.rotation });
			_timer = 0f;
		}
	}

	public void Save()
	{
		var data = JsonUtility.ToJson(new Wrapper { list = _points });
		File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), data);
	}

	[System.Serializable]
	private class Wrapper { public List<Point> list; }
}

public class GhostPlayer : MonoBehaviour
{
	[SerializeField] private Transform target;
	[SerializeField] private string fileName = "best_ghost.json";
	[SerializeField] private float playSpeed = 1f;

	private List<Point> _points;
	private float _t;

	[System.Serializable]
	private struct Point { public Vector3 p; public Quaternion r; }
	[System.Serializable]
	private class Wrapper { public List<Point> list; }

	private void Start()
	{
		string path = Path.Combine(Application.persistentDataPath, fileName);
		if (File.Exists(path))
		{
			_points = JsonUtility.FromJson<Wrapper>(File.ReadAllText(path)).list;
		}
	}

	private void Update()
	{
		if (target == null || _points == null || _points.Count == 0) return;
		_t += Time.deltaTime * playSpeed;
		int idx = Mathf.Clamp(Mathf.FloorToInt(_t * 20f), 0, _points.Count - 1);
		target.SetPositionAndRotation(_points[idx].p, _points[idx].r);
	}
}