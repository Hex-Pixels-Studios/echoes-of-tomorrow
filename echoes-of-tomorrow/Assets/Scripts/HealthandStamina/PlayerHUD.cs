using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Player reference")]
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] PlayerStamina playerStamina;

    [Header("Health bar")]
    [SerializeField] Image healthFill;
    [SerializeField] Image healthDrainFill;        
    [SerializeField] TMP_Text healthLabel;       

    [Header("Stamina bar")]
    [SerializeField] Image staminaFill;
    [SerializeField] Image staminaDrainFill;   
    [SerializeField] TMP_Text staminaLabel;          

    [Header("Ghost bar settings")]
    [SerializeField] float drainSpeed = 2f;     

    [Header("Stamina regen pulse")]
    [SerializeField] float pulseSpeed = 3f;
    [SerializeField] float pulseMinAlpha = 0.6f;

    float targetHealthFill = 1f;
    float targetStaminaFill = 1f;

    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            playerHealth.OnDeath += HandleDeath;
        }

        if (playerStamina != null)
            playerStamina.OnStaminaChanged += HandleStaminaChanged;
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
            playerHealth.OnDeath -= HandleDeath;
        }

        if (playerStamina != null)
            playerStamina.OnStaminaChanged -= HandleStaminaChanged;
    }

    void Start()
    {
        if (playerHealth != null)
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);

        if (playerStamina != null)
            HandleStaminaChanged(playerStamina.CurrentStamina, playerStamina.MaxStamina);
    }

    void Update()
    {
        AnimateGhostBar(healthDrainFill, ref targetHealthFill);
        AnimateGhostBar(staminaDrainFill, ref targetStaminaFill);
        AnimateStaminaPulse();
    }

    //Event handlers

    void HandleHealthChanged(float current, float max)
    {
        float pct = max > 0f ? current / max : 0f;

        if (healthFill != null)
            healthFill.fillAmount = pct;

        targetHealthFill = pct;        

        if (healthLabel != null)
            healthLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    void HandleStaminaChanged(float current, float max)
    {
        float pct = max > 0f ? current / max : 0f;

        if (staminaFill != null)
            staminaFill.fillAmount = pct;

        targetStaminaFill = pct;

        if (staminaLabel != null)
            staminaLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    void HandleDeath()
    {
        var group = GetComponent<CanvasGroup>();
        if (group != null)
            group.alpha = 0.4f;
    }

    //Ghost bar animation

    void AnimateGhostBar(Image ghost, ref float target)
    {
        if (ghost == null) return;
        ghost.fillAmount = Mathf.MoveTowards(ghost.fillAmount, target, drainSpeed * Time.deltaTime);
    }

    //Stamina regen pulse

    void AnimateStaminaPulse()
    {
        if (staminaFill == null || playerStamina == null) return;
        if (!playerStamina.IsRegenerating) return;

        // Gently pulse alpha to signal regen is active
        float alpha = Mathf.Lerp(pulseMinAlpha, 1f, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
        Color c = staminaFill.color;
        c.a = alpha;
        staminaFill.color = c;
    }
}