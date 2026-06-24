using System;
using UnityEngine;

public class PlayerEcho : MonoBehaviour
{
    [Header("knockback")]
    [SerializeField]
    float knockbackForce = 12f;

    [SerializeField]
    float knockbackUpForce = 4f;

    [Header("vitals amounts")]
    [SerializeField]
    float healSmallAmount = 30f;

    [SerializeField]
    float staminaBoostAmount = 40f;

    PlayerController.PlayerID ownerID;
    Action<PlayerController.PlayerID> onCollected;

    static readonly UpgradeType[] combatPool = new UpgradeType[]
    {
        UpgradeType.Grenade,
        UpgradeType.FireMissile,
        UpgradeType.IceFreeze,
        UpgradeType.Teleport,
    };

    static readonly UpgradeType[] vitalsPool = new UpgradeType[]
    {
        UpgradeType.HealSmall,
        UpgradeType.HealFull,
        UpgradeType.StaminaBoost,
        UpgradeType.StaminaFull,
    };

    const float combat_WEIGHT = 0.7f;

    public void Init(
        PlayerController.PlayerID id,
        Action<PlayerController.PlayerID> collectedCallback
    )
    {
        ownerID = id;
        onCollected = collectedCallback;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            return;

        if (player.ID == ownerID)
            CollectEcho(player);
        else
            RejectPlayer(player);
    }

    void CollectEcho(PlayerController player)
    {
        GameEvents.EchoCollected((int)player.ID);
        GameEvents.UpgradeChosen((int)player.ID);

        
        bool pickcombat = UnityEngine.Random.value < combat_WEIGHT;
        UpgradeType upgrade = pickcombat
            ? combatPool[UnityEngine.Random.Range(0, combatPool.Length)]
            : vitalsPool[UnityEngine.Random.Range(0, vitalsPool.Length)];

        if (Isvitals(upgrade))
            ApplyvitalsNow(player.gameObject, upgrade);
        else
            player.GetComponent<PlayerUpgrade>()?.GrantUpgrade(upgrade);

        Debug.Log($"{ownerID} collected echo - got {upgrade}");
        onCollected?.Invoke(ownerID);
        Destroy(gameObject);
    }

    bool Isvitals(UpgradeType upgrade)
    {
        return upgrade == UpgradeType.HealSmall
            || upgrade == UpgradeType.HealFull
            || upgrade == UpgradeType.StaminaBoost
            || upgrade == UpgradeType.StaminaFull;
    }

    void ApplyvitalsNow(GameObject player, UpgradeType upgrade)
    {
        var health = player.GetComponent<PlayerHealth>();
        var stamina = player.GetComponent<PlayerStamina>();

        switch (upgrade)
        {
            case UpgradeType.HealSmall:
                health?.Heal(healSmallAmount);
                Debug.Log($"{ownerID} instantly healed {healSmallAmount}hp");
                break;

            case UpgradeType.HealFull:
                health?.Heal(float.MaxValue);
                Debug.Log($"{ownerID} fully healed");
                break;

            case UpgradeType.StaminaBoost:
                stamina?.AddStamina(staminaBoostAmount);
                Debug.Log($"{ownerID} stamina boosted by {staminaBoostAmount}");
                break;

            case UpgradeType.StaminaFull:
                stamina?.AddStamina(float.MaxValue);
                Debug.Log($"{ownerID} stamina fully restored");
                break;
        }
    }

    void RejectPlayer(PlayerController player)
    {
        GameEvents.EchoRejected((int)player.ID);
        PlayerKnockback knockback = player.GetComponent<PlayerKnockback>();
        if (knockback == null)
            return;

        Vector3 awayDir = (player.transform.position - transform.position).normalized;
        awayDir.y = 0f;
        knockback.ApplyKnockback(awayDir * knockbackForce + Vector3.up * knockbackUpForce);

        Debug.Log($"{player.ID} rejected from {ownerID}'s echo - knocked back");
    }
}
