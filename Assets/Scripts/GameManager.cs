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
        }

        // Optional: Find cameras by name if not assigned
        if (firstPersonCam == null)
            firstPersonCam = GameObject.Find("Camera_FirstPerson")?.GetComponent<Camera>();
        if (thirdPersonCam == null)
            thirdPersonCam = GameObject.Find("Camera_ThirdPerson")?.GetComponent<Camera>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }

    /// <summary>
    /// Call this when an orb is collected
    /// </summary>
    public void CollectOrb()
    {
        collectedOrbs++;
        Debug.Log($"Collected Orbs: {collectedOrbs}");

        if (collectedOrbs >= orbsRequiredForTransformation)
        {
            ActivateHunterMode();
        }
    }

    /// <summary>
    /// Activates Hunter Mode
    /// </summary>
    public void ActivateHunterMode()
    {
        Debug.Log("🔥 Hunter Mode Activated!");

        // Switch Cameras
        if (firstPersonCam != null) firstPersonCam.gameObject.SetActive(false);
        if (thirdPersonCam != null) thirdPersonCam.gameObject.SetActive(true);

        // Optional: Change character appearance
        if (player.TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material.color = Color.red; // Change to red or any predator look
        }

        // Notify enemies to enter vulnerable state
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.SetVulnerable(true);
        }

        // Start timer to revert back
        StartCoroutine(DeactivateHunterModeAfterDelay(hunterDuration));
    }

    /// <summary>
    /// Deactivates Hunter Mode after time runs out
    /// </summary>
    IEnumerator DeactivateHunterModeAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Revert camera
        if (firstPersonCam != null) firstPersonCam.gameObject.SetActive(true);
        if (thirdPersonCam != null) thirdPersonCam.gameObject.SetActive(false);

        // Revert color
        if (player.TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material.color = Color.white;
        }

        // Enemies go back to normal behavior
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.SetVulnerable(false);
        }

        Debug.Log("🛡️ Hunter Mode Ended!");
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
}