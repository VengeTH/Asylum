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
    public int totalObjectivesInScene = 3;
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
    }

    private void HandleObjectiveCollected()
    {
        if (gameWon) return;

        objectivesCollectedCount++;
        Debug.Log($"Objective collected! Total collected: {objectivesCollectedCount}/{totalObjectivesInScene}");

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

        if (firstPersonCam != null && thirdPersonCam != null)
        {
            firstPersonCam.enabled = false;
            thirdPersonCam.enabled = true;
        }

        foreach (EnemyAI enemy in cachedEnemies)
        {
            if (enemy != null)
                enemy.SetVulnerable(true);
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

        foreach (EnemyAI enemy in cachedEnemies)
        {
            if (enemy != null)
                enemy.SetVulnerable(false);
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
        if (isTransformed)
        {
            DeactivateHunterMode();
        }
    }
}