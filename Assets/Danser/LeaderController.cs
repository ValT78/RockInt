using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class LeaderController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public FollowerController follower;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detachedSpeedMultiplier = 1.6f; // when follower is ejecté/detached

    [Header("Ejection")]
    public float ejectForce = 8f;
    public float ejectCooldown = 0.25f;

    [Header("Input (Input System)")]
    public InputActionReference moveAction; // Vector2
    public InputActionReference eAction; // Button

    float lastEjectTime = -10f;

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

    bool ReadEPressed()
    {
        if (eAction != null && eAction.action != null)
            return eAction.action.triggered;
        return false;
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
        rb.MovePosition(targetPos);

        // Ejection trigger (context-sensitive handled inside follower)
        if (ReadEPressed() && follower.CurrentState == FollowerController.State.Solidaire && Time.time - lastEjectTime > ejectCooldown)
        {
            lastEjectTime = Time.time;
            if (follower != null)
            {
                // Tell follower to eject: choose direction from leader to follower
                Vector3 dir = (follower.transform.position - transform.position).normalized;
                if (dir == Vector3.zero) dir = transform.forward; // fallback
                follower.StartEject(dir, ejectForce);
            }
        }
    }

    // debug draw leader direction
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}
