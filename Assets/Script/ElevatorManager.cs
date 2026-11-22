
using UnityEngine;
using System.Collections;

// Stores the result of an elevator ride
public class ElevatorRideResult
{
    public bool otherPawnCarried = false;
}

[System.Serializable]
public class ElevatorPoint
{
    [Header("Elevator Settings")]
    public int waypointIndex;

    public Transform elevatorPlatform;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [HideInInspector]
    public bool isBusy;

    [HideInInspector]
    public Vector3 homePosition;
}

public class ElevatorManager : MonoBehaviour
{
    [Header("Elevator Points")]
    public ElevatorPoint[] elevators;

    private void Start()
    {
        if (elevators == null)
            return;

        foreach (ElevatorPoint elevator in elevators)
        {
            if (elevator == null)
                continue;

            if (elevator.elevatorPlatform != null)
            {
                elevator.homePosition =
                    elevator.elevatorPlatform.position;

                elevator.isBusy = false;
            }
        }
    }

    public IEnumerator TryRideAtWaypoint(
        PawnController pawn,
        int waypointIndex,
        int direction,
        Transform[] waypoints,
        PawnController otherPawn = null,
        ElevatorRideResult result = null)
    {
        if (elevators == null || pawn == null || waypoints == null)
            yield break;

        ElevatorPoint selectedElevator = null;

        foreach (ElevatorPoint elevator in elevators)
        {
            if (elevator != null &&
                elevator.waypointIndex == waypointIndex)
            {
                selectedElevator = elevator;
                break;
            }
        }

        if (selectedElevator == null)
            yield break;

        if (selectedElevator.elevatorPlatform == null)
        {
            Debug.LogError(
                "Elevator platform missing at waypoint "
                + waypointIndex
            );

            yield break;
        }

        int nextIndex = waypointIndex + direction;

        if (nextIndex < 0 || nextIndex >= waypoints.Length)
            yield break;

        if (waypoints[waypointIndex] == null ||
            waypoints[nextIndex] == null)
        {
            Debug.LogError("Elevator waypoint reference is missing!");
            yield break;
        }

        float heightDifference =
            waypoints[nextIndex].position.y -
            waypoints[waypointIndex].position.y;

        if (Mathf.Abs(heightDifference) < 0.01f)
            yield break;

        if (selectedElevator.moveSpeed <= 0f)
        {
            Debug.LogError("Elevator Move Speed must be greater than zero!");
            yield break;
        }

        while (selectedElevator.isBusy)
        {
            yield return null;
        }

        selectedElevator.isBusy = true;

        Transform platform = selectedElevator.elevatorPlatform;

        Vector3 destination =
            selectedElevator.homePosition +
            Vector3.up * heightDifference;

        Transform originalPawnParent = pawn.transform.parent;

        Transform originalOtherParent = null;

        bool carryOtherPawn =
            otherPawn != null && otherPawn != pawn;

        if (carryOtherPawn)
        {
            originalOtherParent = otherPawn.transform.parent;
        }

        pawn.transform.SetParent(platform, true);

        if (carryOtherPawn)
        {
            otherPawn.transform.SetParent(platform, true);

            if (result != null)
            {
                result.otherPawnCarried = true;
            }
        }

        yield return MovePlatform(
            platform,
            destination,
            selectedElevator.moveSpeed
        );

        pawn.transform.SetParent(originalPawnParent, true);

        StartCoroutine(ReturnElevator(
            selectedElevator,
            otherPawn,
            originalOtherParent,
            carryOtherPawn
        ));
    }

    private IEnumerator ReturnElevator(
        ElevatorPoint elevator,
        PawnController otherPawn,
        Transform originalOtherParent,
        bool carryOtherPawn)
    {
        yield return MovePlatform(
            elevator.elevatorPlatform,
            elevator.homePosition,
            elevator.moveSpeed
        );

        if (carryOtherPawn && otherPawn != null)
        {
            otherPawn.transform.SetParent(
                originalOtherParent,
                true
            );
        }

        elevator.isBusy = false;
    }

    private IEnumerator MovePlatform(
        Transform platform,
        Vector3 targetPosition,
        float speed)
    {
        while (Vector3.Distance(
            platform.position,
            targetPosition) > 0.02f)
        {
            platform.position = Vector3.MoveTowards(
                platform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            yield return null;
        }

        platform.position = targetPosition;
    }
}
