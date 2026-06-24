using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerUpgrade))]
[RequireComponent(typeof(PlayerInput))]
public class CombatSystem : MonoBehaviour
{
    [Header("projectile prefabs")]
    [SerializeField]
    GameObject defaultProjectilePrefab;

    [SerializeField]
    GameObject grenadeProjectilePrefab;

    [SerializeField]
    GameObject fireMissileProjectilePrefab;

    [Header("fire point")]
    [SerializeField]
    Transform firePoint;

    [Header("default attack")]
    [SerializeField]
    float defaultDamage = 10f;

    [SerializeField]
    float defaultSpeed = 14f;

    [SerializeField]
    float defaultKnockback = 5f;

    [SerializeField]
    float defaultKnockbackUp = 2f;

    [SerializeField]
    float defaultLifetime = 2f;

    [SerializeField]
    float defaultFireRate = 0.4f;

    [Header("grenade")]
    [SerializeField]
    float grenadeDamage = 35f;

    [SerializeField]
    float grenadeSpeed = 10f;

    [SerializeField]
    float grenadeKnockback = 15f;

    [SerializeField]
    float grenadeKnockbackUp = 6f;

    [SerializeField]
    float grenadeLifetime = 3f;

    [Header("fire missile")]
    [SerializeField]
    float missileDamage = 45f;

    [SerializeField]
    float missileSpeed = 20f;

    [SerializeField]
    float missileKnockback = 18f;

    [SerializeField]
    float missileKnockbackUp = 5f;

    [SerializeField]
    float missileLifetime = 4f;

    [Header("ice freeze")]
    [SerializeField]
    float freezeDuration = 2.5f;

    [Header("teleport")]
    [SerializeField]
    Transform[] teleportPoints;

    PlayerUpgrade playerUpgrade;
    PlayerInput playerInput;
    InputAction attackAction;

    float fireCooldown;

    public event System.Action<UpgradeType?> OnUpgradeChanged;

    void Awake()
    {
        playerUpgrade = GetComponent<PlayerUpgrade>();
        playerInput = GetComponent<PlayerInput>();
        attackAction = playerInput.actions["Attack"];
    }

    void OnEnable() => attackAction.performed += OnAttackPressed;

    void OnDisable() => attackAction.performed -= OnAttackPressed;

    void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    void OnAttackPressed(InputAction.CallbackContext ctx)
    {
        if (fireCooldown > 0f)
            return;

        if (playerUpgrade.HasUpgrade)
            FireUpgrade();
        else
            FireDefault();
    }

    void FireDefault()
    {
        SpawnProjectile(
            defaultProjectilePrefab,
            CombatProjectile.ProjectileType.Default,
            defaultSpeed,
            defaultDamage,
            defaultKnockback,
            defaultKnockbackUp,
            defaultLifetime
        );

        fireCooldown = defaultFireRate;
    }

    void FireUpgrade()
    {
        UpgradeType upgrade = playerUpgrade.CurrentUpgrade.Value;

        switch (upgrade)
        {
            case UpgradeType.Grenade:
                SpawnProjectile(
                    grenadeProjectilePrefab,
                    CombatProjectile.ProjectileType.Grenade,
                    grenadeSpeed,
                    grenadeDamage,
                    grenadeKnockback,
                    grenadeKnockbackUp,
                    grenadeLifetime
                );
                break;

            case UpgradeType.FireMissile:
                SpawnProjectile(
                    fireMissileProjectilePrefab,
                    CombatProjectile.ProjectileType.FireMissile,
                    missileSpeed,
                    missileDamage,
                    missileKnockback,
                    missileKnockbackUp,
                    missileLifetime
                );
                break;

            case UpgradeType.IceFreeze:
                ExecuteIceFreeze();
                break;

            case UpgradeType.Teleport:
                ExecuteTeleport();
                break;
        }

        playerUpgrade.ConsumeUpgrade();

        OnUpgradeChanged?.Invoke(null);
    }

    void SpawnProjectile(
        GameObject prefab,
        CombatProjectile.ProjectileType type,
        float spd,
        float dmg,
        float kb,
        float kbUp,
        float life
    )
    {
        if (prefab == null)
        {
            Debug.LogWarning($"CombatSystem: no prefab for {type}");
            return;
        }

        GameObject proj = Instantiate(prefab, firePoint.position, firePoint.rotation);
        proj.GetComponent<CombatProjectile>()?.Init(gameObject, type, spd, dmg, kb, kbUp, life);
    }

    void ExecuteIceFreeze()
    {
        GameEvents.IceFreeze((int)GetComponent<PlayerController>().ID);
        var opponent = PlayerRegistry.GetOpponent(GetComponent<PlayerController>().ID);
        if (opponent != null)
            StartCoroutine(FreezeRoutine(opponent));
    }

    IEnumerator FreezeRoutine(PlayerController target)
    {
        Debug.Log($"{target.name} frozen for {freezeDuration}s");
        target.enabled = false;
        yield return new WaitForSeconds(freezeDuration);
        target.enabled = true;
        // Debug.Log($"{target.name} unfrozen");
    }

    void ExecuteTeleport()
    {
        GameEvents.Teleport((int)GetComponent<PlayerController>().ID);
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning("CombatSystem: no teleport points assigned");
            return;
        }

        Transform dest = teleportPoints[Random.Range(0, teleportPoints.Length)];
        int tries = 0;
        while (Vector3.Distance(transform.position, dest.position) < 3f && tries < 10)
        {
            dest = teleportPoints[Random.Range(0, teleportPoints.Length)];
            tries++;
        }

        var cc = GetComponent<CharacterController>();
        cc.enabled = false;
        transform.position = dest.position;
        cc.enabled = true;

        Debug.Log($"{name} teleported to {dest.name}");
    }

    public void NotifyUpgradeGranted(UpgradeType upgrade)
    {
        OnUpgradeChanged?.Invoke(upgrade);
    }
}
