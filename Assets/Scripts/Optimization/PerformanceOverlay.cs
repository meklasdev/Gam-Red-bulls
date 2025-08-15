using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Overlay wydajności – podgląd FPS i aktywnego presetu.
/// </summary>
public class PerformanceOverlay : MonoBehaviour
{
	[SerializeField] private Text label;
	[SerializeField] private OptimizationManager optimizationManager;
	[SerializeField] private float avgWindow = 1f;

	private float _acc;
	private int _frames;
	private float _avg;

	private void Update()
	{
		float fps = Time.timeScale / Mathf.Max(Time.deltaTime, 0.0001f);
		_acc += fps; _frames++;
		if (_frames >= avgWindow / Mathf.Max(Time.deltaTime, 0.0001f)) { _avg = _acc / _frames; _acc = 0f; _frames = 0; }
		if (label != null)
		{
			string presetName = optimizationManager != null && optimizationManager.CurrentPreset != null ? optimizationManager.CurrentPreset.displayName : "-";
			label.text = $"FPS: {fps:0.} (avg {_avg:0.})\nPreset: {presetName}";
		}
	}
}