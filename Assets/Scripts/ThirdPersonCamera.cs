using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;                // Player transform
    public Vector3 offset = new Vector3(0, 2f, -5f);  // Adjusted default offset
    public float smoothSpeed = 10f;         // Increased for more responsive following
    public float rotationSpeed = 5f;        // Added rotation speed
    public float minVerticalAngle = -30f;   // Minimum looking down angle
    public float maxVerticalAngle = 60f;    // Maximum looking up angle
    public float mouseXSensitivity = 2f;    // Mouse X sensitivity
    public float mouseYSensitivity = 1f;    // Mouse Y sensitivity

    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    void Start()
    {
        // Initialize rotation
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up);
            rotationY = transform.eulerAngles.y;
        }

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseXSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseYSensitivity;

        // Calculate rotation
        rotationY += mouseX;
        rotationX -= mouseY; // Inverted for natural camera movement
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        // Calculate camera position
        Vector3 targetRotation = new Vector3(rotationX, rotationY, 0);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref smoothVelocity, smoothSpeed * Time.deltaTime);

        // Apply rotation and position
        transform.rotation = Quaternion.Euler(currentRotation);
        Vector3 desiredPosition = target.position + Quaternion.Euler(currentRotation) * offset;

        // Check for collisions
        RaycastHit hit;
        if (Physics.Linecast(target.position + Vector3.up, desiredPosition, out hit))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = desiredPosition;
        }

        // Make camera look at player's head level
        transform.LookAt(target.position + Vector3.up);
    }

    void OnEnable()
    {
        // Lock and hide cursor when camera is enabled
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        // Unlock and show cursor when camera is disabled
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}