using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    public bool invertCamera = false;

    // Camera reference
    public Transform playerCamera;

    // Rotation variables
    private float pitch = 0f;
    private float yaw = 0f;

    void Start()
    {
        // Lock cursor to window
        Cursor.lockState = CursorLockMode.Locked;

        // Make sure we have a camera
        if (playerCamera == null)
        {
            // Try to find the camera as a child component
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                playerCamera = cam.transform;
                Debug.Log("Found camera in children");
            }
            else
            {
                // Fall back to main camera
                playerCamera = Camera.main.transform;
                Debug.Log("No camera assigned, using main camera");
            }
        }

        // Initialize rotation values
        yaw = transform.eulerAngles.y;

        // Initialize pitch - try to get current camera pitch if any
        Vector3 localEulerAngles = playerCamera.localEulerAngles;
        if (localEulerAngles.x > 180f)
            pitch = localEulerAngles.x - 360f;
        else
            pitch = localEulerAngles.x;

        Debug.Log($"Starting with yaw: {yaw}, pitch: {pitch}");
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Apply sensitivity
        mouseX *= mouseSensitivity;
        mouseY *= mouseSensitivity;

        // Debug the inputs
        if (mouseX != 0 || mouseY != 0)
        {
            Debug.Log($"Mouse Input: X={mouseX}, Y={mouseY}");
        }

        // Update rotation values
        yaw += mouseX;

        // Apply vertical rotation based on inversion setting
        if (!invertCamera)
        {
            pitch -= mouseY; // Standard (mouse up = look up)
        }
        else
        {
            pitch += mouseY; // Inverted (mouse up = look down)
        }

        // Clamp vertical rotation
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        // Apply rotations - first horizontal rotation to the body
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Then vertical rotation to the camera
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Debug camera values after application
        if (mouseX != 0 || mouseY != 0)
        {
            Debug.Log($"Applied - Pitch: {pitch}, Camera local X: {playerCamera.localEulerAngles.x}");
        }

        // Allow escape key to unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}