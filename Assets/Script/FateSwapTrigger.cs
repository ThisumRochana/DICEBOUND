using UnityEngine;

public class FateSwapTrigger : MonoBehaviour
{
    public int waypointIndex;

    private bool isCollected = false;

    public bool IsAvailable => !isCollected;

    public bool IsAtWaypoint(int index)
    {
        return IsAvailable && waypointIndex == index;
    }

    public bool TryCollectAt(int currentWaypointIndex, string pawnName)
    {
        if (!IsAvailable)
            return false;

        if (currentWaypointIndex != waypointIndex)
            return false;

        isCollected = true;

        Debug.Log(pawnName + " COLLECTED FATE SWAP!");

        Destroy(gameObject);

        return true;
    }
}