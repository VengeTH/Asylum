using UnityEngine;
using TMPro; // Added for TextMeshPro support

public class ObjectiveCollectible : MonoBehaviour
{
    public static event System.Action OnObjectiveCollected;

    public string objectiveName = "Collectible Objective"; // Name for this objective, can be customized in Inspector
    public TextMeshProUGUI collectPromptText; // Reference to the TextMeshPro UI element for the prompt

    private bool isCollected = false;
    private bool playerInRange = false; // New flag to track if player is in range

    void OnTriggerEnter(Collider other)
    {
        // Check if the player (or whatever should collect this) enters the trigger
        // Ensure your player GameObject has a tag like "Player"
        if (!isCollected && other.CompareTag("Player"))
        {
            playerInRange = true;
            if (collectPromptText != null) collectPromptText.gameObject.SetActive(true);
            // Debug.Log($"Player entered range of {objectiveName}"); // Optional: for debugging
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (collectPromptText != null) collectPromptText.gameObject.SetActive(false);
            // Debug.Log($"Player exited range of {objectiveName}"); // Optional: for debugging
        }
    }

    void Update()
    {
        // Check for right mouse button click if player is in range and objective not collected
        if (playerInRange && !isCollected && Input.GetMouseButtonDown(1)) // 1 is for the right mouse button
        {
            Collect();
        }
    }

    void Collect()
    {
        isCollected = true;
        if (collectPromptText != null) collectPromptText.gameObject.SetActive(false); // Hide prompt on collect
        Debug.Log($"Objective '{objectiveName}' collected by right-click!");

        // Notify GameManager or other systems
        OnObjectiveCollected?.Invoke();

        // Optional: Play a sound effect
        // if (collectionSound != null) AudioSource.PlayClipAtPoint(collectionSound, transform.position);

        // Optional: Show a particle effect
        // if (collectionEffectPrefab != null) Instantiate(collectionEffectPrefab, transform.position, Quaternion.identity);

        // Deactivate or destroy the collectible object
        // gameObject.SetActive(false); // Option 1: Deactivate
        Destroy(gameObject); // Option 2: Destroy
    }

    // Optional: Add fields for sound effects or particle effects if desired
    // public AudioClip collectionSound;
    // public GameObject collectionEffectPrefab;
}