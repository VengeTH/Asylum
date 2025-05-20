using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Orb Settings")]
    public int collectedOrbs = 0;
    public int orbsRequiredForTransformation = 1;
    public float hunterDuration = 20f;

    [Header("Objective Settings")]
    public int totalObjectivesInScene = 11;
    private int objectivesCollectedCount = 0;
    public static event System.Action OnGameWon;

    [Header("Timer Manager")]
    public TimerManager timerManager;

    [Header("Camera References")]
    public Camera firstPersonCam;
    public Camera thirdPersonCam;

    [Header("Player References")]
    public GameObject player;

    private EnemyAI[] cachedEnemies;
    private bool isTransformed = false;
    private bool gameWon = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ObjectiveCollectible.OnObjectiveCollected += HandleObjectiveCollected;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Find TimerManager if not assigned
        if (timerManager == null)
        {
            timerManager = FindAnyObjectByType<TimerManager>();
            if (timerManager == null)
            {
                Debug.LogError("TimerManager not found in the scene!");
            }
        }

        // Optional: Find cameras by name if not assigned
        if (firstPersonCam == null)
            firstPersonCam = GameObject.Find("Camera_FirstPerson")?.GetComponent<Camera>();
        if (thirdPersonCam == null)
            thirdPersonCam = GameObject.Find("Camera_ThirdPerson")?.GetComponent<Camera>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Show")
        {
            ResetGameState();
            // Re-find references in the new scene
            firstPersonCam = GameObject.Find("Camera_FirstPerson")?.GetComponent<Camera>();
            thirdPersonCam = GameObject.Find("Camera_ThirdPerson")?.GetComponent<Camera>();
            player = GameObject.FindGameObjectWithTag("Player");
            timerManager = FindAnyObjectByType<TimerManager>();

            // Set initial camera state
            if (firstPersonCam != null && thirdPersonCam != null)
            {
                firstPersonCam.enabled = true;
                thirdPersonCam.enabled = false;
                Debug.Log("Cameras found and initialized in new scene");
            }
            else
            {
                Debug.LogError($"Cameras not found in new scene. FirstPerson: {firstPersonCam != null}, ThirdPerson: {thirdPersonCam != null}");
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
            TimerManager.OnHunterModeTimerEnd += DeactivateHunterMode;
        }
    }

    private void OnDestroy()
    {
        if (timerManager != null)
        {
            TimerManager.OnHunterModeTimerEnd -= DeactivateHunterMode;
        }
        ObjectiveCollectible.OnObjectiveCollected -= HandleObjectiveCollected;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HandleObjectiveCollected()
    {
        if (gameWon) return;

        objectivesCollectedCount++;
        Debug.Log($"Objective collected! Total collected: {objectivesCollectedCount}/{totalObjectivesInScene}");

        // Check if it's time to trigger enemy aggressive mode
        if (objectivesCollectedCount > 0 && objectivesCollectedCount % 3 == 0)
        {
            Debug.Log($"Objective threshold reached ({objectivesCollectedCount}). Activating aggressive mode for enemies.");
            RefreshEnemiesCache(); // Ensure the cache is up-to-date, though likely static
            foreach (EnemyAI enemy in cachedEnemies)
            {
                if (enemy != null)
                {
                    enemy.ActivateObjectiveBasedAggression();
                }
            }
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (objectivesCollectedCount >= totalObjectivesInScene)
        {
            gameWon = true;
            Debug.Log("🎉 All objectives collected! YOU WIN! 🎉");
            OnGameWon?.Invoke();
            SceneManager.LoadScene("Win");
        }
    }

    private void RefreshEnemiesCache()
    {
        cachedEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
    }

    public void CollectOrb()
    {
        collectedOrbs++;
        Debug.Log($"Collected Orbs: {collectedOrbs}");

        if (!isTransformed && collectedOrbs >= orbsRequiredForTransformation)
        {
            isTransformed = true;
            ActivateHunterMode();
        }
    }

    public void ActivateHunterMode()
    {
        Debug.Log("🔥 Hunter Mode Activated!");

        // Re-find cameras if they're null
        if (firstPersonCam == null)
            firstPersonCam = GameObject.Find("Camera_FirstPerson")?.GetComponent<Camera>();
        if (thirdPersonCam == null)
            thirdPersonCam = GameObject.Find("Camera_ThirdPerson")?.GetComponent<Camera>();

        if (firstPersonCam != null && thirdPersonCam != null)
        {
            firstPersonCam.enabled = false;
            thirdPersonCam.enabled = true;
            Debug.Log("Switched to third person camera for hunter mode");
        }
        else
        {
            Debug.LogError($"Failed to switch cameras. FirstPerson: {firstPersonCam != null}, ThirdPerson: {thirdPersonCam != null}");
        }

        // Refresh enemy cache to ensure we have all enemies
        RefreshEnemiesCache();

        // Activate aggressive pursuit mode for all enemies
        foreach (EnemyAI enemy in cachedEnemies)
        {
            if (enemy != null)
            {
                enemy.ActivateObjectiveBasedAggression(); // This will set player invincible and enemy to aggressive mode
                Debug.Log($"Activated aggressive pursuit for enemy: {enemy.gameObject.name}");
            }
        }

        if (timerManager != null)
        {
            timerManager.StartHunterModeTimer(hunterDuration);
        }
        else
        {
            Debug.LogError("TimerManager reference is not set in GameManager. Cannot start hunter mode timer.");
        }
    }

    private void DeactivateHunterMode()
    {
        isTransformed = false;
        collectedOrbs = 0;

        if (firstPersonCam != null && thirdPersonCam != null)
        {
            firstPersonCam.enabled = true;
            thirdPersonCam.enabled = false;
        }

        // Refresh enemy cache
        RefreshEnemiesCache();

        // Make enemies flee when hunter mode ends
        foreach (EnemyAI enemy in cachedEnemies)
        {
            if (enemy != null)
            {
                enemy.FleeFromOrbAndDeactivateAggression();
                Debug.Log($"Deactivated aggressive pursuit for enemy: {enemy.gameObject.name}");
            }
        }
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
        objectivesCollectedCount = 0;
        gameWon = false;
        if (isTransformed)
        {
            DeactivateHunterMode();
        }
    }
}