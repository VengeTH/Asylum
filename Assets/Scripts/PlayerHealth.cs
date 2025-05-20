using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isInvincible = false;

    // Public property to check invincibility status
    public bool IsInvincible => isInvincible;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged;

    [Header("UI Effects")]
    public CanvasGroup damageFlashCanvas;
    public float flashDuration = 0.2f;

    void Awake()
    {
        Debug.Log("PlayerHealth Awake called");
        // Initialize events if they're null
        if (onDeath == null)
            onDeath = new UnityEvent();
        if (onHealthChanged == null)
            onHealthChanged = new UnityEvent<float>();

        // Set initial health
        currentHealth = maxHealth;
        Debug.Log($"PlayerHealth initialized with {currentHealth}/{maxHealth} health");
    }

    void Start()
    {
        Debug.Log("PlayerHealth Start called");
        // Trigger initial health update
        if (onHealthChanged != null)
        {
            Debug.Log("Invoking initial health changed event");
            onHealthChanged.Invoke(currentHealth);
        }
        else
        {
            Debug.LogError("onHealthChanged event is null in Start!");
        }
        Debug.Log($"Player Health Initialized: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"TakeDamage called with {damage} damage. Current health: {currentHealth}, Is invincible: {isInvincible}, IsInvincible property: {IsInvincible}");

        if (isInvincible)
        {
            Debug.Log("Player is invincible, no damage taken");
            return;
        }

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"Health after damage calculation: {currentHealth} (was {previousHealth})");

        if (currentHealth != previousHealth)
        {
            Debug.Log("Health changed, invoking events");
            if (onHealthChanged != null)
            {
                Debug.Log($"Invoking onHealthChanged event with new health: {currentHealth}");
                onHealthChanged.Invoke(currentHealth);
            }
            else
            {
                Debug.LogError("onHealthChanged event is null when trying to invoke it!");
            }
            Debug.Log($"Player health after damage: {currentHealth}/{maxHealth}");

            // Visual feedback
            if (damageFlashCanvas != null)
            {
                StartCoroutine(FlashDamageIndicator());
            }
            else
            {
                Debug.LogWarning("Damage flash canvas not assigned!");
            }

            if (currentHealth <= 0)
            {
                Debug.Log("Health reached 0, calling Die()");
                Die();
            }
        }
        else
        {
            Debug.Log("Health did not change after damage calculation");
        }
    }

    IEnumerator FlashDamageIndicator()
    {
        damageFlashCanvas.alpha = 1f;
        yield return new WaitForSeconds(flashDuration);
        damageFlashCanvas.alpha = 0f;
    }

    public void Die()
    {
        Debug.Log("Player died!");
        onDeath.Invoke();
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

    // Call this when entering hunter mode
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
        Debug.Log($"Player invincibility set to: {invincible} - Current state: isInvincible = {isInvincible}");
    }
}