
using UnityEngine;

public class DiceController : MonoBehaviour
{
    public int diceA;
    public int diceB;

    public void RollDice()
    {
        diceA = Random.Range(1, 7);
        diceB = Random.Range(1, 7);

        Debug.Log("Dice A: " + diceA);
        Debug.Log("Dice B: " + diceB);

        Debug.Log("Total: " + (diceA + diceB));
    }


    public void RollDiceForUnlock(int luckyNumber, bool forceMatch)
    {
        if (forceMatch)
        {
            int[] possibleDiceA = new int[6];
            int count = 0;

            for (int i = 1; i <= 6; i++)
            {
                int secondDie = luckyNumber - i;

                if (secondDie >= 1 && secondDie <= 6)
                {
                    possibleDiceA[count] = i;
                    count++;
                }
            }

            if (count > 0)
            {
                int selected = Random.Range(0, count);

                diceA = possibleDiceA[selected];
                diceB = luckyNumber - diceA;
            }
            else
            {
                diceA = Random.Range(1, 7);
                diceB = Random.Range(1, 7);

                Debug.LogWarning("No valid dice combination for this Lucky Number!");
            }
        }
        else
        {
            diceA = Random.Range(1, 7);
            diceB = Random.Range(1, 7);
        }

        Debug.Log("Opening Dice A: " + diceA);
        Debug.Log("Opening Dice B: " + diceB);
        Debug.Log("Opening Total: " + (diceA + diceB));
    }

}
