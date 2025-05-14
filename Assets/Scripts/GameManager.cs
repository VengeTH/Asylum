using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int collectedOrbs = 0;
    public int orbsRequiredForTransformation = 5;
    public float hunterDuration = 10f;

    public Camera firstPersonCam;
    public Camera thirdPersonCam;
    private GameObject player;

    public GameObject normalModel;
    public GameObject hunterModel;

    void ActivateHunterMode(){
        normalModel.SetActive(false);
        hunterModel.SetActive(true);
    }

    void Awake()
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
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void CollectOrb()
    {
        collectedOrbs++;
        Debug.Log($"Collected Orbs: {collectedOrbs}");

        if (collectedOrbs == orbsRequiredForTransformation)
        {
            ActivateHunterMode();
        }
    }

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
        foreach (EnemyAI enemy in FindObjectsOfType<EnemyAI>())
        {
            enemy.SetVulnerable(false);
        }
    }
}