using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvulnerable = false;
    public float invulnerabilityDuration = 0.5f;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged;

    [Header("UI Effects")]
    public CanvasGroup damageFlashCanvas;
    public float flashDuration = 0.2f;

    private float invulnerabilityTimer;

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

    void Update()
    {
        // Update invulnerability timer
        if (isInvulnerable && invulnerabilityTimer > 0)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

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
            else
            {
                // Temporary invulnerability
                isInvulnerable = true;
                invulnerabilityTimer = invulnerabilityDuration;
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
        // You can add additional death logic here
    }

    public void Heal(float amount)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (currentHealth != previousHealth)
        {
            Debug.Log($"Player healed for {amount}. New health: {currentHealth}/{maxHealth}");
            onHealthChanged?.Invoke(currentHealth);
        }
    }
}