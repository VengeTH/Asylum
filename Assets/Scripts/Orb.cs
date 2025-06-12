using UnityEngine;

public class Orb : MonoBehaviour
{
    public float hunterDuration = 15f;
    private OrbManager orbManager;
    private bool playerInRange = false;

    void Start()
    {
        orbManager = FindAnyObjectByType<OrbManager>();
        if (orbManager == null)
        {
            Debug.LogError("OrbManager not found in the scene!");
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Check if player is on the same floor (Y position within 1.0f units)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float floorThreshold = 2.5f;
                bool isOnSameFloor = Mathf.Abs(transform.position.y - player.transform.position.y) < floorThreshold;
                if (isOnSameFloor && orbManager != null)
            {
                orbManager.PlayerHasCollectedOrb();
                Destroy(gameObject);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}