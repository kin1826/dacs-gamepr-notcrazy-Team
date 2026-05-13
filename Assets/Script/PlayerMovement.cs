using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = 2f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [SerializeField] private Animator animator; // kéo Graphics vào đây

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Input System gọi
    public void OnMove(InputValue value)
    {
        if (!IsOwner) return;


        moveInput = value.Get<Vector2>();
    }
    
    // 🦘 NHẢY
    public void OnJump(InputValue value)
    {
        if (!IsOwner) return;


        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void Update()
    {
        if (!IsOwner) return; // Chỉ xử lý input cho player sở hữu

        // 🧱 CHECK ĐẤT
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );
        Debug.Log(isGrounded);
        
        // 🎭 Animation
        float speed = Mathf.Abs(moveInput.x);

        bool isRunning = speed > 0.1f;

        // chống tụt về 0 đột ngột
        if (speed < 0.3f && Mathf.Abs(rb.linearVelocity.x) > 0.3f)
        {
            isRunning = true;
        }
        
        // bool isRunning = Mathf.Abs(moveInput.x) > 0.1f;
        animator.SetBool("isRunning", isRunning);

        // 🔁 Flip
        if (moveInput.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(moveInput.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);
        }
    }
}