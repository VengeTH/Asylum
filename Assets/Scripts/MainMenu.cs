using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject optionsPopup; // Assign in Inspector
    public Slider volumeSlider; // Assign in Inspector
    public TMPro.TextMeshProUGUI volumeValueText; // Assign in Inspector (for showing percentage)

    void Start()
    {
        // Initialize volume slider if assigned
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("GlobalVolume", 1f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
            UpdateVolumeText(savedVolume);
        }
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("GlobalVolume", value);
        PlayerPrefs.Save();
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeValueText != null)
            volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }

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
