using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class LeaderController : Player
{
    [Header("References")]
    public Rigidbody rb;
    public FollowerController follower;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detachedSpeedMultiplier = 1.6f; // when follower is ejecté/detached

    [Header("Ejection / Charge")]
    public float chargeMaxTime = 1.2f; // temps max de charge (s)
    public float minEjectDistance = 1.5f; // distance minimale de l'éjection
    public float maxEjectDistance = 6f;   // distance maximale de l'éjection
    public AnimationCurve chargeToDistance = AnimationCurve.Linear(0f, 0f, 1f, 1f); // map normalized charge -> 0..1 distance

    [Header("Indicator")]
    public GameObject landingIndicatorPrefab; // assign a small sprite/quad to show landing spot
    GameObject landingIndicatorInstance;

    [Header("Input (Input System)")]
    public InputActionReference moveAction; // Vector2
    public InputActionReference eAction; // Button

    // runtime
    float chargeTimer = 0f;
    bool isCharging = false;
    bool prevEHeld = false;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (eAction != null) eAction.action.Enable();
    }
    void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (eAction != null) eAction.action.Disable();
    }

    Vector2 ReadMoveInput()
    {
        if (moveAction != null && moveAction.action != null)
            return moveAction.action.ReadValue<Vector2>();
        return Vector2.zero;
    }

    bool ReadEHeld()
    {
        if (eAction != null && eAction.action != null)
        {
            // For button actions returning float: >0 is held
            float val = 0f;
            try { val = eAction.action.ReadValue<float>(); } catch { /* ignore */ }
            if (val > 0.1f) return true;
        }

        if (Keyboard.current != null) return Keyboard.current.eKey.isPressed;
        return Input.GetKey(KeyCode.E);
    }

    void FixedUpdate()
    {
        Vector2 move2 = ReadMoveInput();
        Vector3 movement = new Vector3(move2.x, 0f, move2.y);

        // speed depends on follower state
        float speed = moveSpeed;
        if (follower != null && (follower.CurrentState == FollowerController.State.Ejected || follower.CurrentState == FollowerController.State.Detached))
            speed *= detachedSpeedMultiplier;

        Vector3 targetPos = rb.position + movement * speed * Time.fixedDeltaTime;
       
        // --- Clamp leader pos to dancefloor (optionnel margin pour ne pas coller au bord) ---
        float leaderMargin = 0.2f; // tweak : marge pour que le leader n'aille pas tout au bord
        targetPos = ClampPositionToDancefloor(targetPos, leaderMargin);
        rb.MovePosition(targetPos);

        // ---- Charge & Eject logic ----
        bool eHeld = ReadEHeld();
        bool ePressedThisFrame = !prevEHeld && eHeld;
        bool eReleasedThisFrame = prevEHeld && !eHeld;
        prevEHeld = eHeld;

        if (eHeld && follower != null && follower.CurrentState == FollowerController.State.Solidaire)
        {
            // start or continue charging
            if (!isCharging)
            {
                isCharging = true;
                chargeTimer = 0f;
            }
            chargeTimer += Time.fixedDeltaTime;
            chargeTimer = Mathf.Min(chargeTimer, chargeMaxTime);

            // compute normalized charge 0..1
            float t = Mathf.Clamp01(chargeTimer / chargeMaxTime);
            float lerp = chargeToDistance.Evaluate(t); // 0..1 curve

            // compute landing point: direction from leader toward follower (current orbit direction)
            Vector3 dir = (follower.transform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
            dir = dir.normalized;

            float targetDistance = Mathf.Lerp(minEjectDistance, maxEjectDistance, lerp);
            Vector3 landingPoint = transform.position + dir * targetDistance;

            // show landing indicator (follower handles indicator visuals)
            if (follower != null) follower.ShowLandingIndicator(landingPoint);
        }
        else if (!eHeld && isCharging)
        {
            // release: perform the eject (only when release and were charging)
            isCharging = false;

            // compute final charge value and landing point one last time
            float t = Mathf.Clamp01(chargeTimer / chargeMaxTime);
            float lerp = chargeToDistance.Evaluate(t);
            Vector3 dir = (follower.transform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
            dir = dir.normalized;
            float targetDistance = Mathf.Lerp(minEjectDistance, maxEjectDistance, lerp);
            Vector3 landingPoint = transform.position + dir * targetDistance;

            // tell follower to eject towards landingPoint
            if (follower.CurrentState == FollowerController.State.Solidaire)
            {
                // pass the landing point and the chosen distance (used to compute force)
                follower.StartEject(landingPoint, targetDistance);
            }

            // hide indicator
            if (follower != null) follower.HideLandingIndicator();

            chargeTimer = 0f;
        }
        else
        {
            // not charging: ensure indicator hidden
            if (!isCharging && follower != null)
                follower.HideLandingIndicator();
        }
    }

    // debug draw leader direction
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}
