using UnityEngine;
using UnityEngine.InputSystem;

public class FollowerController : MonoBehaviour
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

    [Header("Input")]
    public InputActionReference accelerateAction; // A
    public InputActionReference stopAction; // E (while detached to stop)

    // runtime
    float ejectTimer = 0f;
    float currentReturnSpeed = 0f;
    Vector3 orbitAngleOffset; // keep a personal offset so they don't overlap perfectly
    Vector3 velSmooth = Vector3.zero;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // initial offset angle
        orbitAngleOffset = Vector3.up * Random.Range(0f, Mathf.PI * 2f);
        EnterSolidaire();
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

    // --- State handling ---
    void EnterSolidaire()
    {
        CurrentState = State.Solidaire;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        currentReturnSpeed = baseReturnSpeed;
        ejectTimer = 0f;
        if (animator != null) animator.SetTrigger("Attach");
    }

    public void StartEject(Vector3 direction, float force)
    {
        // called by leader
        if (CurrentState != State.Solidaire) return;
        CurrentState = State.Ejected;

        rb.isKinematic = false;
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        ejectTimer = 0f;
        if (animator != null) animator.SetTrigger("Eject");
    }

    void EnterDetached()
    {
        CurrentState = State.Detached;
        // ensure rigidbody active
        rb.isKinematic = false;
        currentReturnSpeed = baseReturnSpeed;
        if (animator != null) animator.SetTrigger("Return");
    }

    // --- Updates per state ---
    void UpdateSolidaire()
    {
        if (leader == null) return;
        // orbit angle depends on leader velocity for fun
        Vector3 leaderVel = Vector3.zero;
        Rigidbody leaderRb = leader.GetComponent<Rigidbody>();
        if (leaderRb != null) leaderVel = leaderRb.linearVelocity;

        float speedFactor = 1f + leaderVel.magnitude * 0.2f;
        float ang = orbitAngularSpeed * Time.fixedDeltaTime * speedFactor;
        // simple orbital position around leader on XZ plane
        float angle = Time.time * orbitAngularSpeed + orbitAngleOffset.y;
        Vector3 desired = leader.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * orbitRadius;
        // smooth follow to desired (keeps nice springy movement)
        Vector3 newPos = Vector3.SmoothDamp(transform.position, desired, ref velSmooth, solidaireSmooth, 100f, Time.fixedDeltaTime);
        transform.position = newPos;
        // optionally face leader
        Vector3 look = leader.position - transform.position;
        if (look.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 10f * Time.fixedDeltaTime);
        }
    }

    void UpdateEjected()
    {
        ejectTimer += Time.fixedDeltaTime;
        if (ejectTimer >= ejectTime)
        {
            // switch to detached (start active return)
            EnterDetached();
        }
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
        if (dist <= attachDistance)
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
