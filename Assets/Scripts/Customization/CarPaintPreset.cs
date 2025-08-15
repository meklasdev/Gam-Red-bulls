using UnityEngine;

/// <summary>
/// Preset malowania auta: kolory i opcjonalna grafika (livery/decal jako tekstura).
/// </summary>
[CreateAssetMenu(menuName = "RedBull/Car Paint Preset", fileName = "CarPaintPreset")]
public class CarPaintPreset : ScriptableObject
{
	[Header("Kolory")]
	public Color primaryColor = Color.red;
	public Color secondaryColor = Color.white;

	[Header("Livery / Naklejka (opcjonalna)")]
	public Texture2D overlayTexture;
	[Tooltip("Pozycja naklejki w UV (0-1)")]
	public Vector2 overlayUvMin = new Vector2(0.1f, 0.1f);
	public Vector2 overlayUvMax = new Vector2(0.4f, 0.4f);
}