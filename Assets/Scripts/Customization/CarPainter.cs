using UnityEngine;

/// <summary>
/// Aplikuje malowanie auta: kolory oraz nakładkę graficzną (zdjęcie użytkownika).
/// Wymaga materiału z shaderem wspierającym kolory bazowe i dodatkową teksturę (np. URP/Lit + dodatkowy slot).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class CarPainter : MonoBehaviour
{
	[SerializeField] private Renderer targetRenderer;
	[SerializeField] private int materialIndex = 0;
	[SerializeField] private string colorProperty = "_BaseColor"; // URP/Lit
	[SerializeField] private string overlayTextureProperty = "_DetailAlbedoMap"; // użyj detail map jako naklejki
	[SerializeField] private string overlayColorProperty = "_DetailColor";

	private Material _instancedMat;

	private void Awake()
	{
		if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
		EnsureMaterialInstance();
	}

	private void EnsureMaterialInstance()
	{
		if (targetRenderer == null) return;
		var mats = targetRenderer.sharedMaterials;
		if (mats == null || mats.Length <= materialIndex) return;
		_instancedMat = new Material(mats[materialIndex]);
		mats[materialIndex] = _instancedMat;
		targetRenderer.sharedMaterials = mats;
	}

	public void ApplyPreset(CarPaintPreset preset)
	{
		if (_instancedMat == null || preset == null) return;
		_instancedMat.SetColor(colorProperty, preset.primaryColor);
		if (_instancedMat.HasProperty("_BaseColor2")) _instancedMat.SetColor("_BaseColor2", preset.secondaryColor);
		if (!string.IsNullOrEmpty(overlayTextureProperty))
		{
			_instancedMat.SetTexture(overlayTextureProperty, preset.overlayTexture);
			if (_instancedMat.HasProperty(overlayColorProperty)) _instancedMat.SetColor(overlayColorProperty, Color.white);
		}
	}

	/// <summary>
	/// Ustawia zdjęcie użytkownika jako naklejkę. Obraz musi być teksturą RGBA.
	/// </summary>
	public void ApplyUserImage(Texture2D userTexture)
	{
		if (_instancedMat == null || userTexture == null) return;
		_instancedMat.SetTexture(overlayTextureProperty, userTexture);
		if (_instancedMat.HasProperty(overlayColorProperty)) _instancedMat.SetColor(overlayColorProperty, Color.white);
	}
}