using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPopup; // Assign in Inspector

    public void PlayGame()
    {
        SceneManager.LoadScene("Show"); // Loads the scene named "Show"
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game exited.");
    }

    public void ShowOptionsMessage()
    {
        Debug.Log("ShowOptionsMessage called!");
        if (optionsPopup == null)
            Debug.LogError("optionsPopup is NOT assigned in the Inspector!");
        else
        {
            optionsPopup.SetActive(true);
            Debug.Log("optionsPopup.SetActive(true) called. Active state: " + optionsPopup.activeSelf);
        }
    }

    public void HideOptionsMessage()
    {
        if (optionsPopup != null)
            optionsPopup.SetActive(false);
    }

    void Update()
    {
        if (optionsPopup != null && optionsPopup.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HideOptionsMessage();
        }
    }
}
