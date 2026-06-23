using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    float maxHealth = 100f;

    [SerializeField]
    float currentHealth;

    [Header("Invisbility")]
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

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;
        if (useInvincibility && invincibilityTimer > 0f)
            return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (useInvincibility)
        {
            invincibilityTimer = invincibilityDuration;

            if (currentHealth <= 0f)
            {
                Die();
            }
        }
    }

    public void Heal(float amount)
    {
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

    void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        GetComponent<PlayerController>().enabled = false;
    }

#if UNITY_EDITOR
    [ContextMenu("Debug / Take 10 damage")]
    void DebugDamage() => TakeDamage(10f);

    [ContextMenu("Debug / Heal 10")]
    void DebugHeal() => Heal(10f);
#endif
}
