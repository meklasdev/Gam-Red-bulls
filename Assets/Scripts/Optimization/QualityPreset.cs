using UnityEngine;
#if UNITY_RENDER_PIPELINE_URP || UNITY_URP
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
#endif

/// <summary>
/// Preset jakości i wydajności stosowany przez OptimizationManager.
/// </summary>
[CreateAssetMenu(menuName = "RedBull/Quality Preset", fileName = "QualityPreset")]
public class QualityPreset : ScriptableObject
{
	[Header("Ogólne")]
	public string displayName = "Medium";
	public int targetFrameRate = 60;
	[Range(0.5f, 1.0f)] public float renderScale = 1.0f;
	[Range(0.25f, 4f)] public float lodBias = 1.0f;
	[Range(0, 3)] public int maximumLODLevel = 0;
	[Range(0, 3)] public int textureQuality = 1; // QualitySettings.masterTextureLimit
	public bool anisotropicEnable = true;

	[Header("Cienie")] 
	public bool shadowsEnabled = true;
	[Range(0, 500)] public float shadowDistance = 60f;
	[Range(0, 4)] public int shadowCascades = 2;

	[Header("AA / MSAA")] 
	[Range(1, 8)] public int msaaSamples = 2;

	public void Apply()
	{
		Application.targetFrameRate = targetFrameRate;
		QualitySettings.masterTextureLimit = Mathf.Clamp(textureQuality, 0, 3);
		QualitySettings.anisotropicFiltering = anisotropicEnable ? AnisotropicFiltering.ForceEnable : AnisotropicFiltering.Disable;
		QualitySettings.lodBias = lodBias;
		QualitySettings.maximumLODLevel = maximumLODLevel;
		QualitySettings.shadowDistance = shadowsEnabled ? shadowDistance : 0f;
		QualitySettings.shadowCascades = shadowsEnabled ? shadowCascades : 0;

		#if UNITY_RENDER_PIPELINE_URP || UNITY_URP
		var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		if (rp != null)
		{
			rp.renderScale = renderScale;
			rp.msaaSampleCount = msaaSamples;
			rp.shadowDistance = shadowsEnabled ? shadowDistance : 0f;
		}
		#endif
	}
}