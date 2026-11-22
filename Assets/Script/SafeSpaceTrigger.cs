
using UnityEngine;

public class SafeSpaceTrigger : MonoBehaviour
{
    public PawnMovementTest pawnMovementTest;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pawnMovementTest.UpdatePlayerSafeStatus(this, true);
            Debug.Log("PLAYER ENTERED A SAFE SPACE!");
        }
        else if (other.CompareTag("AI"))
        {
            pawnMovementTest.UpdateAISafeStatus(this, true);
            Debug.Log("AI ENTERED A SAFE SPACE!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pawnMovementTest.UpdatePlayerSafeStatus(this, false);
            Debug.Log("PLAYER LEFT THE SAFE SPACE!");
        }
        else if (other.CompareTag("AI"))
        {
            pawnMovementTest.UpdateAISafeStatus(this, false);
            Debug.Log("AI LEFT THE SAFE SPACE!");
        }
    }
}
