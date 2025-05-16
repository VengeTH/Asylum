using TMPro;
using UnityEngine;
using UnityEngine.UI; // Still needed if you have other UI.Text elements or for the original overload

public class TimerManager : MonoBehaviour
{
    [Header("Main Game Timer")]
    public TextMeshProUGUI mainTimerText; // Assign your TextMeshProUGUI element for the main timer here
    public float mainGameDurationMinutes = 15f;
    private float mainTimerSeconds;
    private bool isMainTimerRunning = false;

    [Header("Hunter Mode Timer")]
    public GameObject hunterModeTimerPanel; // Assign the UI Panel/GameObject that contains the hunter timer UI
    public TextMeshProUGUI hunterModeTimerText; // << CHANGED TO TextMeshProUGUI
    public float defaultHunterModeDurationSeconds = 60f; // Default duration if not specified
    private float hunterModeTimeRemaining;
    private bool isHunterModeTimerRunning = false;

    // Optional: Events for other scripts to subscribe to
    public static event System.Action OnMainTimerEnd;
    public static event System.Action OnHunterModeTimerEnd;

    void Start()
    {
        // Initialize Main Timer
        mainTimerSeconds = mainGameDurationMinutes * 60f;
        UpdateTimeDisplay(mainTimerText, mainTimerSeconds); // This will now call the TMP overload
        // Example: Start the main timer automatically. You might want to call this from a GameManager.
        // StartMainTimer(); 

        // Initialize Hunter Mode Timer (hidden by default)
        if (hunterModeTimerPanel != null)
        {
            hunterModeTimerPanel.SetActive(false);
            hunterModeTimerText.gameObject.SetActive(false); // Hide the timer text element
        }
        hunterModeTimeRemaining = 0f; // Will be set when hunter mode starts
    }

    void Update()
    {
        if (isMainTimerRunning)
        {
            if (mainTimerSeconds > 0)
            {
                mainTimerSeconds -= Time.deltaTime;
                UpdateTimeDisplay(mainTimerText, mainTimerSeconds); // This will now call the TMP overload
            }
            else
            {
                mainTimerSeconds = 0;
                isMainTimerRunning = false;
                UpdateTimeDisplay(mainTimerText, mainTimerSeconds); // This will now call the TMP overload
                MainTimerEnded();
            }
        }

        if (isHunterModeTimerRunning)
        {
            if (hunterModeTimeRemaining > 0)
            {
                hunterModeTimeRemaining -= Time.deltaTime;
                UpdateTimeDisplay(hunterModeTimerText, hunterModeTimeRemaining); // Will now call the TMP overload
            }
            else
            {
                hunterModeTimeRemaining = 0;
                isHunterModeTimerRunning = false;
                UpdateTimeDisplay(hunterModeTimerText, hunterModeTimeRemaining); // Will now call the TMP overload
                HunterModeTimerEnded();
            }
        }
    }

    // Original UpdateTimeDisplay for UnityEngine.UI.Text - Keep if you might use it elsewhere
    private void UpdateTimeDisplay(Text timerTextElement, float timeInSeconds)
    {
        if (timerTextElement == null) return;

        float minutes = Mathf.FloorToInt(timeInSeconds / 60);
        float seconds = Mathf.FloorToInt(timeInSeconds % 60);
        timerTextElement.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Overload for TextMeshProUGUI
    private void UpdateTimeDisplay(TextMeshProUGUI timerTextElement, float timeInSeconds)
    {
        if (timerTextElement == null) return;

        float minutes = Mathf.FloorToInt(timeInSeconds / 60);
        float seconds = Mathf.FloorToInt(timeInSeconds % 60);
        timerTextElement.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // --- Main Game Timer Controls ---
    public void StartMainTimer()
    {
        mainTimerSeconds = mainGameDurationMinutes * 60f; // Reset if called again
        isMainTimerRunning = true;
        Debug.Log("Main game timer started.");
    }

    public void StopMainTimer()
    {
        isMainTimerRunning = false;
        Debug.Log("Main game timer stopped.");
    }

    private void MainTimerEnded()
    {
        Debug.Log("Main Timer Ended! Checking objectives...");
        OnMainTimerEnd?.Invoke(); // Invoke event

        // TODO: Implement your game over logic here.
        // This typically involves checking if objectives are met.
        // For example, you might call a method in your GameManager:
        // if (GameManager.Instance != null && !GameManager.Instance.AreObjectivesMet())
        // {
        //     GameManager.Instance.PlayerLose();
        // }
        // else if (GameManager.Instance == null)
        // {
        //     Debug.LogError("GameManager instance not found to handle game over.");
        // }
        Debug.Log("PLAYER LOSES (Placeholder - implement objective check and game over sequence)");
    }

    // --- Hunter Mode Timer Controls ---
    public void StartHunterModeTimer(float durationSeconds)
    {
        hunterModeTimeRemaining = durationSeconds;
        isHunterModeTimerRunning = true;
        if (hunterModeTimerPanel != null)
        {
            hunterModeTimerPanel.SetActive(true);
            hunterModeTimerText.gameObject.SetActive(true); // Show the timer text element
            Debug.Log("HunterModeTimerPanel.SetActive(true) called. Panel active in hierarchy: " + hunterModeTimerPanel.activeInHierarchy + ", Panel self active: " + hunterModeTimerPanel.activeSelf);
        }
        else
        {
            Debug.LogError("HunterModeTimerPanel is null in StartHunterModeTimer!");
        }
        UpdateTimeDisplay(hunterModeTimerText, hunterModeTimeRemaining); // Will now call the TMP overload
        Debug.Log($"Hunter Mode started for {durationSeconds} seconds.");
    }

    public void StartHunterModeTimer() // Overload to use default duration
    {
        StartHunterModeTimer(defaultHunterModeDurationSeconds);
    }

    private void HunterModeTimerEnded()
    {
        if (hunterModeTimerPanel != null)
        {
            hunterModeTimerPanel.SetActive(false);
            hunterModeTimerText.gameObject.SetActive(false);

        }
        Debug.Log("Hunter Mode Ended!");
        OnHunterModeTimerEnd?.Invoke(); // Invoke event

        // TODO: Implement logic for when hunter mode ends
        // e.g., revert player abilities, change AI behavior, etc.
    }

    // --- Utility ---
    public bool IsMainTimerRunning()
    {
        return isMainTimerRunning;
    }

    public float GetMainTimeRemaining()
    {
        return mainTimerSeconds;
    }
}