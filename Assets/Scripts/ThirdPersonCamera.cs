using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // Player transform
    public Vector3 offset = new Vector3(0, 1.5f, -3);
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}