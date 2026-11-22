
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum Turn
    {
        Player,
        AI
    }

    public Turn currentTurn = Turn.Player;

    public void SwitchTurn()
    {
        if (currentTurn == Turn.Player)
        {
            currentTurn = Turn.AI;
        }
        else
        {
            currentTurn = Turn.Player;
        }

        Debug.Log("Current Turn: " + currentTurn);
    }
}
