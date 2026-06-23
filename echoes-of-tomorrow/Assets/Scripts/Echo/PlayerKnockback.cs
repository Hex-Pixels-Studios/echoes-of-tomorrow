using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerKnockback : MonoBehaviour
{
    [SerializeField]
    float knockbackDecay = 8f;

    CharacterController cc;
    Vector3 knockbackVelocity;

    void Awake() => cc = GetComponent<CharacterController>();

    void Update()
    {
        if (knockbackVelocity.sqrMagnitude < 0.01f)
            return;

        cc.Move(knockbackVelocity * Time.deltaTime);

        knockbackVelocity = Vector3.MoveTowards(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );
    }

    public void ApplyKnockback(Vector3 force)
    {
        knockbackVelocity += force;
    }
}
