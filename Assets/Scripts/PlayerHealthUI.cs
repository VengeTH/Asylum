using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;
    public TextMeshProUGUI healthText;
    public RectTransform fillArea;

    [Header("Player Reference")]
    public GameObject playerObject; // Assign this in the inspector

    private PlayerHealth playerHealth;
    private float targetHealth;
    public float smoothSpeed = 10f;

    void Awake()
    {
        Debug.Log("PlayerHealthUI Awake called");
        InitializeUI();
    }

    void Start()
    {
        Debug.Log("PlayerHealthUI Start called");
        if (playerHealth == null)
        {
            InitializeUI();
        }
    }

    void InitializeUI()
    {
        Debug.Log("Initializing UI components");

        // Debug all UI components
        Debug.Log($"UI Components - Slider: {(healthSlider != null ? "Found" : "Missing")}, " +
                  $"Fill Image: {(fillImage != null ? "Found" : "Missing")}, " +
                  $"Health Text: {(healthText != null ? "Found" : "Missing")}, " +
                  $"Fill Area: {(fillArea != null ? "Found" : "Missing")}");

        // Find the PlayerHealth component
        if (playerObject == null)
        {
            playerObject = GameObject.Find("PlayerOrc");
            if (playerObject == null)
            {
                Debug.LogError("Player GameObject not found! Please assign the player object in the inspector.");
                return;
            }
        }
        Debug.Log($"Found player object: {playerObject.name}");

        playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError($"PlayerHealth component not found on {playerObject.name}! Make sure the PlayerHealth script is attached to the player.");
            return;
        }
        Debug.Log($"Found PlayerHealth component on {playerObject.name}");

        // Validate UI components
        if (healthSlider == null)
        {
            Debug.LogError("Health Slider not assigned!");
            return;
        }
        Debug.Log("Health Slider found");

        if (fillImage == null)
        {
            Debug.LogError("Fill Image not assigned!");
            return;
        }
        Debug.Log("Fill Image found");

        // Set up initial values
        healthSlider.minValue = 0;
        healthSlider.maxValue = playerHealth.maxHealth;
        healthSlider.value = playerHealth.currentHealth;
        targetHealth = playerHealth.currentHealth;

        Debug.Log($"Initialized health slider - Min: {healthSlider.minValue}, Max: {healthSlider.maxValue}, Current: {healthSlider.value}");

        // Configure the fill image
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.color = new Color(1f, 0f, 0f, 1f); // Set to red color
        fillImage.fillAmount = healthSlider.value / healthSlider.maxValue;

        Debug.Log($"Initialized fill image - Fill amount: {fillImage.fillAmount}");

        // Configure the fill image's RectTransform
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        if (fillRect != null)
        {
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            Debug.Log("Configured fill image RectTransform");
        }
        else
        {
            Debug.LogError("Fill image RectTransform not found!");
        }

        // Subscribe to health changes
        if (playerHealth.onHealthChanged == null)
        {
            Debug.LogError("onHealthChanged event is null!");
            return;
        }

        // Remove any existing listeners first
        playerHealth.onHealthChanged.RemoveListener(UpdateHealthUI);
        // Add our listener
        playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
        Debug.Log("Subscribed to health changes");

        // Set initial health text
        if (healthText != null)
        {
            healthText.text = $"Health: {playerHealth.currentHealth}/{playerHealth.maxHealth}";
            Debug.Log($"Set initial health text: {healthText.text}");
        }
        else
        {
            Debug.LogWarning("Health text component not assigned");
        }

        // Initial UI update
        UpdateHealthUI(playerHealth.currentHealth);
        Debug.Log($"Initial health UI update: {playerHealth.currentHealth}/{playerHealth.maxHealth}");
    }

    void Update()
    {
        if (healthSlider == null || playerHealth == null)
        {
            Debug.LogWarning("Update called but healthSlider or playerHealth is null - attempting to reinitialize");
            InitializeUI();
            return;
        }

        // Smooth health bar movement
        float currentFill = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);
        healthSlider.value = currentFill;
        fillImage.fillAmount = currentFill / playerHealth.maxHealth;

        // Update health text if it exists
        if (healthText != null)
        {
            healthText.text = $"Health: {Mathf.RoundToInt(currentFill)}/{playerHealth.maxHealth}";
        }
    }

    void UpdateHealthUI(float currentHealth)
    {
        Debug.Log($"UpdateHealthUI called with health: {currentHealth}");
        targetHealth = currentHealth;

        // Force immediate update of slider and fill
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
            if (fillImage != null)
            {
                fillImage.fillAmount = currentHealth / playerHealth.maxHealth;
                Debug.Log($"Updated fill amount to: {fillImage.fillAmount}");
            }
            else
            {
                Debug.LogError("Fill image is null in UpdateHealthUI!");
            }
        }
        else
        {
            Debug.LogError("Health slider is null in UpdateHealthUI!");
        }

        // Update health text if it exists
        if (healthText != null)
        {
            healthText.text = $"Health: {Mathf.RoundToInt(currentHealth)}/{playerHealth.maxHealth}";
            Debug.Log($"Updated health text to: {healthText.text}");
        }

        Debug.Log($"Health UI updated - Target: {targetHealth}, Slider: {healthSlider.value}, Fill: {fillImage.fillAmount}");
    }

    void OnEnable()
    {
        if (playerHealth != null && playerHealth.onHealthChanged != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHealthUI);
            Debug.Log("Subscribed to health changes in OnEnable");
        }
    }

    void OnDisable()
    {
        if (playerHealth != null && playerHealth.onHealthChanged != null)
        {
            playerHealth.onHealthChanged.RemoveListener(UpdateHealthUI);
            Debug.Log("Unsubscribed from health changes in OnDisable");
        }
    }
}