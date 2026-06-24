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

    [Header("camera")]
    [SerializeField]
    Vector3 cameraOffset = new Vector3(0f, 12f, -6f);

    [SerializeField]
    float cameraFollowSpeed = 8f;

    [SerializeField]
    float cameraRotationX = 55f;

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

    Camera playerCamera;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

        SpawnCamera();
        PlayerRegistry.Register(this);
    }

    void OnDestroy()
    {
        PlayerRegistry.Unregister(this);
        if (playerCamera != null)
            Destroy(playerCamera.gameObject);
    }

    void SpawnCamera()
    {
        GameObject camObj = new GameObject($"Camera_{playerID}");
        playerCamera = camObj.AddComponent<Camera>();
        playerCamera.fieldOfView = 65f;

        var follow = camObj.AddComponent<PlayerCamera>();
        follow.Init(transform);

        camObj.transform.position = transform.position + cameraOffset;
        camObj.transform.rotation = Quaternion.Euler(cameraRotationX, 0f, 0f);

        if (playerID == PlayerID.P1)
            camObj.AddComponent<AudioListener>();
    }

    void Start()
    {
        if (playerRenderer != null)
            playerRenderer.material = playerID == PlayerID.P1 ? player1Material : player2Material;
    }

    public void ResetPlayer()
    {
        velocity = Vector3.zero;
        verticalVelocity = 0f;
        jumpBuffered = false;

        GetComponent<PlayerHealth>()?.Heal(float.MaxValue);
        GetComponent<PlayerStamina>()?.AddStamina(float.MaxValue);
        GetComponent<PlayerUpgrade>()?.ConsumeUpgrade();

        if (!enabled)
            enabled = true;
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

        Vector3 camForward =
            playerCamera != null
                ? Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up).normalized
                : Vector3.forward;
        Vector3 camRight =
            playerCamera != null
                ? Vector3.ProjectOnPlane(playerCamera.transform.right, Vector3.up).normalized
                : Vector3.right;

        Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;

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

    public Camera PlayerCamera => playerCamera;
    public int PlayerIndex => playerInput.playerIndex;
    public Vector3 Velocity => velocity;
    public bool IsGrounded => isGrounded;
    public float VerticalVelocity => verticalVelocity;
    public Vector3 FacingDirection =>
        moveDirection.sqrMagnitude > 0f ? moveDirection.normalized : transform.forward;
    public PlayerID ID => playerID;
}
