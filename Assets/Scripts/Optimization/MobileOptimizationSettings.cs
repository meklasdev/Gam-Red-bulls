using UnityEngine;

/// <summary>
/// Ustawienia optymalizacji dla urządzeń mobilnych.
/// </summary>
public class MobileOptimizationSettings : MonoBehaviour
{
	[SerializeField] private int targetFrameRate = 60;
	[SerializeField] private bool enableVSync = false;
	[SerializeField] private int textureQuality = 1; // 0 full, 1 half, 2 quarter
	[SerializeField] private int anisotropicFiltering = 4;
	[SerializeField] private bool enableShadows = true;

	private void Start()
	{
		Application.targetFrameRate = targetFrameRate;
		QualitySettings.vSyncCount = enableVSync ? 1 : 0;
		QualitySettings.masterTextureLimit = Mathf.Clamp(textureQuality, 0, 3);
		QualitySettings.anisotropicFiltering = anisotropicFiltering > 0 ? AnisotropicFiltering.ForceEnable : AnisotropicFiltering.Disable;
		QualitySettings.shadowCascades = enableShadows ? 2 : 0;
		QualitySettings.shadowDistance = enableShadows ? 60f : 0f;
	}
}