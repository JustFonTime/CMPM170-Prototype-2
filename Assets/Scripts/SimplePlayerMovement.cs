using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayerMovement : MonoBehaviour
{
    public float moveSpeed;

    InputAction moveAction;
    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var moveDirection = moveAction.ReadValue<Vector2>();

        if (moveDirection.x > 0)
        {
            rb.linearVelocity = Vector3.right * moveSpeed;
        }
        else if (moveDirection.x < 0)
        {
            rb.linearVelocity = Vector3.left * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }





    }
}
