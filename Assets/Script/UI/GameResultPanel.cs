
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameResultPanel : MonoBehaviour
{
    [Header("Result Panel")]
    public GameObject resultPanel;

    [Header("Result Text")]
    public TMP_Text resultTitleText;
    public TMP_Text resultDescriptionText;

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

    }

    public void ShowResult(bool playerWon)
    {
        if (resultPanel == null)
            return;

        resultPanel.SetActive(true);

        if (playerWon)
        {
            if (resultTitleText != null)
                resultTitleText.text = "VICTORY!";

            if (resultDescriptionText != null)
                resultDescriptionText.text = "YOU REACHED THE SUMMIT!";
        }
        else
        {
            if (resultTitleText != null)
                resultTitleText.text = "DEFEAT!";

            if (resultDescriptionText != null)
                resultDescriptionText.text = "AI REACHED THE SUMMIT!";
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
