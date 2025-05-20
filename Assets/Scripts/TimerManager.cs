using TMPro;
using UnityEngine;
using UnityEngine.UI; // Still needed if you have other UI.Text elements or for the original overload
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    [Header("Main Game Timer")]
    public TextMeshProUGUI mainTimerText; // Assign your TextMeshProUGUI element for the main timer here
    public float mainGameDurationMinutes = 15f;
    private float mainTimerSeconds;
    private bool isMainTimerRunning = false;

    [Header("Hunter Mode Timer")]
    public GameObject hunterModeTimerPanel; // Assign the UI Panel/GameObject that contains the hunter timer UI
    public GameObject panelTimer; // Reference to the child PanelTimer GameObject
    public TextMeshProUGUI hunterModeTimerText; // << CHANGED TO TextMeshProUGUI
    public float defaultHunterModeDurationSeconds = 60f; // Default duration if not specified
    private float hunterModeTimeRemaining;
    private bool isHunterModeTimerRunning = false;

    // Optional: Events for other scripts to subscribe to
    public static event System.Action OnMainTimerEnd;
    public static event System.Action OnHunterModeTimerEnd;

    void Awake()
    {
        // Ensure UI elements are properly referenced
        if (hunterModeTimerPanel == null)
        {
            hunterModeTimerPanel = GameObject.Find("HunterModeTimerPanel");
            if (hunterModeTimerPanel == null)
            {
                Debug.LogError("HunterModeTimerPanel not found in scene!");
            }
        }

        // Find the PanelTimer child
        if (panelTimer == null && hunterModeTimerPanel != null)
        {
            panelTimer = hunterModeTimerPanel.transform.Find("PanelTimer")?.gameObject;
            if (panelTimer == null)
            {
                Debug.LogError("PanelTimer child not found in HunterModeTimerPanel!");
            }
        }

        if (hunterModeTimerText == null)
        {
            hunterModeTimerText = GameObject.Find("HunterModeTimerText")?.GetComponent<TextMeshProUGUI>();
            if (hunterModeTimerText == null)
            {
                Debug.LogError("HunterModeTimerText not found in scene!");
            }
        }

        // Initialize hunter mode UI state
        if (hunterModeTimerPanel != null)
        {
            hunterModeTimerPanel.SetActive(false);
            if (panelTimer != null) panelTimer.SetActive(false);
            if (hunterModeTimerText != null) hunterModeTimerText.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        // Initialize Main Timer
        mainTimerSeconds = mainGameDurationMinutes * 60f;
        UpdateTimeDisplay(mainTimerText, mainTimerSeconds); // This will now call the TMP overload
        StartMainTimer(); // Start the timer automatically

        // Ensure Hunter Mode UI is hidden at start
        HideHunterModeUI();
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
        // Load lose scene when timer reaches zero
        SceneManager.LoadScene("Lose");
    }

    // --- Hunter Mode Timer Controls ---
    public void StartHunterModeTimer(float durationSeconds)
    {
        Debug.Log($"Starting Hunter Mode Timer for {durationSeconds} seconds");
        hunterModeTimeRemaining = durationSeconds;
        isHunterModeTimerRunning = true;
        ShowHunterModeUI();
        UpdateTimeDisplay(hunterModeTimerText, hunterModeTimeRemaining); // Will now call the TMP overload
    }

    public void StartHunterModeTimer() // Overload to use default duration
    {
        StartHunterModeTimer(defaultHunterModeDurationSeconds);
    }

    private void HunterModeTimerEnded()
    {
        // Hide Hunter Mode UI
        HideHunterModeUI();
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

    private void HideHunterModeUI()
    {
        if (hunterModeTimerPanel != null)
        {
            hunterModeTimerPanel.SetActive(false);
            if (panelTimer != null) panelTimer.SetActive(false);
            if (hunterModeTimerText != null) hunterModeTimerText.gameObject.SetActive(false);
            Debug.Log("Hunter Mode UI hidden");
        }
    }

    private void ShowHunterModeUI()
    {
        if (hunterModeTimerPanel != null)
        {
            hunterModeTimerPanel.SetActive(true);
            if (panelTimer != null) panelTimer.SetActive(true);
            if (hunterModeTimerText != null) hunterModeTimerText.gameObject.SetActive(true);
            Debug.Log("Hunter Mode UI shown");
        }
    }
}