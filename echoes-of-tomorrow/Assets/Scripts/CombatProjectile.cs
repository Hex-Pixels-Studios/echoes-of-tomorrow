using UnityEngine;

public class CombatProjectile : MonoBehaviour
{
    float speed;
    float damage;
    float knockbackForce;
    float knockbackUpForce;
    float lifetime;
    GameObject owner;
    ProjectileType projectileType;

    public enum ProjectileType
    {
        Default,
        Grenade,
        FireMissile,
    }

    public void Init(
        GameObject ownerObj,
        ProjectileType type,
        float spd,
        float dmg,
        float kbForce,
        float kbUp,
        float life
    )
    {
        owner = ownerObj;
        projectileType = type;
        speed = spd;
        damage = dmg;
        knockbackForce = kbForce;
        knockbackUpForce = kbUp;
        lifetime = life;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // ignore owner
        if (other.gameObject == owner)
            return;

        if (other.GetComponent<CombatProjectile>() != null)
            return;

        PlayerController hit = other.GetComponent<PlayerController>();
        if (hit != null)
        {
            hit.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            PlayerKnockback kb = hit.GetComponent<PlayerKnockback>();
            if (kb != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                dir.y = 0f;
                kb.ApplyKnockback(dir * knockbackForce + Vector3.up * knockbackUpForce);
            }

            Debug.Log($"{projectileType} hit {hit.name} for {damage} damage");
        }

        Destroy(gameObject);
    }
}
