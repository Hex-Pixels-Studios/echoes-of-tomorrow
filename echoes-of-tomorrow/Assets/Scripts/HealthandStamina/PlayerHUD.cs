using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("which player this HUD tracks")]
    [SerializeField]
    PlayerController.PlayerID trackedPlayer = PlayerController.PlayerID.P1;

    [Header("health bar")]
    [SerializeField]
    Image healthFill;

    [SerializeField]
    Image healthDrainFill;

    [SerializeField]
    TMP_Text healthLabel;

    [Header("stamina bar")]
    [SerializeField]
    Image staminaFill;

    [SerializeField]
    Image staminaDrainFill;

    [SerializeField]
    TMP_Text staminaLabel;

    [Header("ghost bar settings")]
    [SerializeField]
    float drainSpeed = 2f;

    [Header("stamina regen pulse")]
    [SerializeField]
    float pulseSpeed = 3f;

    [SerializeField]
    float pulseMinAlpha = 0.6f;

    [Header("upgrade slot")]
    [SerializeField]
    Image upgradeIcon;

    [SerializeField]
    TMP_Text upgradeLabel;

    [SerializeField]
    Sprite defaultAttackSprite;

    [SerializeField]
    Sprite grenadeSprite;

    [SerializeField]
    Sprite fireMissileSprite;

    [SerializeField]
    Sprite iceSprite;

    [SerializeField]
    Sprite teleportSprite;

    PlayerHealth playerHealth;
    PlayerStamina playerStamina;
    CombatSystem combatSystem;

    float targetHealthFill = 1f;
    float targetStaminaFill = 1f;

    void Start()
    {
        // players might not be spawned yet on the first frame

        StartCoroutine(BindWhenReady());
    }

    IEnumerator BindWhenReady()
    {
        PlayerController player = null;

        while (player == null)
        {
            player = PlayerRegistry.Get(trackedPlayer);
            if (player == null)
                yield return null; // wait a frame and try again
        }

        playerHealth = player.GetComponent<PlayerHealth>();
        playerStamina = player.GetComponent<PlayerStamina>();
        combatSystem = player.GetComponent<CombatSystem>();

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            playerHealth.OnDeath += HandleDeath;
            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        if (playerStamina != null)
        {
            playerStamina.OnStaminaChanged += HandleStaminaChanged;
            HandleStaminaChanged(playerStamina.CurrentStamina, playerStamina.MaxStamina);
        }

        if (combatSystem != null)
            combatSystem.OnUpgradeChanged += HandleUpgradeChanged;

        SetUpgradeIcon(null);
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
            playerHealth.OnDeath -= HandleDeath;
        }
        if (playerStamina != null)
            playerStamina.OnStaminaChanged -= HandleStaminaChanged;
        if (combatSystem != null)
            combatSystem.OnUpgradeChanged -= HandleUpgradeChanged;
    }

    void Update()
    {
        AnimateGhostBar(healthDrainFill, ref targetHealthFill);
        AnimateGhostBar(staminaDrainFill, ref targetStaminaFill);
        AnimateStaminaPulse();
    }

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

    void HandleUpgradeChanged(UpgradeType? upgrade)
    {
        SetUpgradeIcon(upgrade);
    }

    void SetUpgradeIcon(UpgradeType? upgrade)
    {
        if (upgradeIcon != null)
        {
            upgradeIcon.sprite = upgrade switch
            {
                UpgradeType.Grenade => grenadeSprite,
                UpgradeType.FireMissile => fireMissileSprite,
                UpgradeType.IceFreeze => iceSprite,
                UpgradeType.Teleport => teleportSprite,
                _ => defaultAttackSprite,
            };
        }

        if (upgradeLabel != null)
        {
            upgradeLabel.text = upgrade switch
            {
                UpgradeType.Grenade => "grenade",
                UpgradeType.FireMissile => "fire missile",
                UpgradeType.IceFreeze => "ice freeze",
                UpgradeType.Teleport => "teleport",
                _ => "default shot",
            };
        }
    }

    void AnimateGhostBar(Image ghost, ref float target)
    {
        if (ghost == null)
            return;
        ghost.fillAmount = Mathf.MoveTowards(ghost.fillAmount, target, drainSpeed * Time.deltaTime);
    }

    void AnimateStaminaPulse()
    {
        if (staminaFill == null || playerStamina == null)
            return;
        if (!playerStamina.IsRegenerating)
            return;
        float alpha = Mathf.Lerp(
            pulseMinAlpha,
            1f,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f
        );
        Color c = staminaFill.color;
        c.a = alpha;
        staminaFill.color = c;
    }
}
