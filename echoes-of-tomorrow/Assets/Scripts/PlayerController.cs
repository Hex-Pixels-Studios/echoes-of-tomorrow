using UnityEngine;
using UnityEngine.InputSystem;

// [RequireComponent(typeof(CharacterController))]
// [RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("movement")]
    [SerializeField]
    float moveSpeed = 8f;

    [SerializeField]
    float acceleration = 20f;

    [SerializeField]
    float deceleration = 25f;

    [Header("rotation")]
    [SerializeField]
    float rotationSpeed = 720f;

    [Header("jump")]
    [SerializeField]
    float jumpHeight = 1.8f;

    [SerializeField]
    float gravity = -22f;

    [SerializeField]
    float fallMultiplier = 1.6f;

    [SerializeField]
    float groundCheckDistance = 0.08f;

    [SerializeField]
    LayerMask groundMask;

    CharacterController cc;
    PlayerInput playerInput;

    InputAction moveAction;
    InputAction lookAction;
    InputAction jumpAction;

    Vector3 velocity;
    float verticalVelocity;
    bool isGrounded;

    bool jumpBuffered;
    float jumpBufferTimer;
    const float JUMP_BUFFER = 0.12f;

    Vector3 aimDirection;

    Vector2 lastMousePosition;
    bool mouseMoved;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];

        aimDirection = transform.forward;
    }

    void OnEnable()
    {
        jumpAction.performed += OnJumpPressed;

        lookAction.performed += OnLookPerformed;
    }

    void OnDisable()
    {
        jumpAction.performed -= OnJumpPressed;
        lookAction.performed -= OnLookPerformed;
    }

    void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        jumpBuffered = true;
        jumpBufferTimer = JUMP_BUFFER;
    }

    void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        mouseMoved = true;
    }

    void Update()
    {
        CheckGrounded();
        HandleRotation();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        ApplyMotion();

        mouseMoved = false;
    }

    void CheckGrounded()
    {
        Vector3 sphereOrigin = transform.position + Vector3.up * cc.radius;
        isGrounded = Physics.CheckSphere(sphereOrigin, cc.radius + groundCheckDistance, groundMask);

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
    }

    void HandleRotation()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        if (playerInput.currentControlScheme == "Gamepad")
        {
            if (lookInput.sqrMagnitude > 0.15f)
                aimDirection = new Vector3(lookInput.x, 0f, lookInput.y);
        }
        else
        {
            if (mouseMoved)
            {
                Ray ray = Camera.main.ScreenPointToRay(lookInput);
                Plane groundPlane = new Plane(Vector3.up, transform.position);

                if (groundPlane.Raycast(ray, out float dist))
                {
                    Vector3 worldPoint = ray.GetPoint(dist);
                    Vector3 dir = worldPoint - transform.position;
                    dir.y = 0f;

                    if (dir.sqrMagnitude > 0.5f)
                        aimDirection = dir;
                }
            }
        }

        if (aimDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(aimDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 moveDir = (transform.forward * input.y + transform.right * input.x).normalized;

        float rate = moveDir.sqrMagnitude > 0.01f ? acceleration : deceleration;
        velocity = Vector3.MoveTowards(velocity, moveDir * moveSpeed, rate * Time.deltaTime);
    }

    void HandleJump()
    {
        if (jumpBuffered)
        {
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer <= 0f)
                jumpBuffered = false;
        }

        if (jumpBuffered && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
            jumpBuffered = false;
        }
    }

    void ApplyGravity()
    {
        if (isGrounded)
            return;
        float multiplier = verticalVelocity < 0f ? fallMultiplier : 1f;
        verticalVelocity += gravity * multiplier * Time.deltaTime;
    }

    void ApplyMotion()
    {
        cc.Move((velocity + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    public Vector3 Velocity => velocity;
    public bool IsGrounded => isGrounded;
    public float VerticalVelocity => verticalVelocity;
    public Vector3 AimDirection =>
        aimDirection.sqrMagnitude > 0f ? aimDirection.normalized : transform.forward;
}
