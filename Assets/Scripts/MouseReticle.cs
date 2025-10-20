using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MouseReticle : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform reticle;  // assign the UI Image's RectTransform

    [Header("Behavior")]
    public float baseSize = 12f;     // px
    public float clickSize = 24f;    // px when clicking
    public float followSmooth = 0f;  // 0 = snap; try 10 for a bit of lag
    public bool hideSystemCursor = true;

    Vector3 vel; // for smooth damp
    Vector2 targetPos;
    float currentSize;

    void Awake()
    {
        if (hideSystemCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None; // keep free movement
        }
        currentSize = baseSize;
    }

    void Update()
    {
        // --- 1) Read mouse position & click state (both input systems) ---
        Vector2 mousePos;
        bool isDown = false;

#if ENABLE_INPUT_SYSTEM
        var m = Mouse.current;
        if (m != null)
        {
            mousePos = m.position.ReadValue();
            isDown = m.leftButton.isPressed;
        }
        else { mousePos = Input.mousePosition; isDown = Input.GetMouseButton(0); }
#else
        mousePos = Input.mousePosition;
        isDown = Input.GetMouseButton(0);
#endif

        targetPos = mousePos;

        // --- 2) Size feedback ---
        float targetSize = isDown ? clickSize : baseSize;
        currentSize = Mathf.Lerp(currentSize, targetSize, 20f * Time.deltaTime);

        // --- 3) Apply to UI element ---
        if (reticle != null)
        {
            // Screen Space Overlay: we can set world position to screen coords directly
            if (followSmooth <= 0f)
                reticle.position = targetPos;
            else
                reticle.position = Vector3.SmoothDamp(reticle.position, targetPos, ref vel, 1f / followSmooth);

            reticle.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentSize);
            reticle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentSize);
        }
    }

    void OnDestroy()
    {
        if (hideSystemCursor)
        {
            Cursor.visible = true; // restore on exit
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
