using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged;

    [Header("UI Effects")]
    public CanvasGroup damageFlashCanvas;
    public float flashDuration = 0.2f;

    void Awake()
    {
        // Initialize events if they're null
        if (onDeath == null)
            onDeath = new UnityEvent();
        if (onHealthChanged == null)
            onHealthChanged = new UnityEvent<float>();

        // Set initial health
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Trigger initial health update
        onHealthChanged?.Invoke(currentHealth);
        Debug.Log($"Player Health Initialized: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Player taking damage: {damage}");
        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth != previousHealth)
        {
            onHealthChanged?.Invoke(currentHealth);
            Debug.Log($"Player health after damage: {currentHealth}/{maxHealth}");

            // Visual feedback
            if (damageFlashCanvas != null)
            {
                StartCoroutine(FlashDamageIndicator());
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    IEnumerator FlashDamageIndicator()
    {
        damageFlashCanvas.alpha = 1f;
        yield return new WaitForSeconds(flashDuration);
        damageFlashCanvas.alpha = 0f;
    }

    void Die()
    {
        Debug.Log("Player died!");
        onDeath?.Invoke();
        // Load lose scene when player dies
        SceneManager.LoadScene("Lose");
    }

    public void Heal(float amount)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (currentHealth != previousHealth)
        {
            Debug.Log($"Player healed for {amount}. New health: {currentHealth}/{maxHealth}");
            onHealthChanged?.Invoke(currentHealth);
        }
    }
}