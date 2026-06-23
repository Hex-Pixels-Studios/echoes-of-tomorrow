using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("ground check")]
    [SerializeField]
    Transform groundCheck;

    [SerializeField]
    float groundCheckRadius = 0.2f;

    [SerializeField]
    LayerMask groundMask;

    public enum PlayerID
    {
        P1,
        P2,
    }

    [Header("player identity")]
    [SerializeField]
    PlayerID playerID = PlayerID.P1;

    [SerializeField]
    Renderer playerRenderer;

    [SerializeField]
    Color p1Color = new Color(0.2f, 0.8f, 1f);

    [SerializeField]
    Color p2Color = new Color(1f, 0.4f, 0.1f);

    CharacterController cc;
    PlayerInput playerInput;

    InputAction moveAction;
    InputAction jumpAction;

    Vector3 velocity;
    float verticalVelocity;
    bool isGrounded;

    bool jumpBuffered;
    float jumpBufferTimer;
    const float JUMP_BUFFER = 0.12f;

    Vector3 moveDirection;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
    }

    void Start()
    {
        ApplyPlayerColor();
    }

    void ApplyPlayerColor()
    {
        if (playerRenderer == null)
            return;

        Color color = playerID == PlayerID.P1 ? p1Color : p2Color;
        playerRenderer.material.color = color;
    }

    void OnEnable() => jumpAction.performed += OnJumpPressed;

    void OnDisable() => jumpAction.performed -= OnJumpPressed;

    void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        jumpBuffered = true;
        jumpBufferTimer = JUMP_BUFFER;
    }

    void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleRotation();
        HandleJump();
        ApplyGravity();
        ApplyMotion();
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;
    }

    void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        // world space - camera is fixed topdown so world forward/right maps directly to wasd/stick
        Vector3 moveDir = new Vector3(input.x, 0f, input.y).normalized;

        // cache for rotation to follow
        if (moveDir.sqrMagnitude > 0.01f)
            moveDirection = moveDir;

        float rate = moveDir.sqrMagnitude > 0.01f ? acceleration : deceleration;
        velocity = Vector3.MoveTowards(velocity, moveDir * moveSpeed, rate * Time.deltaTime);
    }

    void HandleRotation()
    {
        // rotate to face the direction of movement - no look action needed
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
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

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    public Vector3 Velocity => velocity;
    public bool IsGrounded => isGrounded;
    public float VerticalVelocity => verticalVelocity;
    public Vector3 FacingDirection =>
        moveDirection.sqrMagnitude > 0f ? moveDirection.normalized : transform.forward;

    public PlayerID ID => playerID;
}
