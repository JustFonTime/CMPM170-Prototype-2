using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class DrunkBalanceController : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveForce = 40f;      // push strength when fully upright
    public float maxSpeed = 6f;            // speed cap when upright
    public float uprightPower = 1.5f;      // higher = movement dies faster when tilted

    [Header("Balance Physics")]
    public float wobbleTorque = 6f;        // random sway strength
    public float wobbleFreq = 0.7f;        // random sway frequency
    public float comOffsetY = 0.6f;        // raise CoM to be top-heavy
    public float angularDragUpright = 0.05f;
    public float angularDragProne = 0.02f;

    [Header("Mouse Support (click & drag head)")]
    public float grabSpring = 600f;        // spring stiffness of your “hold”
    public float grabDamping = 30f;        // damps jitter
    public float grabMaxForce = 2000f;     // absolute force clamp
    public float headOffset = 0.95f;       // % of collider top radius where head is
    public float grabRadiusScreen = 60f;   // px distance from head to start grab

    [Header("Friction (optional)")]
    public PhysicsMaterial uprightMaterial; // low friction (e.g., 0.2)
    public PhysicsMaterial proneMaterial;   // high friction (e.g., 0.9)

    Rigidbody rb;
    CapsuleCollider col;

    float inputX;                  // -1..1 (A/D or arrows)
    float inputZ;                  // -1..1 (W/S or arrows)
    bool grabActive;
    Vector3 grabTargetWorld;

    float wobbleSeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        // Allow Z movement; only lock X/Y rotations so the capsule can tip around Z.
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY;

        // Make it top-heavy
        rb.centerOfMass = new Vector3(0f, comOffsetY, 0f);
        rb.angularDamping = angularDragUpright; // (Unity 6+ API; old API uses angularDrag)

        wobbleSeed = Random.value * 1000f;
    }

    void Update()
    {
        // --- INPUT (supports both systems) ---
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        var ms = Mouse.current;

        inputX = 0f; inputZ = 0f;

        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) inputX -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) inputX += 1f;

            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) inputZ -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) inputZ += 1f;
        }

        bool mouseDown = ms != null ? ms.leftButton.isPressed : Input.GetMouseButton(0);
        Vector2 mousePos = ms != null ? ms.position.ReadValue() : (Vector2)Input.mousePosition;
#else
        inputX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        inputZ = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
        bool   mouseDown = Input.GetMouseButton(0);
        Vector2 mousePos = Input.mousePosition;
#endif

        // --- GRAB start/stop based on proximity to head ---
        Vector3 headWorld = GetHeadWorldPosition();
        Vector3 headScreen = Camera.main.WorldToScreenPoint(headWorld);
        float dist = Vector2.Distance((Vector2)headScreen, mousePos);

        if (!grabActive && mouseDown && dist <= grabRadiusScreen) grabActive = true;
        if (grabActive && !mouseDown) grabActive = false;

        // Update world-space grab target (project mouse onto plane at player Z)
        if (grabActive)
        {
            Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (plane.Raycast(ray, out float t))
                grabTargetWorld = ray.GetPoint(t);
        }
    }

    void FixedUpdate()
    {
        // Upright factor: 1 when upright, 0 when fully inverted
        float upright = Mathf.Clamp01(Vector3.Dot(transform.up, Vector3.up));
        bool prone = upright < 0.35f;

        // Swap friction (optional)
        if (uprightMaterial && proneMaterial)
            col.material = prone ? proneMaterial : uprightMaterial;

        // Angular damping (resistance to spin)
        rb.angularDamping = prone ? angularDragProne : angularDragUpright;

        // Random sway torque around Z
        float n = Mathf.PerlinNoise(wobbleSeed, Time.time * wobbleFreq) * 2f - 1f;
        rb.AddTorque(Vector3.forward * (n * wobbleTorque), ForceMode.Force);

        // Movement strength & speed cap scale with uprightness
        float moveScale = Mathf.Pow(upright, uprightPower);
        float targetPush = baseMoveForce * moveScale;
        float allowedSpd = Mathf.Max(0.8f, maxSpeed * moveScale);

        // Current planar speed (X,Z)
        Vector2 planar = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);

        // Desired push this step on XZ
        Vector3 push = new Vector3(inputX, 0f, inputZ) * targetPush;

        bool changingDir =
            (inputX != 0 && Mathf.Sign(inputX) != Mathf.Sign(planar.x)) ||
            (inputZ != 0 && Mathf.Sign(inputZ) != Mathf.Sign(planar.y));

        if (planar.magnitude < allowedSpd || changingDir)
            rb.AddForce(push, ForceMode.Force);

        // Mouse “hold” – spring-damper force at head position
        if (grabActive)
        {
            Vector3 headWorld = GetHeadWorldPosition();
            Vector3 toTarget = grabTargetWorld - headWorld;

            Vector3 headVel = rb.GetPointVelocity(headWorld);
            Vector3 force = (toTarget * grabSpring) - (headVel * grabDamping);

            // Optional: disallow lifting off the ground (only sideways/down assist)
            // force.y = Mathf.Min(force.y, 0f);

            // Clamp to keep things sane
            if (force.sqrMagnitude > grabMaxForce * grabMaxForce)
                force = force.normalized * grabMaxForce;

            rb.AddForceAtPosition(force, headWorld, ForceMode.Force);
        }
    }

    Vector3 GetHeadWorldPosition()
    {
        // Capsule height in local Y; head near the top hemisphere
        float half = (col.height * 0.5f) - col.radius;
        float yOff = Mathf.Max(0.0f, half) + col.radius * headOffset;
        return transform.TransformPoint(new Vector3(0f, yOff, 0f));
    }

    void OnDrawGizmosSelected()
    {
        if (!col) col = GetComponent<CapsuleCollider>();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetHeadWorldPosition(), 0.1f);
    }
}
