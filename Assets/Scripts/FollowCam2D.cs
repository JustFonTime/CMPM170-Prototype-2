using UnityEngine;

public class FollowCam3D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    public float smooth = 8f;

    [Header("Options")]
    public bool keepOffsetFromStart = true; // compute offset from current positions
    public bool lookAtTarget = true;        // rotate camera to face the target
    public bool lockYToOffset = false;      // keep camera height constant (good for flat levels)

    Vector3 startOffset;

    void Start()
    {
        if (keepOffsetFromStart && target)
            startOffset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 o = keepOffsetFromStart ? startOffset : offset;
        Vector3 goal = target.position + o;

        if (lockYToOffset)
            goal.y = (keepOffsetFromStart ? (target.position.y + startOffset.y) : offset.y);

        transform.position = Vector3.Lerp(transform.position, goal, Time.deltaTime * smooth);

        if (lookAtTarget)
        {
            Quaternion look = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * smooth);
        }
    }
}
