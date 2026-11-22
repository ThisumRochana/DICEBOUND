
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Start()
    {
        Time.timeScale = 1f;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        isPaused = false;
    }

    // HUD Pause Button
    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    // Resume Button
    public void ResumeGame()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1f;
    }

    // Pause Menu Settings Button
    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);

        isPaused = true;
        Time.timeScale = 0f;
    }

    // Settings Panel Back Button
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);

        isPaused = true;
        Time.timeScale = 0f;
    }

    // Restart Button
    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Main Menu Button
    public void MainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}