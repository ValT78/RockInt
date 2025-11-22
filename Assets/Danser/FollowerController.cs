using UnityEngine;
using UnityEngine.InputSystem;

public class FollowerController : Player
{
    public enum State { Solidaire, Ejected, Detached }
    public State CurrentState { get; private set; } = State.Solidaire;

    [Header("References")]
    public Transform leader;
    public Rigidbody rb;
    public Animator animator; // optional

    [Header("Orbit / Solidaire")]
    public float orbitRadius = 1.2f;
    public float orbitAngularSpeed = 2.2f; // rad/s (or micro param)
    public float solidaireSmooth = 0.08f;

    [Header("Eject / Detached")]
    public float ejectTime = 0.6f;
    public float baseReturnSpeed = 2.5f;
    public float returnAccel = 6f; // m/s^2 while holding A
    public float maxReturnSpeed = 8f;
    public float attachDistance;

    [Header("Ejection force mapping")]
    public float ejectForcePerUnitDistance = 1.6f; // multiply by targetDistance to compute impulse
    public float minEjectImpulse = 3f;
    public float maxEjectImpulse = 18f;
    public float landingReachThreshold; // distance tolerance to consider "landed"

    [Header("Indicator (assign prefab)")]
    public GameObject landingIndicatorPrefab;
    GameObject landingIndicatorInstance;

    [Header("Input")]
    public InputActionReference accelerateAction; // A
    public InputActionReference stopAction; // E (while detached to stop)

    // runtime
    float ejectTimer = 0f;
    float currentReturnSpeed = 0f;
    Vector3 orbitAngleOffset; // keep a personal offset so they don't overlap perfectly
    Vector3 velSmooth = Vector3.zero;
    // runtime - landing target used during Ejected state
    Vector3 landingTarget = Vector3.zero;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // initial offset angle
        orbitAngleOffset = Vector3.up * Random.Range(0f, Mathf.PI * 2f);
        EnterSolidaire();
        // prepare indicator instance but keep disabled
        if (landingIndicatorPrefab != null)
        {
            landingIndicatorInstance = Instantiate(landingIndicatorPrefab, transform.position, Quaternion.Euler(90f, 0f, 0f)); // face up
            landingIndicatorInstance.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (accelerateAction != null) accelerateAction.action.Enable();
        if (stopAction != null) stopAction.action.Enable();
    }
    void OnDisable()
    {
        if (accelerateAction != null) accelerateAction.action.Disable();
        if (stopAction != null) stopAction.action.Disable();
    }

    bool ReadAccelerateHeld()
    {
        if (accelerateAction != null && accelerateAction.action != null)
            return accelerateAction.action.ReadValue<float>() > 0.1f;
        return false;
    }
    bool ReadStopHeld()
    {
        if (stopAction != null && stopAction.action != null)
            return stopAction.action.ReadValue<float>() > 0.1f;
        return false;
    }

    void FixedUpdate()
    {
        switch (CurrentState)
        {
            case State.Solidaire:
                UpdateSolidaire();
                break;
            case State.Ejected:
                UpdateEjected();
                break;
            case State.Detached:
                UpdateDetached();
                break;
        }
    }

    void EnterSolidaire()
    {
        CurrentState = State.Solidaire;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        currentReturnSpeed = baseReturnSpeed;
        if (animator != null) animator.SetTrigger("Attach");

        // --- ALIGN ORBIT TO CURRENT POSITION TO AVOID TELEPORT ---
        if (leader != null)
        {
            // calcule l'angle polaire (XZ) du follower par rapport au leader
            Vector3 toFollower = transform.position - leader.position;
            toFollower.y = 0f;
            if (toFollower.sqrMagnitude > 0.0001f)
            {
                float angleToFollower = Mathf.Atan2(toFollower.z, toFollower.x); // en radians
                                                                                 // on veut : angle = Time.time * orbitAngularSpeed + orbitAngleOffset.y
                                                                                 // donc orbitAngleOffset.y = angleToFollower - Time.time * orbitAngularSpeed
                orbitAngleOffset.y = angleToFollower - Time.time * orbitAngularSpeed;
                // repositionne exactement sur la circonférence pour éviter petites dérives
                Vector3 desired = leader.position + new Vector3(Mathf.Cos(angleToFollower), 0f, Mathf.Sin(angleToFollower)) * orbitRadius;
                transform.position = desired;
            }
            else
            {
                // fallback si au même point
                orbitAngleOffset.y = Random.Range(0f, Mathf.PI * 2f);
            }
        }
    }


    /// <summary>
    /// Called by leader when releasing an eject.
    /// landingPoint: world position where we should land / travel toward
    /// targetDistance: used to map to impulse magnitude (if you keep impulse)
    /// </summary>
    public void StartEject(Vector3 landingPoint, float targetDistance)
    {
        if (CurrentState != State.Solidaire) return;

        // compute direction and impulse (optional)
        Vector3 dir = (landingPoint - transform.position);
        dir.y = 0f;
        float distance = dir.magnitude;
        if (distance < 0.001f) dir = transform.forward;
        else dir.Normalize();

        float impulse = Mathf.Clamp(targetDistance * ejectForcePerUnitDistance, minEjectImpulse, maxEjectImpulse);

        // Set landing target (preserve the exact world point)
        landingTarget = landingPoint;

        // Switch to Ejected state (physics-driven movement toward landingTarget)
        CurrentState = State.Ejected;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;

        // Optionally keep a short impulse to make the initial throw feel nicer.
        // You can comment out the AddForce line if you prefer pure interpolation towards landingTarget.
        rb.AddForce(dir * impulse, ForceMode.Impulse);

        if (animator != null) animator.SetTrigger("Eject");

        // Ensure indicator hidden
        HideLandingIndicator();
    }

    void EnterDetached()
    {
        if (CurrentState == State.Detached) return;
        CurrentState = State.Detached;
        rb.isKinematic = false;
        currentReturnSpeed = baseReturnSpeed;
        if (animator != null) animator.SetTrigger("Return");
    }

    // --- Updates per state ---
    void UpdateSolidaire()
    {
        if (leader == null) return;

        // récupère la vélocité du leader si présente
        Vector3 leaderVel = Vector3.zero;
        Rigidbody leaderRb = leader.GetComponent<Rigidbody>();
        if (leaderRb != null) leaderVel = leaderRb.linearVelocity;

        float speedFactor = 1f + leaderVel.magnitude * 0.2f;

        // calcule l'angle courant de l'orbite (dépend du temps + offset déjà aligné dans EnterSolidaire)
        float angle = Time.time * orbitAngularSpeed + orbitAngleOffset.y;

        // point désiré sur la circonférence XZ
        Vector3 desired = leader.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * orbitRadius;

        // smooth follow to desired (garde effet springy)
        Vector3 newPos = Vector3.SmoothDamp(transform.position, desired, ref velSmooth, solidaireSmooth, 100f, Time.fixedDeltaTime);
        transform.position = newPos;

        // face leader doucement
        Vector3 look = leader.position - transform.position;
        if (look.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 10f * Time.fixedDeltaTime);
        }
    }

    void UpdateEjected()
    {
        // Move towards the landingTarget until close enough, then switch to Detached.
        // We prioritize physics movement: set horizontal velocity toward the target while preserving y velocity (gravity).
        Vector3 toTarget = landingTarget - transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        // If we're very close (or overshot), consider landed.
        if (dist <= landingReachThreshold)
        {
            // snap to landing target's XZ plane to avoid tiny drifting, keep current Y
            /*Vector3 snapPos = new Vector3(landingTarget.x, transform.position.y, landingTarget.z);
            transform.position = snapPos;*/

            // Transition to Detached so player can now control return
            EnterDetached();
            return;
        }

        // Compute desired horizontal velocity toward target. We scale speed by dist to avoid abrupt stops.
        // You can tweak the factor below for "gliding" vs "fast travel".
        float travelSpeed = Mathf.Clamp(dist * 6f, 2f, maxReturnSpeed * 1.2f); // tuned for initial travel feeling
        Vector3 desiredVel = toTarget.normalized * travelSpeed;

        // Apply to rigidbody while preserving vertical (gravity) velocity
        rb.linearVelocity = new Vector3(desiredVel.x, rb.linearVelocity.y, desiredVel.z);

        // Optional: orient smoothly toward movement direction
        if (desiredVel.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(desiredVel.x, 0f, desiredVel.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.fixedDeltaTime);
        }

        // While in Ejected state, ignore attach checks (handled only in Detached)
    }

    void UpdateDetached()
    {
        if (leader == null) return;

        // Input: accelerate / stop (controls currentReturnSpeed)
        bool accel = ReadAccelerateHeld();
        bool stop = ReadStopHeld();

        if (stop)
        {
            currentReturnSpeed = 0f;
        }
        else if (accel)
        {
            currentReturnSpeed += returnAccel * Time.fixedDeltaTime;
            currentReturnSpeed = Mathf.Min(currentReturnSpeed, maxReturnSpeed);
        }
        else
        {
            // natural friction / slight slowdown toward base
            currentReturnSpeed = Mathf.MoveTowards(currentReturnSpeed, baseReturnSpeed, 2f * Time.fixedDeltaTime);
        }

        // compute direction to leader (XZ)
        Vector3 toLeader = leader.position - transform.position;
        toLeader.y = 0f;
        float dist = toLeader.magnitude;
        if (dist < 0.001f) toLeader = Vector3.zero;
        Vector3 dir = dist > 0.001f ? toLeader.normalized : Vector3.zero;

        // Move towards leader
        Vector3 desiredVelocity = dir * currentReturnSpeed;
        // apply movement with Rigidbody (simple)
        rb.linearVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);

        // attach check
        if (dist <= attachDistance && CurrentState == State.Detached)
        {
            // snap to orbit position nicely then attach
            transform.position = leader.position + (transform.position - leader.position).normalized * orbitRadius;
            EnterSolidaire();
            // notify leader? (could fire event)
        }

        // optional: orient toward leader
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.fixedDeltaTime);
        }
    }

    public void ShowLandingIndicator(Vector3 worldPos)
    {
        if (landingIndicatorPrefab == null) return;
        if (landingIndicatorInstance == null)
        {
            landingIndicatorInstance = Instantiate(landingIndicatorPrefab, worldPos, Quaternion.Euler(90f, 0f, 0f));
        }
        landingIndicatorInstance.SetActive(true);
        landingIndicatorInstance.transform.position = worldPos + Vector3.up * 0.02f; // slight lift to avoid z-fight
    }

    public void HideLandingIndicator()
    {
        if (landingIndicatorInstance != null)
            landingIndicatorInstance.SetActive(false);
    }

    // Optional debug gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        if (leader != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(leader.position, attachDistance);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(leader.position, orbitRadius);
        }
    }
}
