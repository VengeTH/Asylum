using UnityEngine;

public class OrbManager : MonoBehaviour
{
    public static OrbManager instance;
    private EnemyAI[] enemies;
    private int orbsCollectedCount = 0; // Kept for potential future use or UI, but no longer drives enemy aggression directly.
    // private bool isAggressiveModeGloballyActive = false; // This is no longer needed here.

    void Awake()
    {
        instance = this;
        enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
    }

    public void PlayerHasCollectedOrb()
    {
        // Incrementing orbsCollectedCount might still be useful for other game logic or UI, so we keep it.
        // However, it no longer directly controls enemy aggressive states here.
        // Incrementing orbsCollectedCount might still be useful for other game logic or UI, so we keep it.
        orbsCollectedCount++; 
        Debug.Log($"OrbManager: Orb collected by player. Total orbs collected this session: {orbsCollectedCount}");

        // Notify GameManager about the collected orb
        if (GameManager.instance != null)
        {
            GameManager.instance.CollectOrb();
        }
        else
        {
            Debug.LogError("GameManager instance not found in OrbManager!");
        }

        // The logic for making enemies flee is now handled by GameManager.ActivateHunterMode
        // or if Hunter Mode is not activated, enemies will react based on their standard AI.
        // We can remove the direct enemy manipulation from here to avoid redundancy and centralize control in GameManager.
        // foreach (EnemyAI enemy in enemies)
        // {
        //     if (enemy != null)
        //     {
        //         enemy.FleeFromOrbAndDeactivateAggression();
        //     }
        // }
        // Debug.Log("Orb collected. All enemies instructed to flee and deactivate aggression."); // This log is also now redundant
    }

    // The old ActivateHunterMode and DeactivateHunterMode are now managed by PlayerHasCollectedOrb
    /*
    public float hunterDuration = 10f; // This duration logic is not part of the new requirement

    public void ActivateHunterMode(float duration)
    {
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null)
                enemy.SetVulnerable(true); // SetVulnerable might be reused by UpdateStateOnOrbCollection
        }
        // Invoke("DeactivateHunterMode", duration); // Duration-based deactivation is different
    }

    void DeactivateHunterMode()
    {
        foreach (EnemyAI enemy in enemies)
        {
            if (enemy != null)
                enemy.SetVulnerable(false);
        }
    }
    */
}
