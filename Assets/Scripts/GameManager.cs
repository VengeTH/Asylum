using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Orb Settings")]
    public int collectedOrbs = 0;
    public int orbsRequiredForTransformation = 5;
    public float hunterDuration = 10f;

    [Header("Timer Manager")] // Added Header
    public TimerManager timerManager; // Added TimerManager reference

    [Header("Camera References")]
    public Camera firstPersonCam;
    public Camera thirdPersonCam;

    [Header("Player References")]
    public GameObject player;

    private EnemyAI[] cachedEnemies;
    private bool isTransformed = false;  // New flag to track transformation state
    // private Coroutine hunterModeCoroutine; // To store the coroutine reference // REMOVED

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

        // Get TimerManager instance
        if (timerManager == null)
        {
            timerManager = FindAnyObjectByType<TimerManager>();
            if (timerManager == null)
            {
                Debug.LogError("TimerManager not found in the scene!");
            }
        }
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

        // Subscribe to TimerManager event
        if (timerManager != null)
        {
            TimerManager.OnHunterModeTimerEnd += DeactivateHunterMode; // Subscribe
        }
    }

    private void OnDestroy() // Added OnDestroy
    {
        // Unsubscribe from TimerManager event to prevent memory leaks
        if (timerManager != null) // Check if timerManager was found/assigned
        {
            TimerManager.OnHunterModeTimerEnd -= DeactivateHunterMode; // Unsubscribe
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

        // Start the timer using TimerManager
        if (timerManager != null)
        {
            timerManager.StartHunterModeTimer(hunterDuration);
        }
        else
        {
            Debug.LogError("TimerManager reference is not set in GameManager. Cannot start hunter mode timer.");
            // Fallback or error handling if TimerManager is missing, though Awake should have caught this.
        }

        // If there's an existing coroutine, stop it // REMOVED
        // if (hunterModeCoroutine != null) // REMOVED
        // { // REMOVED
        // StopCoroutine(hunterModeCoroutine); // REMOVED
        // } // REMOVED

        // Start the timer to deactivate hunter mode // REMOVED
        // hunterModeCoroutine = StartCoroutine(DeactivateHunterModeAfterDelay()); // REMOVED
    }

    // private IEnumerator DeactivateHunterModeAfterDelay() // REMOVED
    // { // REMOVED
    //     yield return new WaitForSeconds(hunterDuration); // REMOVED
    //     DeactivateHunterMode(); // REMOVED
    // } // REMOVED

    private void DeactivateHunterMode() // Made public to be accessible by event, or keep private if event is static
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

        // Reset the coroutine reference // REMOVED
        // hunterModeCoroutine = null; // REMOVED

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
        // if (hunterModeCoroutine != null) // REMOVED
        // { // REMOVED
        //     StopCoroutine(hunterModeCoroutine); // REMOVED
        //     DeactivateHunterMode(); // Call directly if needed, or rely on timer to end
        // } // REMOVED
        // If a timer is running, you might want to stop it via TimerManager
        // For now, DeactivateHunterMode will be called by the timer's event if it was running.
        // If you need an immediate stop, you'd add a StopHunterModeTimer to TimerManager and call it here.
        // For simplicity, we'll assume the natural end of the timer or a new game start handles this.
        if (isTransformed) // If hunter mode was active, ensure it's properly deactivated
        {
             DeactivateHunterMode(); // Manually call to reset state if ResetGameState is called mid-hunter mode
        }
    }
}