using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioClip universalBackgroundMusic; // Assign "scawy bg.wav" here in the Inspector

    private AudioSource musicSource;
    public float backgroundMusicVolume = 0.8f; // 80% volume

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = backgroundMusicVolume;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Play music for the initial scene if assigned
        if (universalBackgroundMusic != null)
        {
            musicSource.clip = universalBackgroundMusic;
            musicSource.Play();
            Debug.Log($"AudioManager: Playing universal background music '{universalBackgroundMusic.name}'.");
        }
        else
        {
            Debug.LogWarning("AudioManager: Universal background music not assigned.");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    void PlayMusicForScene(string sceneName) // sceneName is kept for potential future use but not used now
    {
        if (universalBackgroundMusic != null)
        {
            if (musicSource.clip != universalBackgroundMusic || !musicSource.isPlaying)
            {
                musicSource.clip = universalBackgroundMusic;
                musicSource.Play();
                Debug.Log($"AudioManager: Ensuring universal background music '{universalBackgroundMusic.name}' is playing for scene '{sceneName}'.");
            }
        }
        else
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }
            Debug.LogWarning($"AudioManager: Universal background music not assigned. Music stopped for scene '{sceneName}'.");
        }
    }

    // This method might be less relevant now, but can be used to override the universal music if needed.
    public void PlaySpecificMusic(AudioClip musicClip)
    {
        if (musicClip != null)
        {
            if (musicSource.clip != musicClip || !musicSource.isPlaying)
            {
                musicSource.clip = musicClip;
                musicSource.Play();
                Debug.Log($"AudioManager: Playing specific music '{musicClip.name}'.");
            }
        }
        else
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
                Debug.Log("AudioManager: Stopping music as null clip provided to PlaySpecificMusic.");
            }
        }
    }

    // Method to ensure the universal background music is playing (e.g., after returning from a state where it might have been stopped)
    public void EnsureUniversalMusicIsPlaying()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name); 
    }
}