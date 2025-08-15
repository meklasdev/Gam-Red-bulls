using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI do malowania auta: wybór presetu i załadowanie zdjęcia użytkownika z dysku.
/// Na mobile: do integracji z natywną galerią (tu placeholder metoda SetUserImageTexture()).
/// </summary>
public class CarPaintUI : MonoBehaviour
{
	[SerializeField] private CarPainter targetCarPainter;
	[SerializeField] private CarPaintPreset[] presets;
	[SerializeField] private Transform presetListRoot;
	[SerializeField] private Button presetButtonTemplate;
	[SerializeField] private InputField desktopPathInput;
	[SerializeField] private Button loadImageButton;
	[SerializeField] private RawImage preview;

	private void Start()
	{
		BuildPresetButtons();
		if (loadImageButton != null) loadImageButton.onClick.AddListener(LoadImageFromDesktopPath);
	}

	private void BuildPresetButtons()
	{
		foreach (Transform c in presetListRoot) Destroy(c.gameObject);
		foreach (var p in presets)
		{
			var btn = Instantiate(presetButtonTemplate, presetListRoot);
			btn.gameObject.SetActive(true);
			btn.GetComponentInChildren<Text>().text = p != null ? p.name : "Preset";
			btn.onClick.AddListener(() => ApplyPreset(p));
		}
	}

	private void ApplyPreset(CarPaintPreset p)
	{
		targetCarPainter?.ApplyPreset(p);
	}

	private void LoadImageFromDesktopPath()
	{
		if (desktopPathInput == null || string.IsNullOrEmpty(desktopPathInput.text)) return;
		string path = desktopPathInput.text.Trim();
		if (!File.Exists(path)) return;
		byte[] bytes = File.ReadAllBytes(path);
		var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		if (tex.LoadImage(bytes))
		{
			preview.texture = tex;
			preview.gameObject.SetActive(true);
			targetCarPainter?.ApplyUserImage(tex);
		}
	}

	// Mobile placeholder – podłącz tu wywołanie natywnego selektora plików/galerii
	public void SetUserImageTexture(Texture2D tex)
	{
		if (tex == null) return;
		preview.texture = tex;
		preview.gameObject.SetActive(true);
		targetCarPainter?.ApplyUserImage(tex);
	}
}