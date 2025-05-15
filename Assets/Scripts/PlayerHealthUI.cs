using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;
    public TextMeshProUGUI healthText;
    public RectTransform fillArea; // Reference to the fill area

    private PlayerHealth playerHealth;
    private float targetHealth;
    public float smoothSpeed = 10f;

    void Start()
    {
        // Find the PlayerHealth component
        playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found in the scene!");
            return;
        }

        // Validate UI components
        if (healthSlider == null)
        {
            Debug.LogError("Health Slider not assigned!");
            return;
        }

        if (fillImage == null)
        {
            Debug.LogError("Fill Image not assigned!");
            return;
        }

        // Set up initial values
        healthSlider.minValue = 0;
        healthSlider.maxValue = playerHealth.maxHealth;
        healthSlider.value = playerHealth.currentHealth;
        targetHealth = playerHealth.currentHealth;

        // Configure the fill image
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        // Configure the fill image's RectTransform
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        if (fillRect != null)
        {
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
        }

        // Subscribe to health changes
        playerHealth.onHealthChanged.AddListener(UpdateHealthUI);

        // Set static "Health" text
        if (healthText != null)
        {
            healthText.text = "Health";
        }

        // Initial UI update
        UpdateHealthUI(playerHealth.currentHealth);
    }

    void Update()
    {
        if (healthSlider == null || playerHealth == null) return;

        // Smooth health bar movement
        float currentFill = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);
        healthSlider.value = currentFill;
        fillImage.fillAmount = currentFill / playerHealth.maxHealth;
    }

    void UpdateHealthUI(float currentHealth)
    {
        targetHealth = currentHealth;

        if (currentHealth <= 0)
        {
            // Player died
            Debug.Log("Player died!");
            // You can add game over logic here
        }
    }

    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
            if (healthText != null)
            {
                healthText.text = "Health";
            }
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.RemoveListener(UpdateHealthUI);
        }
    }
}