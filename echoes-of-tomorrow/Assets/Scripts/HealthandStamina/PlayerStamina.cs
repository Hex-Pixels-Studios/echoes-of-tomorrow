using System;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField]
    float maxStamina = 100f;

    [SerializeField]
    float currentStamina;

    [Header("Regeneration")]
    [SerializeField]
    float regenRate = 15f;

    [SerializeField]
    float regenDelay = 1.5f;

    float regenTimer;
    bool isRegenerating;

    //For subscribing to UI components or ability systems
    public event Action<float, float> OnStaminaChanged;
    public event Action OnStaminaDepleted;

    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public float StaminaPercent => currentStamina / maxStamina;

    public bool IsRegenerating => isRegenerating;

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        HandleRegen();
    }

    void HandleRegen()
    {
        if (currentStamina >= maxStamina)
        {
            isRegenerating = false;
            return;
        }

        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            isRegenerating = false;
            return;
        }

        isRegenerating = true;
        currentStamina = Mathf.Min(currentStamina + regenRate * Time.deltaTime, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina < amount)
            return false;

        currentStamina -= amount;
        regenTimer = regenDelay;
        isRegenerating = false;

        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        if (currentStamina <= 0f)
            OnStaminaDepleted?.Invoke();

        return true;
    }

    public void AddStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    //This will drain all the stamina that the player has all at once
    public void ExhaustionEffect()
    {
        currentStamina = 0f;
        regenTimer = regenDelay;
        isRegenerating = false;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        OnStaminaDepleted?.Invoke();
    }

    public void IncreaseMaxStamina(float amount)
    {
        maxStamina += amount;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

#if UNITY_EDITOR
    [ContextMenu("Debug / Use 20 stamina")]
    void DebugUse() => UseStamina(20f);

    [ContextMenu("Debug / Add 20 stamina")]
    void DebugAdd() => AddStamina(20f);
#endif
}
