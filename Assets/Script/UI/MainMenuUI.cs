
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuContent;

    [Header("Other Panels")]
    public GameObject settingsPanel;
    public GameObject howToPlayPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuContent.SetActive(true);

        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        mainMenuContent.SetActive(false);
        settingsPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    public void OpenHowToPlay()
    {
        mainMenuContent.SetActive(false);
        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("Quit Game");
    }
}
