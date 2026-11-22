
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LuckyNumberUI : MonoBehaviour
{
    [Header("Game References")]
    public LuckyNumberManager luckyNumberManager;
    public PawnMovementTest pawnMovementTest;

    [Header("Lucky Number Panel")]
    public GameObject luckyNumberPanel;
    public TMP_Text playerNameText;
    public TMP_Text playerLuckyNumberText;
    public Button playerRollButton;

    [Header("Main HUD")]
    public TMP_Text hudPlayerLuckyNumberText;
    public TMP_Text hudAILuckyNumberText;

    [Header("Timing")]
    public float playerRevealDelay = 1f;
    public float aiRevealDelay = 1.5f;
    public float closePanelDelay = 2f;

    private bool hasStarted = false;

    private void Start()
    {
        if (!CheckReferences())
        {
            enabled = false;
            return;
        }

        if (pawnMovementTest.luckyNumberManager != luckyNumberManager)
        {
            Debug.LogError(
                "LuckyNumberUI and PawnMovementTest are using different LuckyNumberManager instances!"
            );

            enabled = false;
            return;
        }

        luckyNumberPanel.SetActive(true);

        playerNameText.text = "PLAYER";
        playerLuckyNumberText.text = "?";

        hudPlayerLuckyNumberText.text = "?";
        hudAILuckyNumberText.text = "?";

        playerRollButton.interactable = true;
        playerRollButton.onClick.AddListener(OnPlayerRollClicked);
    }

    private bool CheckReferences()
    {
        if (luckyNumberManager == null ||
            pawnMovementTest == null ||
            luckyNumberPanel == null ||
            playerNameText == null ||
            playerLuckyNumberText == null ||
            playerRollButton == null ||
            hudPlayerLuckyNumberText == null ||
            hudAILuckyNumberText == null)
        {
            Debug.LogError(
                "LuckyNumberUI: One or more Inspector references are missing!"
            );

            return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        if (playerRollButton != null)
        {
            playerRollButton.onClick.RemoveListener(OnPlayerRollClicked);
        }
    }

    public void OnPlayerRollClicked()
    {
        if (hasStarted)
            return;

        hasStarted = true;
        playerRollButton.interactable = false;

        StartCoroutine(RevealLuckyNumbers());
    }

    private IEnumerator RevealLuckyNumbers()
    {
        int playerNumber =
            luckyNumberManager.GeneratePlayerLuckyNumber();

        playerNameText.text = "PLAYER";
        playerLuckyNumberText.text = playerNumber.ToString();

        hudPlayerLuckyNumberText.text =
            "Lucky Number: " + playerNumber.ToString("00");

        yield return new WaitForSeconds(playerRevealDelay);

        playerNameText.text = "AI OPPONENT";
        playerLuckyNumberText.text = "...";

        yield return new WaitForSeconds(aiRevealDelay);

        int aiNumber =
            luckyNumberManager.GenerateAILuckyNumber();

        playerLuckyNumberText.text = aiNumber.ToString();

        hudAILuckyNumberText.text =
            "Lucky Number: " + aiNumber.ToString("00");

        yield return new WaitForSeconds(closePanelDelay);

        luckyNumberPanel.SetActive(false);

        pawnMovementTest.BeginOpeningUnlockPhase();
    }
}
