using UnityEngine;

/// <summary>
/// PANCHA: FATE'S PATH - cinematic board-game camera.
/// Fixed polished viewing angle, smooth focus following, soft turn transitions,
/// FOV-based zoom/framing, teleport-safe (Fate Swap), finish handling.
/// Does not touch gameplay: it only READS pawn positions and the current turn.
/// </summary>
[DefaultExecutionOrder(100)] // run after pawn movement scripts
[RequireComponent(typeof(Camera))]
public class PanchaCameraController : MonoBehaviour
{
    [Header("Game References")]
    public PawnMovementTest movementManager;

    [Tooltip("Optional. Drag Waypoint_100 (or a finish marker) here to auto-detect a finish.")]
    public Transform finishPoint;

    [Header("Follow")]
    [Tooltip("Seconds the camera takes to catch up with the pawn. Higher = floatier.")]
    [Min(0.01f)] public float focusSmoothTime = 0.35f;

    [Tooltip("Hard cap on camera focus speed (units/sec). Prevents whipping on long jumps.")]
    [Min(1f)] public float maxFocusSpeed = 60f;

    [Tooltip("How far ahead of the pawn the camera looks (world units).")]
    public float lookAheadDistance = 2f;

    [Tooltip("Pawn speed (units/sec) at which look-ahead reaches full distance.")]
    [Min(0.1f)] public float lookAheadFullSpeed = 2f;

    [Tooltip("Smooths the pawn's measured velocity. Higher = steadier look-ahead on turns.")]
    [Min(0.01f)] public float velocitySmoothTime = 0.3f;

    [Tooltip("Raises the focus point above the pawn's pivot (use if pawn pivot is at its feet).")]
    public float focusHeightOffset = 0f;

    [Header("Turn / Teleport Transitions")]
    [Tooltip("Slower smoothing used right after a turn change or Fate Swap.")]
    [Min(0.01f)] public float turnTransitionSmoothTime = 0.8f;

    [Tooltip("How long the slow transition smoothing lasts, then it blends back to normal.")]
    [Min(0.01f)] public float transitionDuration = 1.0f;

    [Tooltip("A single-frame pawn jump bigger than this (units) is treated as a teleport (Fate Swap).")]
    [Min(0.5f)] public float teleportDistance = 5f;

    [Header("Shared Framing (both pawns)")]
    public bool enableSharedView = true;

    [Tooltip("0 = centre between pawns, 1 = fully on the active pawn.")]
    [Range(0f, 1f)] public float sharedFocusBias = 0.6f;

    [Tooltip("Largest FOV the camera may use to fit both pawns. Beyond this, active pawn only.")]
    public float maxSharedFOV = 52f;

    [Tooltip("Multiplier on the space needed around the pawns.")]
    public float framingPadding = 1.25f;

    [Tooltip("Extra world-unit margin around pawns so they never touch the screen edge.")]
    public float framingMargin = 1f;

    [Tooltip("Degrees of hysteresis so shared view does not flicker on/off at the limit.")]
    [Min(0f)] public float sharedHysteresisDeg = 3f;

    [Min(0.01f)] public float sharedBlendSmoothTime = 0.6f;

    [Header("Camera Zoom (Field of View)")]
    public float minFOV = 36f;
    public float maxFOV = 58f;

    [Tooltip("FOV when the active pawn is standing still.")]
    public float zoomInFOV = 42f;

    [Tooltip("FOV when the active pawn is moving.")]
    public float zoomOutFOV = 50f;

    [Tooltip("Pawn speed (units/sec) at which FOV reaches zoomOutFOV.")]
    [Min(0.1f)] public float zoomFullSpeed = 2f;

    [Min(0.01f)] public float zoomSmoothTime = 0.45f;

    [Tooltip("Max FOV change per second (degrees). Guards against sudden zoom.")]
    [Min(1f)] public float maxZoomSpeed = 15f;

    [Header("Finish")]
    [Tooltip("Pawn within this distance of finishPoint triggers the finish camera.")]
    [Min(0.05f)] public float finishDetectRadius = 0.75f;
    public float finishFOV = 40f;
    [Min(0.01f)] public float finishSmoothTime = 0.8f;

    // ---- runtime state ----
    private Camera cam;
    private TurnManager turnManager;
    private Transform playerT;
    private Transform aiT;

    private Quaternion baseRotation;
    private Vector3 startOffset;      // camera position relative to focus point
    private Vector3 camRight;
    private Vector3 camUp;
    private float baseDistance;       // camera distance used for FOV framing maths

    private Vector3 focus;
    private Vector3 focusVel;

    private Transform trackedPawn;
    private Vector3 prevPawnPos;
    private Vector3 smoothedVel;
    private Vector3 velDamp;

    private float transitionTimer;

    private bool sharedActive;
    private float sharedBlend;
    private float sharedBlendVel;

    private float fovVel;

    private bool finished;
    private Transform finishPawn;

    // ------------------------------------------------------------

    private void OnValidate()
    {
        if (maxFOV < minFOV + 1f) maxFOV = minFOV + 1f;
        zoomInFOV = Mathf.Clamp(zoomInFOV, minFOV, maxFOV);
        zoomOutFOV = Mathf.Clamp(zoomOutFOV, minFOV, maxFOV);
        finishFOV = Mathf.Clamp(finishFOV, minFOV, maxFOV);
    }

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (cam == null || cam.orthographic)
        {
            Debug.LogError("PANCHA camera must use Perspective!");
            enabled = false;
            return;
        }

        if (!ResolveReferences())
        {
            Debug.LogError("PanchaCameraController: references are incomplete. " +
                           "Assign movementManager (with playerPawn, aiPawn, turnManager).");
            enabled = false;
            return;
        }

        // Preserve the authored starting angle and offset.
        baseRotation = transform.rotation;
        camRight = baseRotation * Vector3.right;
        camUp = baseRotation * Vector3.up;
        Vector3 forward = baseRotation * Vector3.forward;

        startOffset = transform.position - (playerT.position + Vector3.up * focusHeightOffset);

        baseDistance = -Vector3.Dot(startOffset, forward);
        if (baseDistance <= 0.01f)
            baseDistance = Mathf.Max(0.01f, startOffset.magnitude);

        SnapToActivePawn();
    }

    private bool ResolveReferences()
    {
        if (movementManager == null) return false;
        if (movementManager.playerPawn == null ||
            movementManager.aiPawn == null ||
            movementManager.turnManager == null) return false;

        playerT = movementManager.playerPawn.transform;
        aiT = movementManager.aiPawn.transform;
        turnManager = movementManager.turnManager;
        return true;
    }

    /// <summary>Instantly places the camera on the active pawn (no smoothing).</summary>
    public void SnapToActivePawn()
    {
        if (playerT == null || aiT == null || turnManager == null) return;

        trackedPawn = GetActivePawn();
        prevPawnPos = trackedPawn.position;
        smoothedVel = Vector3.zero;
        velDamp = Vector3.zero;
        focusVel = Vector3.zero;
        fovVel = 0f;
        sharedActive = false;
        sharedBlend = 0f;
        sharedBlendVel = 0f;
        transitionTimer = 0f;

        focus = trackedPawn.position + Vector3.up * focusHeightOffset;
        transform.position = focus + startOffset;
        transform.rotation = baseRotation;
        cam.fieldOfView = Mathf.Clamp(zoomInFOV, minFOV, maxFOV);
    }

    /// <summary>Optional: call when the game ends if you don't use finishPoint.</summary>
    public void NotifyFinish(Transform winnerPawn)
    {
        if (winnerPawn == null) return;
        BeginFinish(winnerPawn);
    }

    /// <summary>Optional: call when restarting a match.</summary>
    public void ResetCamera()
    {
        finished = false;
        finishPawn = null;
        SnapToActivePawn();
    }

    private void BeginFinish(Transform winner)
    {
        if (finished) return;
        finished = true;
        finishPawn = winner;
        transitionTimer = transitionDuration;
    }

    private Transform GetActivePawn()
    {
        return turnManager.currentTurn == TurnManager.Turn.Player ? playerT : aiT;
    }

    // ------------------------------------------------------------

    private void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Safe against pawns/managers being destroyed or unassigned at runtime.
        if (playerT == null || aiT == null || turnManager == null)
        {
            if (!ResolveReferences()) return;
        }

        CheckFinish();

        // 1. Which pawn do we follow?
        Transform active = (finished && finishPawn != null) ? finishPawn : GetActivePawn();
        Transform other = (active == playerT) ? aiT : playerT;

        if (active != trackedPawn)
        {
            // Turn changed: keep the camera where it is and glide to the new pawn.
            trackedPawn = active;
            prevPawnPos = active.position;
            smoothedVel = Vector3.zero;
            velDamp = Vector3.zero;
            transitionTimer = transitionDuration;
        }

        // 2. Measure pawn velocity (horizontal only, smoothed, teleport-safe).
        Vector3 pos = active.position;
        Vector3 delta = pos - prevPawnPos;
        prevPawnPos = pos;

        Vector3 rawVel;
        if (delta.sqrMagnitude > teleportDistance * teleportDistance)
        {
            // Fate Swap or similar jump: do not treat it as movement.
            rawVel = Vector3.zero;
            smoothedVel = Vector3.zero;
            velDamp = Vector3.zero;
            transitionTimer = transitionDuration;
        }
        else
        {
            rawVel = delta / dt;
            rawVel.y = 0f; // ignore hop/bounce animation
        }

        smoothedVel = Vector3.SmoothDamp(smoothedVel, rawVel, ref velDamp,
                                         velocitySmoothTime, Mathf.Infinity, dt);

        float speed = smoothedVel.magnitude;

        Vector3 lookAhead = Vector3.zero;
        if (!finished && speed > 0.01f)
        {
            lookAhead = (smoothedVel / speed) * lookAheadDistance *
                        Mathf.Clamp01(speed / lookAheadFullSpeed);
        }

        // 3. Active-pawn focus.
        Vector3 activeFocus = pos + Vector3.up * focusHeightOffset + lookAhead;

        // 4. Shared framing (smoothly blended, with hysteresis).
        float requiredFov = 0f;
        Vector3 sharedFocus = activeFocus;
        float sharedTarget = 0f;

        if (enableSharedView && !finished)
        {
            Vector3 a = pos + Vector3.up * focusHeightOffset;
            Vector3 b = other.position + Vector3.up * focusHeightOffset;
            sharedFocus = Vector3.Lerp(b, a, sharedFocusBias);

            requiredFov = ComputeRequiredFov(sharedFocus, a, b);

            float limit = maxSharedFOV + (sharedActive ? sharedHysteresisDeg : -sharedHysteresisDeg);
            sharedActive = requiredFov <= limit;
            sharedTarget = sharedActive ? 1f : 0f;
        }
        else
        {
            sharedActive = false;
        }

        sharedBlend = Mathf.SmoothDamp(sharedBlend, sharedTarget, ref sharedBlendVel,
                                       sharedBlendSmoothTime, Mathf.Infinity, dt);

        Vector3 focusTarget = Vector3.Lerp(activeFocus, sharedFocus, sharedBlend);

        // 5. Smooth focus movement (slower right after turn change / teleport).
        float smoothTime;
        if (finished)
        {
            smoothTime = finishSmoothTime;
        }
        else
        {
            float t = 0f;
            if (transitionTimer > 0f)
            {
                t = Mathf.SmoothStep(0f, 1f, transitionTimer / transitionDuration);
                transitionTimer = Mathf.Max(0f, transitionTimer - dt);
            }
            smoothTime = Mathf.Lerp(focusSmoothTime, turnTransitionSmoothTime, t);
        }

        focus = Vector3.SmoothDamp(focus, focusTarget, ref focusVel,
                                   smoothTime, maxFocusSpeed, dt);

        transform.position = focus + startOffset;
        transform.rotation = baseRotation; // stable angle, no spinning

        // 6. Smooth perspective zoom.
        float activeFov = Mathf.Lerp(zoomInFOV, zoomOutFOV, Mathf.Clamp01(speed / zoomFullSpeed));
        float sharedFov = Mathf.Max(activeFov, requiredFov);
        float fovTarget = Mathf.Lerp(activeFov, sharedFov, sharedBlend);

        if (finished) fovTarget = finishFOV;

        fovTarget = Mathf.Clamp(fovTarget, minFOV, maxFOV);

        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, fovTarget, ref fovVel,
                                           zoomSmoothTime, maxZoomSpeed, dt);
    }

    // ------------------------------------------------------------

    private void CheckFinish()
    {
        if (finished || finishPoint == null) return;

        float r2 = finishDetectRadius * finishDetectRadius;
        Vector3 fp = finishPoint.position;

        if ((playerT.position - fp).sqrMagnitude <= r2) BeginFinish(playerT);
        else if ((aiT.position - fp).sqrMagnitude <= r2) BeginFinish(aiT);
    }

    // FOV needed so both pawns fit around 'center' at the camera's base distance.
    private float ComputeRequiredFov(Vector3 center, Vector3 a, Vector3 b)
    {
        float half = Mathf.Max(HalfExtent(a - center), HalfExtent(b - center));
        half = half * framingPadding + framingMargin;
        return 2f * Mathf.Atan(half / baseDistance) * Mathf.Rad2Deg;
    }

    private float HalfExtent(Vector3 offset)
    {
        float v = Mathf.Abs(Vector3.Dot(offset, camUp));
        float h = Mathf.Abs(Vector3.Dot(offset, camRight)) / Mathf.Max(cam.aspect, 0.1f);
        return Mathf.Max(v, h);
    }
}