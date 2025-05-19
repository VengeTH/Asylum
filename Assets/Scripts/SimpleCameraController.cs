using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float horizontalSensitivity = 2f;
    public float verticalSensitivity = 2f;
    public float maxLookAngle = 80f;
    public bool invertCamera = false;

    private Camera playerCamera;
    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Get the camera component
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError("No camera found in children of SimpleCameraController!");
            enabled = false;
            return;
        }

        // Initialize rotation values
        Vector3 rotation = transform.eulerAngles;
        rotationX = rotation.y;
        rotationY = rotation.x;

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        // Apply vertical rotation (pitch)
        rotationY += invertCamera ? mouseY : -mouseY;
        rotationY = Mathf.Clamp(rotationY, -maxLookAngle, maxLookAngle);

        // Apply horizontal rotation (yaw)
        rotationX += mouseX;

        // Apply rotations
        transform.rotation = Quaternion.Euler(0f, rotationX, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationY, 0f, 0f);

        // Toggle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }
}