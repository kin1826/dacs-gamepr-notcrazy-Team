using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PushBlock : MonoBehaviour
{
    [Header("Movement")]
    public bool freezeRotation = true;
    public bool freezeVerticalPosition = false;
    public float maxHorizontalSpeed = 4f;
    public bool stopWhenNotPushed = true;

    private Rigidbody2D rb;
    private float startY;
    private int playerContacts;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startY = transform.position.y;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        if (freezeRotation)
        {
            rb.freezeRotation = true;
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsConnectedClient &&
            !NetworkManager.Singleton.IsServer)
        {
            return;
        }

        Vector2 velocity = rb.linearVelocity;
        velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);

        if (stopWhenNotPushed && playerContacts == 0)
        {
            velocity.x = 0f;
        }

        rb.linearVelocity = velocity;

        if (freezeVerticalPosition)
        {
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsPlayer(collision.collider))
        {
            playerContacts++;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsPlayer(collision.collider))
        {
            playerContacts = Mathf.Max(0, playerContacts - 1);
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null;
    }
}
