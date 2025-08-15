using UnityEngine;
using System.Linq;

/// <summary>
/// Adaptacyjna optymalizacja: monitoruje FPS i przełącza presety jakości.
/// </summary>
public class OptimizationManager : MonoBehaviour
{
	[SerializeField] private QualityPreset[] presets;
	[SerializeField] private int startPresetIndex = 1; // Medium
	[SerializeField] private float downshiftBelowFps = 52f;
	[SerializeField] private float upshiftAboveFps = 58f;
	[SerializeField] private float sampleWindowSeconds = 1.0f;
	[SerializeField] private bool autoApplyOnStart = true;
	[SerializeField] private bool scaleParticleSystems = true;
	[SerializeField] private int particleMaxCountLow = 200;
	[SerializeField] private int particleMaxCountHigh = 2000;

	private int _currentIndex;
	private float _accum;
	private int _frames;
	private float _avgFps;

	public int CurrentIndex => _currentIndex;
	public QualityPreset CurrentPreset => (presets != null && presets.Length > 0 && _currentIndex >= 0 && _currentIndex < presets.Length) ? presets[_currentIndex] : null;

	private void Start()
	{
		if (presets == null || presets.Length == 0) return;
		_currentIndex = Mathf.Clamp(startPresetIndex, 0, presets.Length - 1);
		if (autoApplyOnStart) ApplyCurrent();
	}

	private void Update()
	{
		_accum += Time.timeScale / Mathf.Max(Time.deltaTime, 0.0001f);
		_frames++;
		if (_frames >= Mathf.Max(5, Mathf.RoundToInt(sampleWindowSeconds / Mathf.Max(Time.deltaTime, 0.0001f))))
		{
			_avgFps = _accum / _frames;
			_accum = 0f; _frames = 0;
			Evaluate();
		}
	}

	private void Evaluate()
	{
		if (presets == null || presets.Length == 0) return;
		if (_avgFps < downshiftBelowFps && _currentIndex > 0)
		{
			_currentIndex--;
			ApplyCurrent();
		}
		else if (_avgFps > upshiftAboveFps && _currentIndex < presets.Length - 1)
		{
			_currentIndex++;
			ApplyCurrent();
		}
	}

	public void ApplyCurrent()
	{
		var p = CurrentPreset;
		if (p == null) return;
		p.Apply();
		if (scaleParticleSystems) ScaleParticlesByPreset(_currentIndex);
	}

	private void ScaleParticlesByPreset(int idx)
	{
		int maxCount = Mathf.RoundToInt(Mathf.Lerp(particleMaxCountLow, particleMaxCountHigh, presets.Length <= 1 ? 1f : (float)idx / (presets.Length - 1)));
		var systems = FindObjectsOfType<ParticleSystem>(true);
		foreach (var ps in systems)
		{
			var main = ps.main;
			main.maxParticles = Mathf.Min(main.maxParticles, maxCount);
		}
	}
}