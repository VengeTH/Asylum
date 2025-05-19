using UnityEngine;

public class ObjectiveCollectible : MonoBehaviour
{
    public static event System.Action OnObjectiveCollected;

    public string objectiveName = "Collectible Objective"; // Name for this objective, can be customized in Inspector

    private bool isCollected = false;

    void OnTriggerEnter(Collider other)
    {
        // Check if the player (or whatever should collect this) enters the trigger
        // Ensure your player GameObject has a tag like "Player"
        if (!isCollected && other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        isCollected = true;
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

    // Optional: Add fields for sound effects or particle effects if desired
    // public AudioClip collectionSound;
    // public GameObject collectionEffectPrefab;
}