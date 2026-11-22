
using UnityEngine;
using System.Collections;

public class PawnController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Rotation")]
    public float rotationSpeed = 360f;
    public float modelForwardOffset = 0f;
    public float turnLookAheadDistance = 1.5f;

    private Animator characterAnimator;

    private void Awake()
    {
        characterAnimator = GetComponentInChildren<Animator>(true);

        if (characterAnimator == null)
        {
            Debug.LogError("No Animator found on the pawn character!");
        }
    }

    // Existing movement method (kept for compatibility)
    public IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        yield return MoveToPosition(targetPosition, null);
    }

    // Movement with the following waypoint for corner anticipation
    public IEnumerator MoveToPosition(
        Vector3 targetPosition,
        Vector3? nextWaypointPosition)
    {
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("IsMoving", true);
        }

        while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
        {
            Vector3 incomingDirection =
                targetPosition - transform.position;

            incomingDirection.y = 0f;

            Vector3 desiredDirection = incomingDirection.normalized;

            // Anticipate the next corner
            if (nextWaypointPosition.HasValue)
            {
                Vector3 outgoingDirection =
                    nextWaypointPosition.Value - targetPosition;

                outgoingDirection.y = 0f;

                if (outgoingDirection.sqrMagnitude > 0.001f &&
                    incomingDirection.sqrMagnitude > 0.001f)
                {
                    float distance = Vector3.Distance(
                        transform.position,
                        targetPosition
                    );

                    float turnAmount = 1f - Mathf.Clamp01(
                        distance / turnLookAheadDistance
                    );

                    desiredDirection = Vector3.Slerp(
                        incomingDirection.normalized,
                        outgoingDirection.normalized,
                        turnAmount
                    ).normalized;
                }
            }

            // Apply character forward correction
            if (desiredDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(desiredDirection) *
                    Quaternion.Euler(0f, modelForwardOffset, 0f);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            // Move towards current waypoint
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;

        if (characterAnimator != null)
        {
            characterAnimator.SetBool("IsMoving", false);
        }
    }
}
