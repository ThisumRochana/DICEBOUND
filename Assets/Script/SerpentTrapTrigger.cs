using UnityEngine;

public class SerpentTrapTrigger : MonoBehaviour
{
    public int waypointIndex;

    private bool isTriggered = false;

    public bool IsAvailable => !isTriggered;

    public bool IsAtWaypoint(int index)
    {
        return IsAvailable && waypointIndex == index;
    }

    public bool TryTriggerAt(int currentWaypointIndex, string pawnName)
    {
        if (!IsAvailable)
            return false;

        if (currentWaypointIndex != waypointIndex)
            return false;

        isTriggered = true;

        Debug.Log(pawnName + " LANDED ON SERPENT TRAP!");

        return true;
    }
}