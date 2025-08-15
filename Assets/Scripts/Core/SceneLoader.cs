using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Narzędzia do asynchronicznego ładowania scen z prostym ekranem ładowania.
/// </summary>
public class SceneLoader : MonoBehaviour
{
	[SerializeField] private Canvas loadingCanvas;
	[SerializeField] private UnityEngine.UI.Slider progressBar;

	/// <summary>
	/// Ładuje scenę po nazwie.
	/// </summary>
	public void LoadSceneByName(string sceneName)
	{
		StartCoroutine(LoadSceneRoutine(sceneName));
	}

	private IEnumerator LoadSceneRoutine(string sceneName)
	{
		if (loadingCanvas != null) loadingCanvas.enabled = true;
		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
		operation.allowSceneActivation = false;

		while (!operation.isDone)
		{
			float progress = Mathf.Clamp01(operation.progress / 0.9f);
			if (progressBar != null) progressBar.value = progress;

			if (operation.progress >= 0.9f)
			{
				// Tutaj można dodać fade-out lub warunek potwierdzenia
				operation.allowSceneActivation = true;
			}
			yield return null;
		}

		if (loadingCanvas != null) loadingCanvas.enabled = false;
	}
}