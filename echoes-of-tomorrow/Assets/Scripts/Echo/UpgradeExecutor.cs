using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerUpgrade))]
[RequireComponent(typeof(PlayerInput))]
public class UpgradeExecutor : MonoBehaviour
{
    [Header("grenade / fire missile")]
    [SerializeField]
    float explosionRadius = 4f;

    [SerializeField]
    float explosionDamage = 35f;

    [SerializeField]
    float explosionKnockback = 15f;

    [SerializeField]
    float explosionUpForce = 6f;

    [SerializeField]
    LayerMask playerMask;

    [Header("ice freeze")]
    [SerializeField]
    float freezeDuration = 2.5f;

    [Header("teleport")]
    [SerializeField]
    Transform[] teleportPoints;

    PlayerUpgrade upgrade;
    PlayerInput playerInput;
    InputAction useAction;

    PlayerController opponent;

    void Awake()
    {
        upgrade = GetComponent<PlayerUpgrade>();
        playerInput = GetComponent<PlayerInput>();
        useAction = playerInput.actions["Use"];
    }

    void OnEnable() => useAction.performed += OnUsePressed;

    void OnDisable() => useAction.performed -= OnUsePressed;

    public void SetOpponent(PlayerController opponentController)
    {
        opponent = opponentController;
    }

    void OnUsePressed(InputAction.CallbackContext ctx)
    {
        if (!upgrade.HasUpgrade)
            return;

        switch (upgrade.CurrentUpgrade)
        {
            case UpgradeType.Grenade:
                ExecuteGrenade();
                break;
            case UpgradeType.FireMissile:
                ExecuteFireMissile();
                break;
            case UpgradeType.IceFreeze:
                ExecuteIceFreeze();
                break;
            case UpgradeType.Teleport:
                ExecuteTeleport();
                break;
        }

        upgrade.ConsumeUpgrade();
    }

    // grenade - damage + knockback around player's position
    // (no projectile yet -
    void ExecuteGrenade()
    {
        Debug.Log($"{gameObject.name} used Grenade");
        ExplodeAt(transform.position + transform.forward * 2f);
    }

    // fire missile -

    void ExecuteFireMissile()
    {
        Debug.Log($"{gameObject.name} used FireMissile");

        if (opponent == null)
        {
            return;
        }

        Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
        Vector3 hitPoint = Physics.Raycast(ray, out RaycastHit hit, 20f)
            ? hit.point
            : ray.GetPoint(20f);

        ExplodeAt(hitPoint);
    }

    void ExplodeAt(Vector3 point)
    {
        Debug.Log($"explosion at {point}");

        Collider[] hits = Physics.OverlapSphere(point, explosionRadius, playerMask);
        foreach (var col in hits)
        {
            if (col.gameObject == gameObject)
                continue;

            col.GetComponent<PlayerHealth>()?.TakeDamage(explosionDamage);

            PlayerKnockback kb = col.GetComponent<PlayerKnockback>();
            if (kb != null)
            {
                Vector3 dir = (col.transform.position - point).normalized;
                dir.y = 0f;
                kb.ApplyKnockback(dir * explosionKnockback + Vector3.up * explosionUpForce);
            }
        }
    }

    void ExecuteIceFreeze()
    {
        Debug.Log($"{gameObject.name} used IceFreeze");

        if (opponent == null)
        {
            Debug.LogWarning("UpgradeExecutor: opponent not assigned");
            return;
        }

        PlayerController opponentController = opponent.GetComponent<PlayerController>();
        if (opponentController != null)
            StartCoroutine(FreezePlayer(opponentController));
    }

    IEnumerator FreezePlayer(PlayerController target)
    {
        Debug.Log($"{target.name} frozen for {freezeDuration}s");
        target.enabled = false;
        yield return new WaitForSeconds(freezeDuration);
        target.enabled = true;
        Debug.Log($"{target.name} unfrozen");
    }

    void ExecuteTeleport()
    {
        if (teleportPoints == null || teleportPoints.Length == 0)
        {
            Debug.LogWarning("UpgradeExecutor: no teleport points assigned");
            return;
        }

        Transform dest = teleportPoints[Random.Range(0, teleportPoints.Length)];
        int attempts = 0;
        while (Vector3.Distance(transform.position, dest.position) < 3f && attempts < 10)
        {
            dest = teleportPoints[Random.Range(0, teleportPoints.Length)];
            attempts++;
        }

        CharacterController cc = GetComponent<CharacterController>();
        cc.enabled = false;
        transform.position = dest.position;
        cc.enabled = true;

        Debug.Log($"{gameObject.name} teleported to {dest.name}");
    }
}
