using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [SerializeField] private Animator animator;

    private NetworkVariable<bool> isRunningNetwork = new NetworkVariable<bool>(false);
    private NetworkVariable<int> facingSignNetwork = new NetworkVariable<int>(1);

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool lastRunning;
    private int lastFacingSign = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("PlayerMovement: Rigidbody2D not found on this object!");
        }

        if (groundCheck == null)
        {
            Debug.LogError("PlayerMovement: groundCheck Transform not assigned!");
        }
    }

    public override void OnNetworkSpawn()
    {
        isRunningNetwork.OnValueChanged += OnRunningChanged;
        facingSignNetwork.OnValueChanged += OnFacingChanged;

        ApplyRunning(isRunningNetwork.Value);
        ApplyFacing(facingSignNetwork.Value);
    }

    public void OnMove(InputValue value)
    {
        if (!IsOwner) return;
        if (NetworkManager.Singleton == null) return;

        moveInput = value.Get<Vector2>();
        SubmitMovementServerRpc(moveInput);
    }

    public void OnJump(InputValue value)
    {
        if (!IsOwner) return;
        if (NetworkManager.Singleton == null) return;

        if (value.isPressed && isGrounded && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    [ServerRpc]
    private void SubmitMovementServerRpc(Vector2 input)
    {
        moveInput = input;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (rb == null || groundCheck == null) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        float speed = Mathf.Abs(moveInput.x);
        bool isRunning = speed > 0.1f;

        if (speed < 0.3f && Mathf.Abs(rb.linearVelocity.x) > 0.3f)
        {
            isRunning = true;
        }

        ApplyRunning(isRunning);
        if (isRunning != lastRunning)
        {
            lastRunning = isRunning;
            SetRunningServerRpc(isRunning);
        }

        if (moveInput.x != 0)
        {
            int facingSign = moveInput.x > 0 ? 1 : -1;
            ApplyFacing(facingSign);

            if (facingSign != lastFacingSign)
            {
                lastFacingSign = facingSign;
                SetFacingServerRpc(facingSign);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (rb == null) return;

        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    [ServerRpc]
    private void SetRunningServerRpc(bool value)
    {
        isRunningNetwork.Value = value;
    }

    [ServerRpc]
    private void SetFacingServerRpc(int value)
    {
        facingSignNetwork.Value = value >= 0 ? 1 : -1;
    }

    private void OnRunningChanged(bool oldValue, bool newValue)
    {
        ApplyRunning(newValue);
    }

    private void OnFacingChanged(int oldValue, int newValue)
    {
        ApplyFacing(newValue);
    }

    private void ApplyRunning(bool value)
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", value);
        }
    }

    private void ApplyFacing(int value)
    {
        Vector3 scale = transform.localScale;
        scale.x = (value >= 0 ? 1 : -1) * Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public override void OnNetworkDespawn()
    {
        isRunningNetwork.OnValueChanged -= OnRunningChanged;
        facingSignNetwork.OnValueChanged -= OnFacingChanged;
        base.OnNetworkDespawn();
    }
}
