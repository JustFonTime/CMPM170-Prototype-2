using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Player2_5DController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;        // horizontal/forward speed (X & Z)
    public float accel = 20f;           // how quickly to reach target speed

    [Header("Jump")]
    public float jumpForce = 10f;
    public float coyoteTime = 0.1f;
    public float jumpBuffer = 0.1f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.15f;
    public float groundCheckOffset = 0.9f;

    Rigidbody rb;
    CapsuleCollider col;

    // desired planar (X,Z) velocity
    float desiredX;
    float desiredZ;
    float lastGroundedTime;
    float lastJumpPressed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        // IMPORTANT: In the Rigidbody constraints, do NOT freeze Position Z anymore.
        // You can still freeze rotations X/Y/Z if you want the capsule upright.
    }

    void Update()
    {
        // WASD / Arrow keys (old Input Manager)
        float h = Input.GetAxisRaw("Horizontal"); // A/D  or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S  or Up/Down

        desiredX = h * moveSpeed;
        desiredZ = v * moveSpeed;

        if (Input.GetButtonDown("Jump"))
            lastJumpPressed = Time.time;

        if (IsGrounded())
            lastGroundedTime = Time.time;
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.linearVelocity;

        // Smoothly move current XZ velocity toward desired XZ target
        Vector2 curXZ = new Vector2(vel.x, vel.z);
        Vector2 targetXZ = new Vector2(desiredX, desiredZ);
        Vector2 newXZ = Vector2.MoveTowards(curXZ, targetXZ, accel * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(newXZ.x, vel.y, newXZ.y);

        // Jump (coyote + buffer)
        bool canCoyote = (Time.time - lastGroundedTime) <= coyoteTime;
        bool hasBuffered = (Time.time - lastJumpPressed) <= jumpBuffer;

        if (canCoyote && hasBuffered)
        {
            lastJumpPressed = -999f;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    bool IsGrounded()
    {
        Vector3 center = transform.position + Vector3.down * groundCheckOffset;
        return Physics.CheckSphere(center, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + Vector3.down * groundCheckOffset;
        Gizmos.DrawWireSphere(center, groundCheckRadius);
    }
}
