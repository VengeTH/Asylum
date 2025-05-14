using UnityEngine;

public class OrbInteractable : MonoBehaviour
{
    public float interactionDistance = 2f;
    public GameObject interactionPrompt; // Assign your "E to Collect" Text here

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

        if (interactionPrompt == null)
        {
            Debug.LogWarning("Interaction Prompt not assigned on orb: " + name);
        }
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= interactionDistance && !isPlayerNearby)
        {
            interactionPrompt.SetActive(true);
            isPlayerNearby = true;
        }
        else if (distanceToPlayer > interactionDistance && isPlayerNearby)
        {
            interactionPrompt.SetActive(false);
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
        // Notify GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.CollectOrb();
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }

        // Destroy the orb
        Destroy(gameObject);
    }
}