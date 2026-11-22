
using UnityEngine;

public class LuckyNumberManager : MonoBehaviour
{
    public int playerLuckyNumber { get; private set; }
    public int aiLuckyNumber { get; private set; }

    public bool playerUnlocked { get; private set; }
    public bool aiUnlocked { get; private set; }

    private int playerUnlockAttempt;
    private int aiUnlockAttempt;

    private int playerOpeningRolls;
    private int aiOpeningRolls;

    public void InitializeLuckyNumbers()
    {
        // Reset Lucky Numbers.
        playerLuckyNumber = 0;
        aiLuckyNumber = 0;

        // Choose the opening attempt on which a match is forced.
        // Random.Range(1, 6) returns a value from 1 to 5.
        playerUnlockAttempt = Random.Range(1, 6);
        aiUnlockAttempt = Random.Range(1, 6);

        // Reset pawn unlock states.
        playerUnlocked = false;
        aiUnlocked = false;

        // Reset opening roll counters.
        playerOpeningRolls = 0;
        aiOpeningRolls = 0;

        Debug.Log("Lucky Number system initialized.");
        Debug.Log("Player forced-match attempt: " + playerUnlockAttempt);
        Debug.Log("AI forced-match attempt: " + aiUnlockAttempt);
    }

    public int GeneratePlayerLuckyNumber()
    {
        playerLuckyNumber = Random.Range(2, 13);

        Debug.Log("PLAYER LUCKY NUMBER: " + playerLuckyNumber);

        return playerLuckyNumber;
    }

    public int GenerateAILuckyNumber()
    {
        aiLuckyNumber = Random.Range(2, 13);

        Debug.Log("AI LUCKY NUMBER: " + aiLuckyNumber);

        return aiLuckyNumber;
    }

    // Check whether the Player's next opening roll
    // should be a guaranteed matching roll.
    public bool ShouldForcePlayerMatch()
    {
        return !playerUnlocked &&
               playerOpeningRolls + 1 >= playerUnlockAttempt;
    }

    // Check whether the AI's next opening roll
    // should be a guaranteed matching roll.
    public bool ShouldForceAIMatch()
    {
        return !aiUnlocked &&
               aiOpeningRolls + 1 >= aiUnlockAttempt;
    }

    public void RecordPlayerOpeningRoll(int total)
    {
        // Do not process another opening roll after unlocking.
        if (playerUnlocked)
            return;

        playerOpeningRolls++;

        Debug.Log("========== PLAYER OPENING ROLL ==========");
        Debug.Log("Dice Total: " + total);
        Debug.Log("Player Lucky Number: " + playerLuckyNumber);
        Debug.Log("Opening Attempt: " + playerOpeningRolls);

        // Only the sum of both dice can unlock the Player.
        if (total == playerLuckyNumber)
        {
            playerUnlocked = true;

            Debug.Log("PLAYER PAWN UNLOCKED!");
            Debug.Log("Player can now take a normal gameplay turn.");
        }
        else
        {
            Debug.Log("PLAYER PAWN STILL LOCKED!");
            Debug.Log("Player must wait for another opening turn.");
        }

        Debug.Log("==========================================");
    }

    public void RecordAIOpeningRoll(int total)
    {
        // Do not process another opening roll after unlocking.
        if (aiUnlocked)
            return;

        aiOpeningRolls++;

        Debug.Log("========== AI OPENING ROLL ==========");
        Debug.Log("Dice Total: " + total);
        Debug.Log("AI Lucky Number: " + aiLuckyNumber);
        Debug.Log("Opening Attempt: " + aiOpeningRolls);

        // Only the sum of both dice can unlock the AI.
        if (total == aiLuckyNumber)
        {
            aiUnlocked = true;

            Debug.Log("AI PAWN UNLOCKED!");
            Debug.Log("AI can now take a normal gameplay turn.");
        }
        else
        {
            Debug.Log("AI PAWN STILL LOCKED!");
            Debug.Log("AI must wait for another opening turn.");
        }

        Debug.Log("=====================================");
    }

    public bool BothPawnsUnlocked()
    {
        return playerUnlocked && aiUnlocked;
    }
}
