using UnityEngine;
using UnityEngine.SceneManagement;

public class PressAnyKeyToContinue : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float minimumTimeToWait = 1f; // Minimum time to wait before accepting input

    private float startTime;

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        // Check if minimum time has passed
        if (Time.time - startTime < minimumTimeToWait)
            return;

        // Check for any key press
        if (Input.anyKeyDown)
        {
            LoadMainMenu();
        }
    }

    private void LoadMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}