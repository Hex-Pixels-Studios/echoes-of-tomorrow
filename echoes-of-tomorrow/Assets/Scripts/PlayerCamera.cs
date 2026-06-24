using UnityEngine;

// follows its assigned player from a fixed topdown-ish angle
// spawned at runtime by PlayerController - no prefab camera needed
public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Vector3 offset = new Vector3(0f, 12f, -6f);
    [SerializeField] float followSpeed = 8f;
    [SerializeField] float rotationX = 55f;   // tilt angle - 90 = pure topdown, lower = more perspective

    Transform target;

    public void Init(Transform playerTransform)
    {
        target = playerTransform;
        transform.rotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
    }
}
