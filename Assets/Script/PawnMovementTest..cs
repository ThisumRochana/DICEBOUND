
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PawnMovementTest : MonoBehaviour
{
    private bool playerSkipNextTurn = false;
    private bool aiSkipNextTurn = false;

    private bool playerSkipElevatorAtStart = false;
    private bool aiSkipElevatorAtStart = false;

    private int playerExtraTurns = 0;
    private int aiExtraTurns = 0;

    private int playerCurseMovesRemaining = 0;
    private int aiCurseMovesRemaining = 0;

    private bool openingAITurnStarted = false;

    // =====================================
    // GAME REFERENCES
    // =====================================

    [Header("Game References")]
    public PawnController playerPawn;
    public PawnController aiPawn;

    public TurnManager turnManager;
    public DiceController diceController;
    public LuckyNumberManager luckyNumberManager;

    public WaypointManager waypointManager;
    public ElevatorManager elevatorManager;

    public GameplayUI gameplayUI;

    private Transform[] waypoints;

    // =====================================
    // POWER-UP REFERENCES
    // =====================================

    [Header("Power-up References")]
    public LuckyLeafTrigger[] luckyLeaves;
    public DashPowerUpTrigger[] dashPowerUps;
    public FateSwapTrigger[] fateSwapPowerUps;

    // =====================================
    // TRAP REFERENCES
    // =====================================

    [Header("Trap References")]
    public SerpentTrapTrigger[] serpentTraps;
    public BrokenBridgeTrap[] brokenBridgeTraps;
    public CurseTrapTrigger[] curseTraps;

    // =====================================
    // SMART AI SETTINGS
    // =====================================

    [Header("Smart AI Settings")]
    public float progressWeight = 1f;
    public float captureWeight = 30f;
    public float luckyLeafWeight = 12f;
    public float dashWeight = 1.5f;
    public float serpentPenalty = 10f;
    public float brokenBridgePenalty = 8f;
    public float cursePenalty = 10f;
    public float victoryWeight = 1000f;

    public GameResultPanel gameResultPanel;

    // =====================================
    // GAME STATE
    // =====================================

    private int playerWaypointIndex = 0;
    private int aiWaypointIndex = 0;

    private bool isMoving = false;
    private bool gameFinished = false;
    private bool hasRolled = false;

    private bool aiTurnStarted = false;
    private bool luckyNumberSetupComplete = false;

    private bool playerHasMoved = false;
    private bool aiHasMoved = false;

    private bool playerIsSafe = false;
    private bool aiIsSafe = false;

    private HashSet<SafeSpaceTrigger> playerSafeSpaces =
        new HashSet<SafeSpaceTrigger>();

    private HashSet<SafeSpaceTrigger> aiSafeSpaces =
        new HashSet<SafeSpaceTrigger>();

    // =====================================
    // START
    // =====================================

    private void Start()
    {
        if (waypointManager == null)
        {
            Debug.LogError("WaypointManager is NOT assigned!");
            enabled = false;
            return;
        }

        if (turnManager == null ||
            diceController == null ||
            luckyNumberManager == null)
        {
            Debug.LogError("A required game reference is missing!");
            enabled = false;
            return;
        }

        waypoints = waypointManager.waypoints;

        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogError("Not enough waypoints found!");
            enabled = false;
            return;
        }

        if (elevatorManager == null)
        {
            Debug.LogWarning(
                "ElevatorManager is NOT assigned. Elevators will not activate."
            );
        }

        if (gameplayUI == null)
        {
            Debug.LogWarning(
                "GameplayUI is NOT assigned. UI updates will not work."
            );
        }
        else
        {
            SyncDiceUI();
        }

        Debug.Log("PawnMovementTest connected successfully!");
        Debug.Log("Total Waypoints: " + waypoints.Length);

        luckyNumberManager.InitializeLuckyNumbers();

        turnManager.currentTurn = TurnManager.Turn.Player;

        luckyNumberSetupComplete = false;

        Debug.Log("Waiting for Lucky Number setup...");
    }

    // =====================================
    // DICE UI SYNCHRONIZATION
    // =====================================

    private void SyncDiceUI()
    {
        if (gameplayUI == null)
            return;

        gameplayUI.diceController = diceController;

        Debug.Log(
            "Dice UI synchronized: " +
            (gameplayUI.diceController == diceController)
        );
    }

    // =====================================
    // TURN UI
    // =====================================

    private void UpdateTurnUI()
    {
        if (gameplayUI == null)
            return;

        if (turnManager.currentTurn == TurnManager.Turn.Player)
        {
            gameplayUI.ShowPlayerTurn();
        }
        else
        {
            gameplayUI.ShowAITurn();
        }
    }

    // =====================================
    // LUCKY NUMBER SETUP COMPLETE
    // =====================================

    public void BeginOpeningUnlockPhase()
    {
        Debug.Log("========== OPENING PHASE DIAGNOSTIC ==========");

        Debug.Log("PawnMovementTest LuckyNumberManager ID: " +
            luckyNumberManager.GetInstanceID());

        Debug.Log("Player Lucky Number: " +
            luckyNumberManager.playerLuckyNumber);

        Debug.Log("AI Lucky Number: " +
            luckyNumberManager.aiLuckyNumber);

        Debug.Log("Player Unlocked: " +
            luckyNumberManager.playerUnlocked);

        Debug.Log("AI Unlocked: " +
            luckyNumberManager.aiUnlocked);

        Debug.Log("==============================================");

        if (luckyNumberSetupComplete)
            return;

        luckyNumberSetupComplete = true;

        turnManager.currentTurn = TurnManager.Turn.Player;
        UpdateTurnUI();

        Debug.Log("LUCKY NUMBERS REVEALED!");
        Debug.Log("OPENING UNLOCK PHASE STARTED!");
    }

    // =====================================
    // PLAYER ROLL BUTTON
    // =====================================

    public void PlayerRollButtonPressed()
    {
        Debug.Log("PLAYER ROLL BUTTON PRESSED!");

        if (gameFinished || isMoving || !luckyNumberSetupComplete)
            return;

        if (turnManager.currentTurn != TurnManager.Turn.Player)
            return;

        if (hasRolled)
            return;

        // =================================
        // OPENING UNLOCK ROLL
        // =================================

        Debug.Log("========== PLAYER ROLL DEBUG ==========");
        Debug.Log("Player Lucky Number: " + luckyNumberManager.playerLuckyNumber);
        Debug.Log("Player Unlocked: " + luckyNumberManager.playerUnlocked);
        Debug.Log("Manager Instance ID: " + luckyNumberManager.GetInstanceID());
        Debug.Log("=======================================");
        if (!luckyNumberManager.playerUnlocked)
        {
            Debug.Log("PLAYER OPENING ROLL!");

            int luckyNumber = luckyNumberManager.playerLuckyNumber;

            bool forceMatch =
                luckyNumberManager.ShouldForcePlayerMatch();

            diceController.RollDiceForUnlock(
                luckyNumber,
                forceMatch
            );
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDiceRoll();
            }

            int diceA = diceController.diceA;
            int diceB = diceController.diceB;
            int total = diceA + diceB;

            Debug.Log("========== PLAYER UNLOCK VERIFICATION ==========");
            Debug.Log("Player Lucky Number: " + luckyNumber);
            Debug.Log("Actual Dice A: " + diceA);
            Debug.Log("Actual Dice B: " + diceB);
            Debug.Log("Actual Dice Total: " + total);
            Debug.Log("Forced Match: " + forceMatch);

            if (gameplayUI != null)
            {
                SyncDiceUI();

                gameplayUI.UpdatePlayerDice();

                gameplayUI.SetPlayerChoiceValues(
                    diceA,
                    diceB
                );

                gameplayUI.UpdatePlayerDiceButtons(false, false, false);
            }

            // Only the actual sum of both dice can unlock the Player.

            luckyNumberManager.RecordPlayerOpeningRoll(total);

            if (luckyNumberManager.playerUnlocked)
            {
                Debug.Log("PLAYER UNLOCKED!");
                if (gameplayUI != null)
                {
                    gameplayUI.ShowNotification("YOUR PAWN IS UNLOCKED!");
                }
                Debug.Log("PLAYER GETS THE FIRST NORMAL GAMEPLAY TURN!");
            }
            else
            {
                Debug.Log("PLAYER IS STILL LOCKED!");

                turnManager.SwitchTurn();
                UpdateTurnUI();
            }

            return;
        }

        // =================================
        // NORMAL GAMEPLAY ROLL
        // =================================

        diceController.RollDice();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDiceRoll();
        }

        hasRolled = true;

        int normalDiceA = diceController.diceA;
        int normalDiceB = diceController.diceB;

        int normalTotal = normalDiceA + normalDiceB;

        Debug.Log("PLAYER NORMAL ROLL");
        Debug.Log("Dice A: " + normalDiceA);
        Debug.Log("Dice B: " + normalDiceB);
        Debug.Log("Total: " + normalTotal);

        if (gameplayUI != null)
        {
            SyncDiceUI();

            gameplayUI.UpdatePlayerDice();

            gameplayUI.SetPlayerChoiceValues(
                normalDiceA,
                normalDiceB
            );

            gameplayUI.UpdatePlayerDiceButtons(true, true, true);
        }

        // Lucky Number bonus.

        if (normalTotal == luckyNumberManager.playerLuckyNumber)
        {
            Debug.Log("PLAYER MATCHED LUCKY NUMBER!");

            GrantPlayerExtraTurn();
        }

        // Check available movement.

        int remainingSpaces =
            (waypoints.Length - 1) - playerWaypointIndex;

        bool validA = normalDiceA <= remainingSpaces;
        bool validB = normalDiceB <= remainingSpaces;
        bool validTotal = normalTotal <= remainingSpaces;

        if (playerCurseMovesRemaining > 0)
        {
            validA = remainingSpaces >= 1;
            validB = remainingSpaces >= 1;
            validTotal = remainingSpaces >= 1;
        }

        if (!validA && !validB && !validTotal)
        {
            Debug.Log("NO VALID MOVES! PLAYER SKIPS THE MOVE.");

            hasRolled = false;

            if (gameplayUI != null)
            {
                gameplayUI.UpdatePlayerDiceButtons(false, false, false);
            }

            if (playerExtraTurns > 0)
            {
                playerExtraTurns--;

                Debug.Log("PLAYER USES EXTRA TURN!");
            }
            else
            {
                EndPlayerTurn();
            }

            return;
        }

        if (playerCurseMovesRemaining > 0)
        {
            Debug.Log(
                "PLAYER IS CURSED! ONLY ONE WAYPOINT MOVEMENT ALLOWED."
            );
        }
        else
        {
            Debug.Log("CHOOSE DICE A, DICE B OR BOTH!");
        }
    }

    // =====================================
    // PLAYER DICE CHOICE BUTTON
    // =====================================

    public void PlayerChooseDice(int choice)
    {
        Debug.Log("PLAYER CHOSE DICE OPTION: " + choice);

        if (gameFinished || isMoving || !hasRolled)
            return;

        if (turnManager.currentTurn != TurnManager.Turn.Player)
            return;

        int spaces = 0;

        if (choice == 1)
        {
            spaces = diceController.diceA;
        }
        else if (choice == 2)
        {
            spaces = diceController.diceB;
        }
        else if (choice == 3)
        {
            spaces = diceController.diceA + diceController.diceB;
        }
        else
        {
            return;
        }

        // Curse forces one waypoint movement.

        if (playerCurseMovesRemaining > 0)
        {
            spaces = 1;
        }

        int targetIndex = playerWaypointIndex + spaces;

        // Exact finish required.

        if (targetIndex >= waypoints.Length)
        {
            Debug.Log("INVALID MOVE! CHOOSE ANOTHER DICE OPTION.");
            return;
        }

        if (gameplayUI != null)
        {
            gameplayUI.UpdatePlayerDiceButtons(false, false, false);
        }

        StartCoroutine(MovePlayer(spaces));
    }

    // =====================================
    // UPDATE
    // =====================================

    private void Update()
    {
        // Update player roll button availability.

        if (gameplayUI != null)
        {
            bool canRoll =
                !gameFinished &&
                !isMoving &&
                luckyNumberSetupComplete &&
                turnManager.currentTurn == TurnManager.Turn.Player &&
                !hasRolled;

            gameplayUI.UpdateRollButton(canRoll);
        }

        if (gameFinished || isMoving || !luckyNumberSetupComplete)
            return;

        if (turnManager.currentTurn == TurnManager.Turn.Player)
        {
            if (!luckyNumberManager.playerUnlocked)
            {
                HandlePlayerUnlockTurn();
            }
            else
            {
                HandlePlayerTurn();
            }
        }
        else
        {
            if (!luckyNumberManager.aiUnlocked)
            {
                HandleAIUnlockTurn();
            }
            else
            {
                HandleAITurn();
            }
        }
    }

    // =====================================
    // PLAYER OPENING UNLOCK
    // =====================================

    private void HandlePlayerUnlockTurn()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            PlayerRollButtonPressed();
        }
    }

    // =====================================
    // AI OPENING UNLOCK
    // =====================================

    private void HandleAIUnlockTurn()
    {
        if (!openingAITurnStarted)
        {
            StartCoroutine(PlayAIUnlockTurn());
        }
    }

    private IEnumerator PlayAIUnlockTurn()
    {
        openingAITurnStarted = true;

        Debug.Log("AI WILL ROLL IN 2 SECONDS!");

        yield return new WaitForSeconds(2f);

        Debug.Log("AI OPENING ROLL!");

        int luckyNumber = luckyNumberManager.aiLuckyNumber;

        bool forceMatch =
            luckyNumberManager.ShouldForceAIMatch();

        diceController.RollDiceForUnlock(
            luckyNumber,
            forceMatch
        );
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDiceRoll();
        }

        // Save actual dice values before updating the UI.

        int diceA = diceController.diceA;
        int diceB = diceController.diceB;
        int total = diceA + diceB;

        Debug.Log("========== AI UNLOCK VERIFICATION ==========");
        Debug.Log("AI Lucky Number: " + luckyNumber);
        Debug.Log("Actual Dice A: " + diceA);
        Debug.Log("Actual Dice B: " + diceB);
        Debug.Log("Actual Dice Total: " + total);
        Debug.Log("Forced Match: " + forceMatch);

        if (gameplayUI != null)
        {
            SyncDiceUI();

            gameplayUI.UpdateAIDice();

            gameplayUI.SetAIChoiceValues(
                diceA,
                diceB
            );
        }

        yield return new WaitForSeconds(1.5f);

        // Record the saved total, not a newly read UI value.

        luckyNumberManager.RecordAIOpeningRoll(total);

        openingAITurnStarted = false;

        if (luckyNumberManager.aiUnlocked)
        {
            Debug.Log("AI UNLOCKED!");
            if (gameplayUI != null)
            {
                gameplayUI.ShowNotification("AI PAWN UNLOCKED!");
            }
            Debug.Log("AI GETS THE FIRST NORMAL GAMEPLAY TURN!");
        }
        else
        {
            Debug.Log("AI IS STILL LOCKED!");

            turnManager.SwitchTurn();
            UpdateTurnUI();
        }
    }

    // =====================================
    // PLAYER NORMAL TURN
    // =====================================

    private void HandlePlayerTurn()
    {
        if (Keyboard.current == null)
            return;

        // Roll using R key.

        if (!hasRolled &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            PlayerRollButtonPressed();
        }

        if (!hasRolled)
            return;

        // Select dice using keyboard.

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            PlayerChooseDice(1);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            PlayerChooseDice(2);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            PlayerChooseDice(3);
        }
    }

    // =====================================
    // END PLAYER TURN
    // =====================================

    private void EndPlayerTurn()
    {
        turnManager.SwitchTurn();

        if (aiSkipNextTurn)
        {
            aiSkipNextTurn = false;

            Debug.Log("AI'S TURN SKIPPED!");

            turnManager.SwitchTurn();
        }

        UpdateTurnUI();
    }

    // =====================================
    // AI NORMAL TURN
    // =====================================

    private void HandleAITurn()
    {
        if (!aiTurnStarted)
        {
            aiTurnStarted = true;

            StartCoroutine(PlayAITurn());
        }
    }

    // =====================================
    // PLAYER MOVEMENT
    // =====================================

    private IEnumerator MovePlayer(int spaces)
    {
        bool wasCursed = playerCurseMovesRemaining > 0;

        if (wasCursed)
        {
            spaces = 1;
        }

        int targetIndex = playerWaypointIndex + spaces;

        if (targetIndex >= waypoints.Length)
        {
            Debug.Log(
                "Invalid choice! Please select another valid dice option."
            );

            yield break;
        }

        isMoving = true;

        if (playerSkipElevatorAtStart)
        {
            playerSkipElevatorAtStart = false;

            Debug.Log("PLAYER ALREADY RODE THE ELEVATOR!");
        }
        else
        {
            yield return TryRideWithOtherPawn(
                playerPawn,
                playerWaypointIndex,
                1
            );
        }

        while (playerWaypointIndex < targetIndex)
        {
            playerWaypointIndex++;

            yield return MovePawnToWaypoint(
                playerPawn,
                playerWaypointIndex,
                1,
                playerWaypointIndex < targetIndex
            );
        }

        playerHasMoved = true;

        int normalLandingIndex = playerWaypointIndex;

        yield return StartCoroutine(ApplyPlayerPowerUps());

        yield return StartCoroutine(ApplyPlayerTraps());

        yield return StartCoroutine(ApplyPlayerBrokenBridge());

        if (wasCursed)
        {
            playerCurseMovesRemaining--;

            Debug.Log(
                "PLAYER CURSE MOVEMENTS REMAINING: "
                + playerCurseMovesRemaining
            );

            if (playerCurseMovesRemaining <= 0)
            {
                Debug.Log("PLAYER CURSE HAS ENDED!");
            }
        }

        ApplyPlayerCurse(normalLandingIndex);

        // =================================
        // CAPTURE AI
        // =================================

        if (aiHasMoved && playerWaypointIndex == aiWaypointIndex)
        {
            if (aiIsSafe)
            {
                Debug.Log("AI is on a SAFE SPACE! No capture.");
            }
            else
            {
                Debug.Log("PLAYER CAPTURED AI!");
                if (gameplayUI != null)
                {
                    gameplayUI.ShowNotification("OPPONENT CAPTURED!");
                }

                aiPawn.transform.SetParent(null, true);
                aiPawn.transform.position = waypoints[0].position;

                aiWaypointIndex = 1;
                aiHasMoved = false;

                aiSkipElevatorAtStart = false;

                aiSafeSpaces.Clear();
                aiIsSafe = false;

                Debug.Log("AI returned to START!");
            }
        }

        // =================================
        // VICTORY
        // =================================

        if (playerWaypointIndex == waypoints.Length - 1)
        {
            CheckWinner("PLAYER");

            isMoving = false;

            hasRolled = false;

            if (gameplayUI != null)
            {
                gameplayUI.UpdatePlayerDiceButtons(false, false, false);
            }

            yield break;
        }

        hasRolled = false;
        isMoving = false;

        if (gameplayUI != null)
        {
            gameplayUI.UpdatePlayerDiceButtons(false, false, false);
        }

        if (playerExtraTurns > 0)
        {
            playerExtraTurns--;

            Debug.Log("PLAYER GETS AN EXTRA TURN!");

            yield break;
        }

        EndPlayerTurn();
    }

    // =====================================
    // AI MOVEMENT
    // =====================================

    private IEnumerator MoveAI(int spaces)
    {
        bool wasCursed = aiCurseMovesRemaining > 0;

        if (wasCursed)
        {
            spaces = 1;
        }

        int targetIndex = aiWaypointIndex + spaces;

        if (targetIndex >= waypoints.Length)
        {
            Debug.Log("AI cannot move beyond the final space.");
            yield break;
        }

        isMoving = true;

        if (aiSkipElevatorAtStart)
        {
            aiSkipElevatorAtStart = false;

            Debug.Log("AI ALREADY RODE THE ELEVATOR!");
        }
        else
        {
            yield return TryRideWithOtherPawn(
                aiPawn,
                aiWaypointIndex,
                1
            );
        }

        while (aiWaypointIndex < targetIndex)
        {
            aiWaypointIndex++;

            yield return MovePawnToWaypoint(
                aiPawn,
                aiWaypointIndex,
                1,
                aiWaypointIndex < targetIndex
            );
        }

        aiHasMoved = true;

        int normalLandingIndex = aiWaypointIndex;

        yield return StartCoroutine(ApplyAIPowerUps());

        yield return StartCoroutine(ApplyAITraps());

        yield return StartCoroutine(ApplyAIBrokenBridge());

        if (wasCursed)
        {
            aiCurseMovesRemaining--;

            Debug.Log(
                "AI CURSE MOVEMENTS REMAINING: "
                + aiCurseMovesRemaining
            );

            if (aiCurseMovesRemaining <= 0)
            {
                Debug.Log("AI CURSE HAS ENDED!");
            }
        }

        ApplyAICurse(normalLandingIndex);

        // =================================
        // CAPTURE PLAYER
        // =================================

        if (playerHasMoved && aiWaypointIndex == playerWaypointIndex)
        {
            if (playerIsSafe)
            {
                Debug.Log("PLAYER is on a SAFE SPACE! No capture.");
            }
            else
            {
                Debug.Log("AI CAPTURED PLAYER!");
                if (gameplayUI != null)
                {
                    gameplayUI.ShowNotification("OPPONENT CAPTURED!");
                }

                playerPawn.transform.SetParent(null, true);
                playerPawn.transform.position = waypoints[0].position;

                playerWaypointIndex = 1;
                playerHasMoved = false;

                playerSkipElevatorAtStart = false;

                playerSafeSpaces.Clear();
                playerIsSafe = false;

                Debug.Log("PLAYER returned to START!");
            }
        }

        // =================================
        // VICTORY
        // =================================

        if (aiWaypointIndex == waypoints.Length - 1)
        {
            CheckWinner("AI");

            isMoving = false;
            aiTurnStarted = false;

            yield break;
        }

        isMoving = false;
        aiTurnStarted = false;

        if (aiExtraTurns > 0)
        {
            aiExtraTurns--;

            Debug.Log("AI GETS AN EXTRA TURN!");

            yield break;
        }

        Debug.Log("AI finished. Returning to Player!");

        turnManager.SwitchTurn();

        if (playerSkipNextTurn)
        {
            playerSkipNextTurn = false;

            Debug.Log("PLAYER'S TURN SKIPPED!");

            turnManager.SwitchTurn();
        }

        UpdateTurnUI();
    }

    // =====================================
    // SMART AI DECISION MAKING
    // =====================================

    private int ChooseSmartAIMove(int diceA, int diceB)
    {
        if (aiCurseMovesRemaining > 0)
        {
            int remainingSpaces =
                (waypoints.Length - 1) - aiWaypointIndex;

            if (remainingSpaces >= 1)
            {
                Debug.Log("AI IS CURSED! FORCED MOVEMENT: 1");

                return 1;
            }

            return 0;
        }

        int[] options =
        {
            diceA,
            diceB,
            diceA + diceB
        };

        int bestMove = 0;
        float bestScore = float.MinValue;

        HashSet<int> checkedOptions = new HashSet<int>();

        foreach (int option in options)
        {
            if (!checkedOptions.Add(option))
                continue;

            if (option <= 0)
                continue;

            int targetIndex = aiWaypointIndex + option;

            if (targetIndex >= waypoints.Length)
                continue;

            float score = EvaluateAIMove(targetIndex, option);

            Debug.Log(
                "AI OPTION: " + option +
                " | DESTINATION: " + targetIndex +
                " | SCORE: " + score
            );

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = option;
            }
        }

        if (bestMove > 0)
        {
            Debug.Log(
                "SMART AI CHOSE MOVE: " + bestMove +
                " | SCORE: " + bestScore
            );
        }

        return bestMove;
    }

    // =====================================
    // EVALUATE POSSIBLE AI MOVES
    // =====================================

    private float EvaluateAIMove(int targetIndex, int moveSpaces)
    {
        float score = 0f;

        // Basic forward progress.

        score += moveSpaces * progressWeight;

        // =================================
        // LUCKY LEAF
        // =================================

        if (HasLuckyLeaf(targetIndex))
        {
            score += luckyLeafWeight;

            Debug.Log("AI IDENTIFIED LUCKY LEAF!");
        }

        // =================================
        // DASH
        // =================================

        int dashSpaces = GetAIDashBonus(targetIndex);

        int dashDestination = Mathf.Min(
            targetIndex + dashSpaces,
            waypoints.Length - 1
        );

        if (dashSpaces > 0)
        {
            score += dashSpaces * dashWeight;

            Debug.Log(
                "AI IDENTIFIED DASH! +" + dashSpaces + " SPACES."
            );
        }

        // =================================
        // FATE SWAP
        // =================================

        bool hasFateSwap = HasAIFateSwap(targetIndex);

        int expectedAIPosition = dashDestination;
        int expectedPlayerPosition = playerWaypointIndex;

        if (hasFateSwap)
        {
            expectedAIPosition = playerWaypointIndex;
            expectedPlayerPosition = dashDestination;

            float swapAdvantage =
                expectedAIPosition - expectedPlayerPosition;

            score += Mathf.Clamp(
                swapAdvantage * 0.5f,
                -15f,
                15f
            );

            Debug.Log("AI IDENTIFIED FATE SWAP!");
        }

        // =================================
        // SERPENT TRAP
        // =================================

        if (HasAISerpent(expectedAIPosition))
        {
            score -= serpentPenalty;

            expectedAIPosition = Mathf.Max(
                expectedAIPosition - 4,
                1
            );

            Debug.Log("AI IDENTIFIED SERPENT DANGER!");
        }

        // =================================
        // BROKEN BRIDGE
        // =================================

        if (HasAIBrokenBridge(expectedAIPosition))
        {
            score -= brokenBridgePenalty;

            Debug.Log("AI IDENTIFIED BROKEN BRIDGE!");
        }

        // =================================
        // CURSE TRAP
        // =================================

        if (HasAICurse(targetIndex))
        {
            score -= cursePenalty;

            Debug.Log("AI IDENTIFIED CURSE DANGER!");
        }

        // =================================
        // CAPTURE OPPORTUNITY
        // =================================

        if (playerHasMoved &&
            expectedAIPosition == expectedPlayerPosition &&
            !playerIsSafe)
        {
            score += captureWeight;

            Debug.Log("AI FOUND A POSSIBLE CAPTURE!");
        }

        // =================================
        // FINAL DESTINATION
        // =================================

        if (expectedAIPosition == waypoints.Length - 1)
        {
            score += victoryWeight;

            Debug.Log("AI IDENTIFIED A POSSIBLE WINNING MOVE!");
        }

        return score;
    }

    // =====================================
    // SMART AI BOARD INSPECTION
    // =====================================

    private bool HasLuckyLeaf(int index)
    {
        if (luckyLeaves == null)
            return false;

        foreach (LuckyLeafTrigger leaf in luckyLeaves)
        {
            if (leaf != null && leaf.IsAtWaypoint(index))
            {
                return true;
            }
        }

        return false;
    }

    private int GetAIDashBonus(int index)
    {
        int bonus = 0;

        if (dashPowerUps == null)
            return 0;

        foreach (DashPowerUpTrigger dash in dashPowerUps)
        {
            if (dash != null && dash.IsAtWaypoint(index))
            {
                bonus += dash.bonusSpaces;
            }
        }

        return bonus;
    }

    private bool HasAIFateSwap(int index)
    {
        if (fateSwapPowerUps == null)
            return false;

        foreach (FateSwapTrigger fateSwap in fateSwapPowerUps)
        {
            if (fateSwap != null &&
                fateSwap.IsAtWaypoint(index))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAISerpent(int index)
    {
        if (serpentTraps == null)
            return false;

        foreach (SerpentTrapTrigger serpent in serpentTraps)
        {
            if (serpent != null &&
                serpent.IsAtWaypoint(index))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAIBrokenBridge(int index)
    {
        if (brokenBridgeTraps == null)
            return false;

        foreach (BrokenBridgeTrap bridge in brokenBridgeTraps)
        {
            if (bridge != null &&
                bridge.IsAtWaypoint(index))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasAICurse(int index)
    {
        if (curseTraps == null)
            return false;

        foreach (CurseTrapTrigger curse in curseTraps)
        {
            if (curse != null &&
                curse.IsAtWaypoint(index))
            {
                return true;
            }
        }

        return false;
    }

    // =====================================
    // AI TURN EXECUTION
    // =====================================

    private IEnumerator PlayAITurn()
    {
        Debug.Log("SMART AI TURN STARTED!");

        Debug.Log("AI WILL ROLL IN 2 SECONDS!");

        yield return new WaitForSeconds(2f);

        diceController.RollDice();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDiceRoll();
        }

        // Save the actual dice values.

        int diceA = diceController.diceA;
        int diceB = diceController.diceB;
        int total = diceA + diceB;

        // Update AI dice UI.

        if (gameplayUI != null)
        {
            SyncDiceUI();

            gameplayUI.UpdateAIDice();

            gameplayUI.SetAIChoiceValues(
                diceA,
                diceB
            );
        }

        // Lucky Number.

        if (total == luckyNumberManager.aiLuckyNumber)
        {
            Debug.Log("AI MATCHED LUCKY NUMBER!");

            GrantAIExtraTurn();
        }

        // Preserve the existing result-display delay.

        yield return new WaitForSeconds(1.5f);

        Debug.Log("AI Dice A: " + diceA);
        Debug.Log("AI Dice B: " + diceB);
        Debug.Log("AI Dice Total: " + total);

        // Evaluate all available options.

        int bestMove = ChooseSmartAIMove(diceA, diceB);

        if (bestMove == 0)
        {
            Debug.Log("AI HAS NO VALID MOVE!");

            aiTurnStarted = false;

            if (aiExtraTurns > 0)
            {
                aiExtraTurns--;

                Debug.Log("AI USES EXTRA TURN!");

                yield return new WaitForSeconds(0.5f);

                aiTurnStarted = true;

                yield return StartCoroutine(PlayAITurn());
            }
            else
            {
                turnManager.SwitchTurn();

                if (playerSkipNextTurn)
                {
                    playerSkipNextTurn = false;

                    Debug.Log("PLAYER'S TURN SKIPPED!");

                    turnManager.SwitchTurn();
                }
                UpdateTurnUI();
            }

            yield break;
        }

        Debug.Log("AI SELECTED MOVEMENT: " + bestMove);

        yield return StartCoroutine(MoveAI(bestMove));
    }

    // =====================================
    // PLAYER POWER-UPS
    // =====================================

    private IEnumerator ApplyPlayerPowerUps()
    {
        int landingIndex = playerWaypointIndex;

        if (luckyLeaves != null)
        {
            foreach (LuckyLeafTrigger leaf in luckyLeaves)
            {
                if (leaf != null &&
                    leaf.TryCollectAt(landingIndex, "PLAYER"))
                {
                    GrantPlayerExtraTurn();
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayPowerUp1();
                    }

                    if (gameplayUI != null)
                    {
                        gameplayUI.ShowNotification("LUCKY LEAF! EXTRA TURN!");
                    }
                }
            }
        }

        int dashSpaces = 0;

        if (dashPowerUps != null)
        {
            foreach (DashPowerUpTrigger dash in dashPowerUps)
            {
                if (dash != null &&
                    dash.TryCollectAt(landingIndex, "PLAYER"))
                {
                    dashSpaces += dash.bonusSpaces;
                }
            }
        }

        bool collectedFateSwap = false;

        if (fateSwapPowerUps != null)
        {
            foreach (FateSwapTrigger fateSwap in fateSwapPowerUps)
            {
                if (fateSwap != null &&
                    fateSwap.TryCollectAt(landingIndex, "PLAYER"))
                {
                    collectedFateSwap = true;
                }
            }
        }

        // =================================
        // DASH
        // =================================

        if (dashSpaces > 0)
        {
            int dashTarget = Mathf.Min(
                playerWaypointIndex + dashSpaces,
                waypoints.Length - 1
            );
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPowerUp1();
            }

            Debug.Log("PLAYER DASHES FORWARD " + dashSpaces + " SPACES!");

            if (gameplayUI != null)
            {
                gameplayUI.ShowNotification(
                    "DASH ACTIVATED! +" + dashSpaces + " STEPS"
                );
            }

            if (playerSkipElevatorAtStart)
            {
                playerSkipElevatorAtStart = false;
            }
            else
            {
                yield return TryRideWithOtherPawn(
                    playerPawn,
                    playerWaypointIndex,
                    1
                );
            }

            while (playerWaypointIndex < dashTarget)
            {
                playerWaypointIndex++;

                yield return MovePawnToWaypoint(
                    playerPawn,
                    playerWaypointIndex,
                    1,
                    playerWaypointIndex < dashTarget
                );
            }
        }

        // =================================
        // FATE SWAP
        // =================================

        if (collectedFateSwap)
        {
            yield return StartCoroutine(SwapPawnPositions());
        }
    }

    // =====================================
    // AI POWER-UPS
    // =====================================

    private IEnumerator ApplyAIPowerUps()
    {
        int landingIndex = aiWaypointIndex;

        if (luckyLeaves != null)
        {
            foreach (LuckyLeafTrigger leaf in luckyLeaves)
            {
                if (leaf != null &&
                    leaf.TryCollectAt(landingIndex, "AI"))
                {
                    GrantAIExtraTurn();
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayPowerUp1();
                    }
                    if (gameplayUI != null)
                    {
                        gameplayUI.ShowNotification("LUCKY LEAF! EXTRA TURN AI!");
                    }
                }
            }
        }

        int dashSpaces = 0;

        if (dashPowerUps != null)
        {
            foreach (DashPowerUpTrigger dash in dashPowerUps)
            {
                if (dash != null &&
                    dash.TryCollectAt(landingIndex, "AI"))
                {
                    dashSpaces += dash.bonusSpaces;
                }
            }
        }

        bool collectedFateSwap = false;

        if (fateSwapPowerUps != null)
        {
            foreach (FateSwapTrigger fateSwap in fateSwapPowerUps)
            {
                if (fateSwap != null &&
                    fateSwap.TryCollectAt(landingIndex, "AI"))
                {
                    collectedFateSwap = true;
                }
            }
        }

        // =================================
        // DASH
        // =================================

        if (dashSpaces > 0)
        {
            int dashTarget = Mathf.Min(
                aiWaypointIndex + dashSpaces,
                waypoints.Length - 1
            );
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPowerUp1();
            }

            Debug.Log("AI DASHES FORWARD " + dashSpaces + " SPACES!");

            if (gameplayUI != null)
            {
                gameplayUI.ShowNotification(
                    "DASH ACTIVATED! +" + dashSpaces + " STEPS"
                );
            }

            if (aiSkipElevatorAtStart)
            {
                aiSkipElevatorAtStart = false;
            }
            else
            {
                yield return TryRideWithOtherPawn(
                    aiPawn,
                    aiWaypointIndex,
                    1
                );
            }

            while (aiWaypointIndex < dashTarget)
            {
                aiWaypointIndex++;

                yield return MovePawnToWaypoint(
                    aiPawn,
                    aiWaypointIndex,
                    1,
                    aiWaypointIndex < dashTarget
                );
            }
        }

        // =================================
        // FATE SWAP
        // =================================

        if (collectedFateSwap)
        {
            yield return StartCoroutine(SwapPawnPositions());
        }
    }

    // =====================================
    // FATE SWAP
    // =====================================

    private IEnumerator SwapPawnPositions()
    {
        Debug.Log("FATE SWAP ACTIVATED!");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUp2();
        }
        if (gameplayUI != null)
{
    gameplayUI.ShowNotification(
        "FATE SWAP! POSITIONS CHANGED!"
    );
}

        int oldPlayerIndex = playerWaypointIndex;
        int oldAIIndex = aiWaypointIndex;

        playerWaypointIndex = oldAIIndex;
        aiWaypointIndex = oldPlayerIndex;

        playerSkipElevatorAtStart = false;
        aiSkipElevatorAtStart = false;

        playerPawn.transform.SetParent(null, true);
        aiPawn.transform.SetParent(null, true);

        playerPawn.transform.position =
            waypoints[playerWaypointIndex].position;

        aiPawn.transform.position =
            waypoints[aiWaypointIndex].position;

        Debug.Log("PLAYER MOVED TO WAYPOINT " + playerWaypointIndex);
        Debug.Log("AI MOVED TO WAYPOINT " + aiWaypointIndex);

        Physics.SyncTransforms();

        yield return new WaitForFixedUpdate();

        Debug.Log("FATE SWAP COMPLETED!");
    }

    // =====================================
    // PLAYER TRAPS
    // =====================================

    private IEnumerator ApplyPlayerTraps()
    {
        int landingIndex = playerWaypointIndex;

        if (serpentTraps != null)
        {
            foreach (SerpentTrapTrigger serpent in serpentTraps)
            {
                if (serpent != null &&
                    serpent.TryTriggerAt(landingIndex, "PLAYER"))
                {
                    int backwardSpaces = Random.Range(3, 6);

                    int targetIndex = Mathf.Max(
                        playerWaypointIndex - backwardSpaces,
                        1
                    );

                    Debug.Log(
                        "PLAYER HIT SERPENT! MOVING BACK "
                        + backwardSpaces + " SPACES!"
                    );
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayPowerUp2();
                    }

                    if (gameplayUI != null)
                    {
                        gameplayUI.ShowNotification(
                            "SERPENT ATTACK! MOVING BACK!"
                        );
                    }

                    yield return TryRideWithOtherPawn(
                        playerPawn,
                        playerWaypointIndex,
                        -1
                    );

                    while (playerWaypointIndex > targetIndex)
                    {
                        playerWaypointIndex--;

                        yield return MovePawnToWaypoint(
                            playerPawn,
                            playerWaypointIndex,
                            -1,
                            playerWaypointIndex > targetIndex
                        );
                    }
                }
            }
        }
    }

    private IEnumerator ApplyPlayerBrokenBridge()
    {
        int landingIndex = playerWaypointIndex;

        if (brokenBridgeTraps != null)
        {
            foreach (BrokenBridgeTrap bridge in brokenBridgeTraps)
            {
                if (bridge != null &&
                    bridge.TryTriggerAt(landingIndex, "PLAYER"))
                {
                    playerSkipNextTurn = true;

                    if (gameplayUI != null)
                    {
                        gameplayUI.ShowNotification(
                            "BROKEN BRIDGE! NEXT TURN SKIPPED!"
                        );
                    }

                    Debug.Log("PLAYER WILL LOSE THE NEXT TURN!");
                }
            }
        }

        yield return null;
    }

    // =====================================
    // AI TRAPS
    // =====================================

    private IEnumerator ApplyAITraps()
    {
        int landingIndex = aiWaypointIndex;

        if (serpentTraps != null)
        {
            foreach (SerpentTrapTrigger serpent in serpentTraps)
            {
                if (serpent != null &&
                    serpent.TryTriggerAt(landingIndex, "AI"))
                {
                    int backwardSpaces = Random.Range(3, 6);

                    int targetIndex = Mathf.Max(
                        aiWaypointIndex - backwardSpaces,
                        1
                    );

                    Debug.Log(
                        "AI HIT SERPENT! MOVING BACK "
                        + backwardSpaces + " SPACES!"
                    );
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayPowerUp2();
                    }

                    if (gameplayUI != null)
                    {
                        gameplayUI.ShowNotification(
                            "SERPENT ATTACK! MOVING BACK!"
                        );
                    }

                    yield return TryRideWithOtherPawn(
                        aiPawn,
                        aiWaypointIndex,
                        -1
                    );

                    while (aiWaypointIndex > targetIndex)
                    {
                        aiWaypointIndex--;

                        yield return MovePawnToWaypoint(
                            aiPawn,
                            aiWaypointIndex,
                            -1,
                            aiWaypointIndex > targetIndex
                        );
                    }
                }
            }
        }
    }

    private IEnumerator ApplyAIBrokenBridge()
    {
        int landingIndex = aiWaypointIndex;

        if (brokenBridgeTraps != null)
        {
            foreach (BrokenBridgeTrap bridge in brokenBridgeTraps)
            {
                if (bridge != null &&
                    bridge.TryTriggerAt(landingIndex, "AI"))
                {
                    aiSkipNextTurn = true;

                    if (gameplayUI != null)
                    {
                        gameplayUI.ShowNotification(
                            "BROKEN BRIDGE! NEXT TURN SKIPPED!"
                        );
                    }

                    Debug.Log("AI WILL LOSE THE NEXT TURN!");
                }
            }
        }

        yield return null;
    }

    // =====================================
    // CURSE TRAP
    // =====================================

    private void ApplyPlayerCurse(int landingIndex)
    {
        if (curseTraps == null)
            return;

        foreach (CurseTrapTrigger curse in curseTraps)
        {
            if (curse != null &&
                curse.TryTriggerAt(landingIndex, "PLAYER"))
            {
                playerCurseMovesRemaining = 3;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPowerUp2();
                }

                if (gameplayUI != null)
                {
                    gameplayUI.ShowNotification("CURSE ACTIVATED!");
                }

                Debug.Log("PLAYER IS CURSED!");

                Debug.Log(
                    "PLAYER MUST MOVE ONLY ONE WAYPOINT FOR THE NEXT 3 MOVEMENTS!"
                );

                break;
            }
        }
    }

    private void ApplyAICurse(int landingIndex)
    {
        if (curseTraps == null)
            return;

        foreach (CurseTrapTrigger curse in curseTraps)
        {
            if (curse != null &&
                curse.TryTriggerAt(landingIndex, "AI"))
            {
                aiCurseMovesRemaining = 3;

                if (gameplayUI != null)
                {
                    gameplayUI.ShowNotification("CURSE ACTIVATED!");
                }

                Debug.Log("AI IS CURSED!");

                Debug.Log(
                    "AI MUST MOVE ONLY ONE WAYPOINT FOR THE NEXT 3 MOVEMENTS!"
                );

                break;
            }
        }
    }

    // =====================================
    // SAFE SPACE STATUS
    // =====================================

    public void UpdatePlayerSafeStatus(
        SafeSpaceTrigger safeSpace,
        bool isInside)
    {
        if (isInside)
        {
            playerSafeSpaces.Add(safeSpace);

            if (gameplayUI != null)
            {
                gameplayUI.ShowNotification(
                    "SAFE SPACE! YOU ARE PROTECTED!"
                );
            }
        }
        else
        {
            playerSafeSpaces.Remove(safeSpace);
        }

        playerIsSafe = playerSafeSpaces.Count > 0;

        Debug.Log("Player Safe Status: " + playerIsSafe);
    }

    public void UpdateAISafeStatus(
        SafeSpaceTrigger safeSpace,
        bool isInside)
    {
        if (isInside)
        {
            aiSafeSpaces.Add(safeSpace);

            if (gameplayUI != null)
            {
                gameplayUI.ShowNotification(
                    "SAFE SPACE! YOU ARE PROTECTED!"
                );
            }
        }
        else
        {
            aiSafeSpaces.Remove(safeSpace);
        }

        aiIsSafe = aiSafeSpaces.Count > 0;

        Debug.Log("AI Safe Status: " + aiIsSafe);
    }

    // =====================================
    // EXTRA TURNS
    // =====================================

    public void GrantPlayerExtraTurn()
    {
        playerExtraTurns++;

        Debug.Log("PLAYER EARNED AN EXTRA TURN!");

        if (gameplayUI != null)
            gameplayUI.ShowExtraTurn(true);
    }

    public void GrantAIExtraTurn()
    {
        aiExtraTurns++;

        Debug.Log("AI EARNED AN EXTRA TURN!");

        if (gameplayUI != null)
            gameplayUI.ShowExtraTurn(false);
    }

    // =====================================
    // WINNER
    // =====================================

    void CheckWinner(string winner)
    {
        gameFinished = true;

        Debug.Log(winner + " WINS!");

        // VICTORY / DEFEAT NOTIFICATION
        if (gameplayUI != null)
        {
            if (winner == "PLAYER")
            {
                gameplayUI.ShowNotification(
                    "VICTORY! YOU REACHED THE SUMMIT!", 4f
                );
            }
            else
            {
                gameplayUI.ShowNotification(
                    "DEFEAT! AI REACHED THE SUMMIT!", 4f
                );
            }
        }

        // SHOW GAME RESULT PANEL
        if (gameResultPanel != null)
        {
            gameResultPanel.ShowResult(winner == "PLAYER");
        }
    }

    // =====================================
    // ELEVATOR WITH BOTH PAWNS
    // =====================================

    private IEnumerator TryRideWithOtherPawn(
        PawnController movingPawn,
        int waypointIndex,
        int direction)
    {
        if (elevatorManager == null)
            yield break;

        PawnController otherPawn = null;

        if (movingPawn == playerPawn &&
            aiWaypointIndex == waypointIndex)
        {
            otherPawn = aiPawn;
        }
        else if (movingPawn == aiPawn &&
                 playerWaypointIndex == waypointIndex)
        {
            otherPawn = playerPawn;
        }

        ElevatorRideResult result = new ElevatorRideResult();

        yield return elevatorManager.TryRideAtWaypoint(
            movingPawn,
            waypointIndex,
            direction,
            waypoints,
            otherPawn,
            result
        );

        if (result.otherPawnCarried)
        {
            if (otherPawn == playerPawn)
            {
                playerSkipElevatorAtStart = true;
            }
            else if (otherPawn == aiPawn)
            {
                aiSkipElevatorAtStart = true;
            }
        }
    }

    // =====================================
    // WAYPOINT MOVEMENT WITH ELEVATOR
    // =====================================

    private IEnumerator MovePawnToWaypoint(
        PawnController pawn,
        int targetIndex,
        int direction,
        bool willContinue = false)
    {
        Vector3 targetPosition = waypoints[targetIndex].position;

        int nextIndex = targetIndex + direction;

        Vector3? nextPosition = null;

        if (nextIndex >= 0 && nextIndex < waypoints.Length)
        {
            nextPosition = waypoints[nextIndex].position;
        }

        yield return pawn.MoveToPosition(
            targetPosition,
            nextPosition
        );

        if (willContinue && elevatorManager != null)
        {
            yield return TryRideWithOtherPawn(
                pawn,
                targetIndex,
                direction
            );
        }
    }
}
