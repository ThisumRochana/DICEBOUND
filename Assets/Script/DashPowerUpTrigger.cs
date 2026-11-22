using UnityEngine;

public class DashPowerUpTrigger : MonoBehaviour
{
    public int waypointIndex;

    public int bonusSpaces = 3;

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

        Debug.Log(
            pawnName + " COLLECTED DASH! +" + bonusSpaces + " MOVEMENT!"
        );

        Destroy(gameObject);

        return true;
    }
}