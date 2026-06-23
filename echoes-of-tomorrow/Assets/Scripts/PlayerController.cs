using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
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
    Material player1Material;

    [SerializeField]
    Material player2Material;

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

        PlayerRegistry.Register(this);
    }

    void OnDestroy()
    {
        PlayerRegistry.Unregister(this);
    }

    void Start()
    {
        if (playerRenderer != null)
            playerRenderer.material =
                playerInput.playerIndex == 0 ? player1Material : player2Material;
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
        Vector3 moveDir = new Vector3(input.x, 0f, input.y).normalized;
        if (moveDir.sqrMagnitude > 0.01f)
            moveDirection = moveDir;
        float rate = moveDir.sqrMagnitude > 0.01f ? acceleration : deceleration;
        velocity = Vector3.MoveTowards(velocity, moveDir * moveSpeed, rate * Time.deltaTime);
    }

    void HandleRotation()
    {
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
