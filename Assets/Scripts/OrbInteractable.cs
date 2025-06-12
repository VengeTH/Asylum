using UnityEngine;

public class OrbInteractable : MonoBehaviour
{
    public float interactionDistance = 2f;
    public GameObject interactionPrompt; // Assign your "E to Collect" Text here
    public AudioClip pickupSound;
    public float pickupSoundVolume = 1.2f; // 120% volume
    private AudioSource audioSource;

    private Transform playerTransform;
    private bool isPlayerNearby = false;

    void Start()
    {
        // Find the player once at start
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            Debug.LogError("Player not found! Make sure it has tag 'Player'");
        }

        // Find the interaction prompt if not assigned
        if (interactionPrompt == null)
        {
            // Try to find the interaction prompt in the canvas
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                Transform promptTransform = canvas.transform.Find("InteractionText");
                if (promptTransform != null)
                {
                    interactionPrompt = promptTransform.gameObject;
                    Debug.Log($"Found interaction prompt in canvas {canvas.name}: {interactionPrompt.name}");
                    break;
                }
            }

            if (interactionPrompt == null)
            {
                Debug.LogError("InteractionText not found in any canvas! Make sure it exists and is named 'InteractionText'");
            }
        }

        // Ensure prompt is hidden at start
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
            Debug.Log($"Interaction prompt initialized and hidden for orb: {gameObject.name}");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogWarning("OrbInteractable: AudioSource component not found. Adding one.");
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            // Try to find player again if lost
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= interactionDistance && !isPlayerNearby)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                Debug.Log($"Showing interaction prompt for orb: {gameObject.name} at distance: {distanceToPlayer:F2}");
            }
            else
            {
                Debug.LogWarning($"Interaction prompt is null for orb: {gameObject.name}");
            }
            isPlayerNearby = true;
        }
        else if (distanceToPlayer > interactionDistance && isPlayerNearby)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
                Debug.Log($"Hiding interaction prompt for orb: {gameObject.name} at distance: {distanceToPlayer:F2}");
            }
            isPlayerNearby = false;
        }

        // Only check input if nearby
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Collect();
        }
    }

    void Collect()
    {
        // Play pickup sound
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound, pickupSoundVolume);
        }
        else if (pickupSound == null)
        {
            Debug.LogWarning($"Pickup sound not assigned for orb: {gameObject.name}");
        }
        else if (audioSource == null) // Should not happen if Start() is correct
        {
            Debug.LogWarning($"AudioSource not found for orb: {gameObject.name}, cannot play sound.");
        }

        // Notify GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.CollectOrb();
            Debug.Log($"Orb collected, notifying GameManager");
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }

        // Hide the prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
            Debug.Log($"Hiding interaction prompt after collection");
        }

        // Destroy the orb
        // For short sounds, PlayOneShot is fine before Destroy.
        // If the sound is long, consider playing it on a separate, persistent AudioSource
        // or delaying the Destroy call.
        Destroy(gameObject);
    }
}