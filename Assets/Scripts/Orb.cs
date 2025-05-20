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
            if (orbManager != null)
            {
                orbManager.PlayerHasCollectedOrb();
                Destroy(gameObject);
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