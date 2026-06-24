using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("health")]
    [SerializeField]
    float maxHealth = 100f;

    [SerializeField]
    float currentHealth;

    [Header("invincibility")]
    [SerializeField]
    bool useInvincibility = true;

    [SerializeField]
    float invincibilityDuration = 0.5f;

    float invincibilityTimer;
    bool isDead;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => currentHealth / maxHealth;
    public bool IsDead => isDead;

    void Awake() => currentHealth = maxHealth;

    void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(float amount)
    {
        GameEvents.PlayerHit((int)GetComponent<PlayerController>().ID);

        if (isDead)
            return;
        if (useInvincibility && invincibilityTimer > 0f)
            return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (useInvincibility)
            invincibilityTimer = invincibilityDuration;

        if (currentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        GameEvents.PlayerHealed((int)GetComponent<PlayerController>().ID);
        if (isDead)
            return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void InstantKill()
    {
        if (isDead)
            return;
        currentHealth = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Die();
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        invincibilityTimer = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        GameEvents.PlayerKilled((int)GetComponent<PlayerController>().ID);
        isDead = true;

        // stop the player moving
        var controller = GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;

        // stop the knockback too
        var knockback = GetComponent<PlayerKnockback>();
        if (knockback != null)
            knockback.enabled = false;

        OnDeath?.Invoke(); // GameManager.PlayerEliminated listens to this
    }

#if UNITY_EDITOR
    [ContextMenu("Debug / Take 10 damage")]
    void DebugDamage() => TakeDamage(10f);

    [ContextMenu("Debug / Heal 10")]
    void DebugHeal() => Heal(10f);

    [ContextMenu("Debug / Instant Kill")]
    void DebugKill() => InstantKill();
#endif
}
