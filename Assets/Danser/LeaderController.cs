using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class LeaderController : Player
{
    [Header("References")]
    public Rigidbody rb;
    public FollowerController follower;

    [Header("Movement")]
    public float moveSpeed;
    public float detachedSpeedMultiplier; // when follower is ejecté/detached

    [Header("Ejection / Charge")]
    public float chargeMaxTime; // temps max de charge (s)
    public float minEjectDistance; // distance minimale de l'éjection
    public float maxEjectDistance;   // distance maximale de l'éjection
    public AnimationCurve chargeToDistance = AnimationCurve.Linear(0f, 0f, 1f, 1f); // map normalized charge -> 0..1 distance

    [Header("Indicator")]
    public GameObject landingIndicatorPrefab; // assign a small sprite/quad to show landing spot
    GameObject landingIndicatorInstance;

    [Header("Input (Input System)")]
    public InputActionReference moveAction; // Vector2
    public InputActionReference eAction; // Button

    // runtime
    float chargeTimer;
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
            // charging logic...
            // compute landingPoint
            Vector3 dir = (follower.transform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
            dir = dir.normalized; 
            float t = Mathf.Clamp01(chargeTimer / chargeMaxTime);
            float targetDistance = Mathf.Lerp(minEjectDistance, maxEjectDistance, chargeToDistance.Evaluate(t));
            Vector3 landingPoint = transform.position + dir * targetDistance;

            // IMPORTANT: clamp landingPoint to dancefloor minus a margin so follower can land fully inside
            float landingMargin = 0.25f + follower.orbitRadius; // safe margin (évite d'atterrir collé au bord)
            print(landingPoint);
            landingPoint = ClampPositionToDancefloor(landingPoint, landingMargin);
            print(landingPoint);

            if (follower != null) follower.ShowLandingIndicator(landingPoint);
        }
        else if (!eHeld && isCharging)
        {
            // release: compute landingPoint again (same as above)
            Vector3 dir = (follower.transform.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
            dir = dir.normalized; 
            float t = Mathf.Clamp01(chargeTimer / chargeMaxTime);
            float targetDistance = Mathf.Lerp(minEjectDistance, maxEjectDistance, chargeToDistance.Evaluate(t));
            Vector3 landingPoint = transform.position + dir * targetDistance;

            // clamp landingPoint to bounds
            float landingMargin = 0.25f + follower.orbitRadius;
            print(landingPoint);
            landingPoint = ClampPositionToDancefloor(landingPoint, landingMargin);
            print(landingPoint);

            if (follower != null && follower.CurrentState == FollowerController.State.Solidaire)
            {
                follower.StartEject(landingPoint, targetDistance);
            }

            if (follower != null) follower.HideLandingIndicator();

            chargeTimer = 0f;
        }
        else
        {
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
