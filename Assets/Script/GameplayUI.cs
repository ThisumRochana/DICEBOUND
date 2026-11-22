using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    private Dictionary<GameObject, Vector2> arrowStartPositions =
    new Dictionary<GameObject, Vector2>();

    [Header("Dice Choice Hints")]
    public GameObject diceAHint;
    public GameObject diceBHint;
    public GameObject bothDiceHint;

    [Header("Dice Click Hint")]
    public GameObject diceClickHint;

    [Header("Player Marker")]
    public RectTransform playerMarker;
    public Transform playerPawn;

    [Header("Notification UI")]
    public CanvasGroup notificationGroup;
    public TMP_Text notificationText;

    private Coroutine notificationCoroutine;

    [Header("Game References")]
    public PawnMovementTest pawnMovementTest;
    public DiceController diceController;

    [Header("Player Dice UI")]
    public Image playerDiceAImage;
    public Image playerDiceBImage;
    public Button playerRollDiceButton;

    [Header("Player Dice Choice UI")]
    public Button playerDiceAButton;
    public Button playerDiceBButton;
    public Button playerBothDiceButton;

    public TMP_Text playerDiceAButtonText;
    public TMP_Text playerDiceBButtonText;
    public TMP_Text playerBothDiceButtonText;

    [Header("AI Dice UI")]
    public Image aiDiceAImage;
    public Image aiDiceBImage;
    public Button aiRollDiceButton;

    [Header("AI Dice Choice UI")]
    public Button aiDiceAButton;
    public Button aiDiceBButton;
    public Button aiBothDiceButton;

    public TMP_Text aiDiceAButtonText;
    public TMP_Text aiDiceBButtonText;
    public TMP_Text aiBothDiceButtonText;

    [Header("Dice Sprites")]
    public Sprite[] diceSprites;

    [Header("Turn UI")]
    public TMP_Text turnText;

    private void Awake()
    {
        if (playerRollDiceButton != null)
            playerRollDiceButton.onClick.AddListener(OnPlayerRollClicked);

        if (playerDiceAButton != null)
            playerDiceAButton.onClick.AddListener(OnPlayerDiceAClicked);

        if (playerDiceBButton != null)
            playerDiceBButton.onClick.AddListener(OnPlayerDiceBClicked);

        if (playerBothDiceButton != null)
            playerBothDiceButton.onClick.AddListener(OnPlayerBothDiceClicked);
    }

    private void Start()
    {
        Debug.Log("========== GAMEPLAY UI CHECK ==========");

        if (pawnMovementTest == null)
            Debug.LogError("GameplayUI: PawnMovementTest is NOT assigned!");

        if (diceController == null)
            Debug.LogError("GameplayUI: DiceController is NOT assigned!");

        CheckTextReferences();
        CheckButtonReferences();

        SetPlayerChoiceValues(0, 0);
        SetAIChoiceValues(0, 0);

        UpdatePlayerDiceButtons(false, false, false);
        UpdateRollButton(false);

        if (aiRollDiceButton != null)
            aiRollDiceButton.interactable = false;

        if (aiDiceAButton != null)
            aiDiceAButton.interactable = false;

        if (aiDiceBButton != null)
            aiDiceBButton.interactable = false;

        if (aiBothDiceButton != null)
            aiBothDiceButton.interactable = false;

        Debug.Log("========== GAMEPLAY UI READY ==========");
    }

    private void Update()
    {
        UpdatePlayerMarkerPosition();
        AnimateDiceClickHint();
    }

    private void OnDestroy()
    {
        if (playerRollDiceButton != null)
            playerRollDiceButton.onClick.RemoveListener(OnPlayerRollClicked);

        if (playerDiceAButton != null)
            playerDiceAButton.onClick.RemoveListener(OnPlayerDiceAClicked);

        if (playerDiceBButton != null)
            playerDiceBButton.onClick.RemoveListener(OnPlayerDiceBClicked);

        if (playerBothDiceButton != null)
            playerBothDiceButton.onClick.RemoveListener(OnPlayerBothDiceClicked);
    }

    // PLAYER BUTTON EVENTS

    private void OnPlayerRollClicked()
    {
        Debug.Log("MOUSE / TOUCH BUTTON CLICK RECEIVED!");

        if (pawnMovementTest == null)
        {
            Debug.LogError("PawnMovementTest reference is missing!");
            return;
        }

        pawnMovementTest.PlayerRollButtonPressed();
    }

    private void OnPlayerDiceAClicked()
    {
        Debug.Log("PLAYER SELECTED DICE A!");

        if (pawnMovementTest != null)
            pawnMovementTest.PlayerChooseDice(1);
    }

    private void OnPlayerDiceBClicked()
    {
        Debug.Log("PLAYER SELECTED DICE B!");

        if (pawnMovementTest != null)
            pawnMovementTest.PlayerChooseDice(2);
    }

    private void OnPlayerBothDiceClicked()
    {
        Debug.Log("PLAYER SELECTED BOTH DICE!");

        if (pawnMovementTest != null)
            pawnMovementTest.PlayerChooseDice(3);
    }

    // PLAYER DICE DISPLAY

    public void UpdatePlayerDice()
    {
        if (diceController == null)
        {
            Debug.LogError("PLAYER UI: DiceController is missing!");
            return;
        }

        Debug.Log("========== PLAYER DICE UI DEBUG ==========");
        Debug.Log("Actual Dice A: " + diceController.diceA);
        Debug.Log("Actual Dice B: " + diceController.diceB);
        Debug.Log("Actual Total: " +
                  (diceController.diceA + diceController.diceB));

        if (!ValidateDiceValues())
            return;

        if (playerDiceAImage != null)
        {
            playerDiceAImage.sprite =
                diceSprites[diceController.diceA - 1];
        }

        if (playerDiceBImage != null)
        {
            playerDiceBImage.sprite =
                diceSprites[diceController.diceB - 1];
        }

        Debug.Log("PLAYER DICE UI UPDATED!");
        Debug.Log("==========================================");
    }

    // AI DICE DISPLAY

    public void UpdateAIDice()
    {
        if (diceController == null || !ValidateDiceValues())
            return;

        if (aiDiceAImage != null)
            aiDiceAImage.sprite = diceSprites[diceController.diceA - 1];

        if (aiDiceBImage != null)
            aiDiceBImage.sprite = diceSprites[diceController.diceB - 1];

        Debug.Log("AI DICE DISPLAY UPDATED!");
    }

    // PLAYER CHOICE VALUES

    public void SetPlayerChoiceValues(int diceA, int diceB)
    {
        if (playerDiceAButtonText != null)
            playerDiceAButtonText.text = diceA.ToString();

        if (playerDiceBButtonText != null)
            playerDiceBButtonText.text = diceB.ToString();

        if (playerBothDiceButtonText != null)
            playerBothDiceButtonText.text = (diceA + diceB).ToString();

        Debug.Log("PLAYER CHOICE VALUES: " +
                  diceA + ", " + diceB + ", " + (diceA + diceB));
    }

    // AI CHOICE VALUES

    public void SetAIChoiceValues(int diceA, int diceB)
    {
        if (aiDiceAButtonText != null)
            aiDiceAButtonText.text = diceA.ToString();

        if (aiDiceBButtonText != null)
            aiDiceBButtonText.text = diceB.ToString();

        if (aiBothDiceButtonText != null)
            aiBothDiceButtonText.text = (diceA + diceB).ToString();

        Debug.Log("AI CHOICE VALUES: " +
                  diceA + ", " + diceB + ", " + (diceA + diceB));
    }

    // PLAYER BUTTON CONTROL

    public void UpdatePlayerDiceButtons(
    bool canChooseA,
    bool canChooseB,
    bool canChooseBoth)
    {
        if (playerDiceAButton != null)
            playerDiceAButton.interactable = canChooseA;

        if (playerDiceBButton != null)
            playerDiceBButton.interactable = canChooseB;

        if (playerBothDiceButton != null)
            playerBothDiceButton.interactable = canChooseBoth;

        if (diceAHint != null)
            diceAHint.SetActive(canChooseA);

        if (diceBHint != null)
            diceBHint.SetActive(canChooseB);

        if (bothDiceHint != null)
            bothDiceHint.SetActive(canChooseBoth);
    }

    public void UpdateRollButton(bool canRoll)
    {
        if (playerRollDiceButton != null)
            playerRollDiceButton.interactable = canRoll;
        diceClickHint.SetActive(canRoll);
    }

    // VALIDATION

    private bool ValidateDiceValues()
    {
        if (diceSprites == null || diceSprites.Length < 6)
        {
            Debug.LogError("GameplayUI: Assign all six dice sprites!");
            return false;
        }

        if (diceController.diceA < 1 || diceController.diceA > 6 ||
            diceController.diceB < 1 || diceController.diceB > 6)
        {
            Debug.LogError("GameplayUI: Invalid dice values!");
            return false;
        }

        return true;
    }

    private void CheckTextReferences()
    {
        Debug.Log("PLAYER TEXT REFERENCES:");
        Debug.Log("A: " + (playerDiceAButtonText != null ? playerDiceAButtonText.name : "MISSING"));
        Debug.Log("B: " + (playerDiceBButtonText != null ? playerDiceBButtonText.name : "MISSING"));
        Debug.Log("Both: " + (playerBothDiceButtonText != null ? playerBothDiceButtonText.name : "MISSING"));

        Debug.Log("AI TEXT REFERENCES:");
        Debug.Log("A: " + (aiDiceAButtonText != null ? aiDiceAButtonText.name : "MISSING"));
        Debug.Log("B: " + (aiDiceBButtonText != null ? aiDiceBButtonText.name : "MISSING"));
        Debug.Log("Both: " + (aiBothDiceButtonText != null ? aiBothDiceButtonText.name : "MISSING"));

        if (playerDiceAButtonText == aiDiceAButtonText ||
            playerDiceBButtonText == aiDiceBButtonText ||
            playerBothDiceButtonText == aiBothDiceButtonText ||
            playerDiceAButtonText == aiDiceBButtonText ||
            playerDiceAButtonText == aiBothDiceButtonText ||
            playerDiceBButtonText == aiDiceAButtonText ||
            playerDiceBButtonText == aiBothDiceButtonText ||
            playerBothDiceButtonText == aiDiceAButtonText ||
            playerBothDiceButtonText == aiDiceBButtonText)
        {
            Debug.LogError("IMPORTANT: A Player TMP reference is connected to an AI TMP reference!");
        }
        else
        {
            Debug.Log("Player and AI TMP references are separate.");
        }
    }

    private void CheckButtonReferences()
    {
        if (playerRollDiceButton == aiRollDiceButton ||
            playerDiceAButton == aiDiceAButton ||
            playerDiceBButton == aiDiceBButton ||
            playerBothDiceButton == aiBothDiceButton)
        {
            Debug.LogError("IMPORTANT: A Player button reference may be connected to an AI button!");
        }
    }

    // TURN DISPLAY

    public void ShowPlayerTurn()
    {
        if (turnText != null)
            turnText.text = "YOUR TURN";
    }

    public void ShowAITurn()
    {
        if (turnText != null)
            turnText.text = "AI TURN";
    }

    public void ShowExtraTurn(bool isPlayer)
    {
        if (turnText == null)
            return;

        if (isPlayer)
            turnText.text = "EXTRA TURN PLAYER!";
        else
            turnText.text = "EXTRA TURN AI!";
    }

    // NOTIFICATION SYSTEM

    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationGroup == null || notificationText == null)
            return;

        // Stop previous notification
        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        notificationCoroutine = StartCoroutine(
            NotificationRoutine(message, duration)
        );
    }

    private IEnumerator NotificationRoutine(string message, float duration)
    {
        notificationText.text = message;

        notificationGroup.gameObject.SetActive(true);
        notificationGroup.alpha = 0f;

        // Fade In
        float fadeDuration = 0.25f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            notificationGroup.alpha = Mathf.Clamp01(
                timer / fadeDuration
            );

            yield return null;
        }

        notificationGroup.alpha = 1f;

        // Display Message
        yield return new WaitForSecondsRealtime(duration);

        // Fade Out
        timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            notificationGroup.alpha = 1f - Mathf.Clamp01(
                timer / fadeDuration
            );

            yield return null;
        }

        notificationGroup.alpha = 0f;
        notificationGroup.gameObject.SetActive(false);

        notificationCoroutine = null;
    }

    public void TestNotification()
    {
        ShowNotification("TEST NOTIFICATION!");
    }

    private void UpdatePlayerMarkerPosition()
    {
        if (playerMarker == null || playerPawn == null)
            return;

        float bobOffset = Mathf.Sin(Time.time * 4f) * 0.25f;

        Vector3 worldPosition = playerPawn.position
                              + Vector3.up * (3.5f + bobOffset);

        playerMarker.position = Camera.main.WorldToScreenPoint(worldPosition);
    }

    private void AnimateDiceClickHint()
    {
        if (diceClickHint == null || !diceClickHint.activeSelf)
            return;

        float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.2f;

        diceClickHint.transform.localScale = Vector3.one * pulse;
    }


    private void AnimateChooseHints()
    {
        AnimateArrow(diceAHint);
        AnimateArrow(diceBHint);
        AnimateArrow(bothDiceHint);
    }

    private void AnimateArrow(GameObject arrow)
    {
        if (arrow == null || !arrow.activeSelf)
            return;

        RectTransform rect = arrow.GetComponent<RectTransform>();

        if (rect == null)
            return;

        float offset = Mathf.Sin(Time.time * 3f) * 8f;

        rect.anchoredPosition = new Vector2(
            rect.anchoredPosition.x,
            arrowStartPositions[arrow].y + offset
        );
    }
}
