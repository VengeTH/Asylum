using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Orb Settings")]
    public int collectedOrbs = 0;
    public int orbsRequiredForTransformation = 5;
    public float hunterDuration = 10f;

    [Header("Camera References")]
    public Camera firstPersonCam;
    public Camera thirdPersonCam;

    [Header("Player References")]
    public GameObject player;

    private EnemyAI[] cachedEnemies;
    private bool isTransformed = false;  // New flag to track transformation state
    private Coroutine hunterModeCoroutine; // To store the coroutine reference

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Optional: Find cameras by name if not assigned
        if (firstPersonCam == null)
            firstPersonCam = GameObject.Find("Camera_FirstPerson")?.GetComponent<Camera>();
        if (thirdPersonCam == null)
            thirdPersonCam = GameObject.Find("Camera_ThirdPerson")?.GetComponent<Camera>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        // Cache enemies at start
        RefreshEnemiesCache();

        // Set initial camera state
        if (firstPersonCam != null && thirdPersonCam != null)
        {
            firstPersonCam.enabled = true;
            thirdPersonCam.enabled = false;
        }
        else
        {
            Debug.LogError("Camera references not set in GameManager!");
        }
    }

    private void RefreshEnemiesCache()
    {
        cachedEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
    }

    /// <summary>
    /// Call this when an orb is collected
    /// </summary>
    public void CollectOrb()
    {
        collectedOrbs++;
        Debug.Log($"Collected Orbs: {collectedOrbs}");

        // Only transform if we haven't transformed yet and have enough orbs
        if (!isTransformed && collectedOrbs >= orbsRequiredForTransformation)
        {
            isTransformed = true;  // Set the flag
            ActivateHunterMode();
        }
    }

    /// <summary>
    /// Activates Hunter Mode
    /// </summary>
    public void ActivateHunterMode()
    {
        Debug.Log("🔥 Hunter Mode Activated!");

        // Switch to third person camera
        if (firstPersonCam != null && thirdPersonCam != null)
        {
            firstPersonCam.enabled = false;
            thirdPersonCam.enabled = true;
            Debug.Log("Switched to third person camera");
        }

        // Make enemies vulnerable
        foreach (EnemyAI enemy in cachedEnemies)
        {
            if (enemy != null)
                enemy.SetVulnerable(true);
        }

        // If there's an existing coroutine, stop it
        if (hunterModeCoroutine != null)
        {
            StopCoroutine(hunterModeCoroutine);
        }

        // Start the timer to deactivate hunter mode
        hunterModeCoroutine = StartCoroutine(DeactivateHunterModeAfterDelay());
    }

    private IEnumerator DeactivateHunterModeAfterDelay()
    {
        yield return new WaitForSeconds(hunterDuration);
        DeactivateHunterMode();
    }

    private void DeactivateHunterMode()
    {
        Debug.Log("Hunter Mode Deactivated!");

        // Switch back to first person camera
        if (firstPersonCam != null && thirdPersonCam != null)
        {
            firstPersonCam.enabled = true;
            thirdPersonCam.enabled = false;
            Debug.Log("Switched back to first person camera");
        }

        // Make enemies invulnerable again
        foreach (EnemyAI enemy in cachedEnemies)
        {
            if (enemy != null)
                enemy.SetVulnerable(false);
        }

        // Reset the coroutine reference
        hunterModeCoroutine = null;

        // Optional: Allow player to transform again
        isTransformed = false;
        collectedOrbs = 0; // Optional: Reset orb count
    }

    /// <summary>
    /// Changes the number of orbs needed to activate Hunter Mode
    /// </summary>
    /// <param name="newRequirement">The new number of orbs required</param>
    public void SetOrbsRequiredForTransformation(int newRequirement)
    {
        if (newRequirement > 0)
        {
            orbsRequiredForTransformation = newRequirement;
            Debug.Log($"Orbs required for transformation set to: {newRequirement}");
        }
        else
        {
            Debug.LogWarning("Orb requirement must be greater than 0.");
        }
    }

    private void SwitchToThirdPerson()
    {
        if (firstPersonCam != null && thirdPersonCam != null)
        {
            firstPersonCam.enabled = false;
            thirdPersonCam.enabled = true;
            Debug.Log("Switched to third person camera");
        }
    }

    // Optional: Method to reset the game state if needed
    public void ResetGameState()
    {
        isTransformed = false;
        collectedOrbs = 0;
        if (hunterModeCoroutine != null)
        {
            StopCoroutine(hunterModeCoroutine);
            DeactivateHunterMode();
        }
    }
}