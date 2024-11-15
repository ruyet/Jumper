using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject loadingPanel;
    public Slider progressSlider;
    public Text progressText;

    public GameObject[] cutscenes; // Assign each cutscene UI Image object in the Inspector
    public float[] cutsceneDurations; // Assign durations for each cutscene in the Inspector

    public void PlayGame()
    {
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        // Show loading screen and reset slider
        progressSlider.value = 0;
        loadingPanel.SetActive(true);

        // Load the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        asyncLoad.allowSceneActivation = false; // Prevent immediate activation

        // Fake loading progress
        int fakeProgress = 0;
        while (fakeProgress < 100)
        {
            fakeProgress++;
            progressSlider.value = fakeProgress / 100f;
            yield return new WaitForSeconds(0.01f);
        }

        // Hide loading screen
        loadingPanel.SetActive(false);

        // Show cutscenes after loading
        yield return StartCoroutine(ShowCutscenes());

        // Activate the scene only after cutscenes are complete
        asyncLoad.allowSceneActivation = true;
    }

    private IEnumerator ShowCutscenes()
    {
        for (int i = 0; i < cutscenes.Length; i++)
        {
            // Activate the current cutscene
            cutscenes[i].SetActive(true);

            // Wait for the specified duration
            yield return new WaitForSeconds(cutsceneDurations[i]);

            // Deactivate the current cutscene before moving to the next
            cutscenes[i].SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
