using UnityEngine;
using TMPro; // Added for TextMeshPro support

public class ObjectiveCollectible : MonoBehaviour
{
    public static event System.Action OnObjectiveCollected;

    // Objective Counter Variables
    public static int totalObjectives = 5; // Default total, can be set in Inspector for one instance
    private static int collectedObjectives = 0;
    public static TextMeshProUGUI objectiveCounterText; // Assign this in the Inspector for one of the collectibles

    public string objectiveName = "Collectible Objective"; // Name for this objective, can be customized in Inspector
    public TextMeshProUGUI collectPromptText; // Reference to the TextMeshPro UI element for the prompt

    private bool isCollected = false;
    private bool playerInRange = false; // New flag to track if player is in range

    // Ensure the counter text is initialized correctly
    void Awake()
    {
        // Attempt to find the counter text if not assigned, or ensure it's set up by one instance
        if (objectiveCounterText == null)
        {
            // This is a common way to find a UI element by name/tag if not directly assigned.
            // For robustness, it's better to assign this in the Inspector.
            GameObject counterTextObject = GameObject.FindWithTag("ObjectiveCounterText"); // Make sure you have a UI TextMeshPro with this tag
            if (counterTextObject != null)
            {
                objectiveCounterText = counterTextObject.GetComponent<TextMeshProUGUI>();
            }
        }
        UpdateObjectiveCounterText(); // Initial update
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the player (or whatever should collect this) enters the trigger
        // Ensure your player GameObject has a tag like "Player"
        if (!isCollected && other.CompareTag("Player"))
        {
            playerInRange = true;
            if (collectPromptText != null) 
            {
                collectPromptText.text = "Collect"; // Set the prompt text
                collectPromptText.gameObject.SetActive(true);
            }
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
        // Check for 'E' key press if player is in range and objective not collected
        if (playerInRange && !isCollected && Input.GetKeyDown(KeyCode.E)) // Check for 'E' key
        {
            Collect();
        }
    }

    void Collect()
    {
        isCollected = true;
        collectedObjectives++; // Increment collected objectives
        UpdateObjectiveCounterText(); // Update the counter display

        if (collectPromptText != null) collectPromptText.gameObject.SetActive(false); // Hide prompt on collect
        Debug.Log($"Objective '{objectiveName}' collected!");

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

    // Static method to update the objective counter UI Text
    public static void UpdateObjectiveCounterText()
    {
        if (objectiveCounterText != null)
        {
            objectiveCounterText.text = $"Objectives: {collectedObjectives}/{totalObjectives}";
        }
        else
        {
            Debug.LogWarning("ObjectiveCounterText is not assigned in ObjectiveCollectible script.");
        }
    }

    // Call this method if you need to reset the objectives (e.g., on game restart or level load)
    public static void ResetObjectives(int newTotalObjectives)
    {
        collectedObjectives = 0;
        totalObjectives = newTotalObjectives;
        UpdateObjectiveCounterText();
    }

    // Optional: Add fields for sound effects or particle effects if desired
    // public AudioClip collectionSound;
    // public GameObject collectionEffectPrefab;
}