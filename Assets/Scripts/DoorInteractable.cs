using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    public float interactionDistance = 2f;
    public GameObject interactionPrompt;
    public float rotationSpeed = 90f; // Degrees per second
    public float maxRotationAngle = 90f; // Maximum door opening angle

    private Transform playerTransform;
    private bool isPlayerNearby = false;
    private bool isOpen = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    void Start()
    {
        // Find the player once at start
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Make sure it has tag 'Player'");
        }

        if (interactionPrompt == null)
        {
            Debug.LogWarning("Interaction Prompt not assigned on door: " + name);
        }

        // Store the initial rotation
        initialRotation = transform.rotation;
        targetRotation = initialRotation;

        Debug.Log($"Door {gameObject.name} initialized. Initial rotation: {initialRotation.eulerAngles}");
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Handle interaction prompt
        if (distanceToPlayer <= interactionDistance && !isPlayerNearby)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                Debug.Log($"Player entered interaction range of door {gameObject.name}");
            }
            isPlayerNearby = true;
        }
        else if (distanceToPlayer > interactionDistance && isPlayerNearby)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
                Debug.Log($"Player left interaction range of door {gameObject.name}");
            }
            isPlayerNearby = false;
        }

        // Handle door interaction
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"E pressed near door {gameObject.name}. Current rotation: {transform.rotation.eulerAngles}");
            ToggleDoor();
        }

        // Smooth door rotation
        if (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            Debug.Log($"Door {gameObject.name} rotating. Current: {transform.rotation.eulerAngles}, Target: {targetRotation.eulerAngles}");
        }
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            // Calculate which way to open the door based on player position
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            float dot = Vector3.Dot(transform.right, directionToPlayer);
            float rotationAngle = dot > 0 ? maxRotationAngle : -maxRotationAngle;

            targetRotation = initialRotation * Quaternion.Euler(0, rotationAngle, 0);
            Debug.Log($"Opening door {gameObject.name}. Target rotation: {targetRotation.eulerAngles}");
        }
        else
        {
            targetRotation = initialRotation;
            Debug.Log($"Closing door {gameObject.name}. Target rotation: {targetRotation.eulerAngles}");
        }
    }
}