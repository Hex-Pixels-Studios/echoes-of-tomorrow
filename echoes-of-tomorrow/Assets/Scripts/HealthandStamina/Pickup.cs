using NUnit.Framework.Internal.Commands;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType
    {
        Stamina,
        Health,
        MaxStamina,
        MaxHealth,
        FullRestore,
    }

    [Header("Pickup")]
    [SerializeField] PickupType type = PickupType.Stamina;

    [Tooltip("Amount restored or added.")]
    [SerializeField] float amount = 30f;

    [Header("Player filter")]
    [Tooltip("Uncheck to make this pickup work for any player")]
    [SerializeField] bool filterByPlayer = false;
    [SerializeField] PlayerController.PlayerID targetPlayer = PlayerController.PlayerID.P1;

    [Header("Feedback")]
    [SerializeField] GameObject collectEffect;
    [SerializeField] AudioClip collectSound;

    bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        PlayerController pc = other.GetComponent<PlayerController>();

        if (pc == null) return;

        if (filterByPlayer && pc.ID != targetPlayer) return;

        Apply(other.gameObject);
    }


    void Apply(GameObject player)
    {
        switch (type)
        {
            case PickupType.Stamina:
                player.GetComponent<PlayerStamina>()?.AddStamina(amount);
                break;

            case PickupType.Health:
                player.GetComponent<PlayerHealth>()?.Heal(amount);
                break;

            case PickupType.MaxStamina:
                // Permanently raise the cap, then fill the new headroom.
                var stamina = player.GetComponent<PlayerStamina>();
                if (stamina != null)
                {
                    stamina.IncreaseMaxStamina(amount);
                    stamina.AddStamina(amount);
                }
                break;

            case PickupType.MaxHealth:
                var health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.IncreaseMaxHealth(amount);
                    health.Heal(amount);
                }
                break;

            case PickupType.FullRestore:
                player.GetComponent<PlayerHealth>()?.Heal(float.MaxValue);
                player.GetComponent<PlayerStamina>()?.AddStamina(float.MaxValue);
                break;
        }

        Collect();
    }

    void Collect()
    {
        collected = true;

        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        Destroy(gameObject);
    }
}
