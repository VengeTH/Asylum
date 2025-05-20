using UnityEngine;

public class Orb : MonoBehaviour
{
    public float hunterDuration = 15f;
    private OrbManager orbManager;

    void Start()
    {
        orbManager = FindAnyObjectByType<OrbManager>();
        if (orbManager == null)
        {
            Debug.LogError("OrbManager not found in the scene!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && orbManager != null)
        {
            orbManager.PlayerHasCollectedOrb(); // Changed to new method
            Destroy(gameObject);
        }
    }
}