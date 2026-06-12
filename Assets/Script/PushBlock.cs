using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PushBlock : NetworkBehaviour
{
    [Header("Movement")]
    public bool freezeRotation = true;
    public bool freezeVerticalPosition = false;
    public float maxHorizontalSpeed = 4f;
    public bool stopWhenNotPushed = true;

    private NetworkVariable<Vector2> syncedPosition = new NetworkVariable<Vector2>(
        Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody2D rb;
    private float startY;
    private int playerContacts;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startY = transform.position.y;

        if (freezeRotation)
            rb.freezeRotation = true;
    }

    public override void OnNetworkSpawn()
    {
        syncedPosition.OnValueChanged += OnSyncedPositionChanged;

        if (IsServer)
        {
            // Server owns physics: Dynamic Rigidbody2D.
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            syncedPosition.Value = rb.position;
        }
        else
        {
            // Clients have no physics simulation. Position comes from server.
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.simulated = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        syncedPosition.OnValueChanged -= OnSyncedPositionChanged;
    }

    // Client-side smoothing target.
    private Vector2 clientTargetPosition;

    private void OnSyncedPositionChanged(Vector2 _, Vector2 newPos)
    {
        if (IsServer) return;
        clientTargetPosition = newPos;
    }

    private void Update()
    {
        // Client: smooth toward server's position (Rigidbody is Kinematic, so set directly).
        if (IsSpawned && !IsServer)
        {
            Vector2 smooth = Vector2.MoveTowards(rb.position, clientTargetPosition, maxHorizontalSpeed * 2f * Time.deltaTime);
            rb.position = smooth;
            transform.position = new Vector3(smooth.x, smooth.y, transform.position.z);
        }
    }

    private void FixedUpdate()
    {
        // Physics runs on server only.
        if (IsSpawned && !IsServer) return;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);

        if (stopWhenNotPushed && playerContacts == 0)
            velocity.x = 0f;

        rb.linearVelocity = velocity;

        if (freezeVerticalPosition)
        {
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        // Broadcast position to clients at NGO tick rate.
        if (IsSpawned)
            syncedPosition.Value = rb.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsSpawned && !IsServer) return;
        if (IsPlayer(collision.collider))
            playerContacts++;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsSpawned && !IsServer) return;
        if (IsPlayer(collision.collider))
            playerContacts = Mathf.Max(0, playerContacts - 1);
    }

    private bool IsPlayer(Collider2D other)
        => other.CompareTag("Player") || other.GetComponentInParent<PlayerMovement>() != null;
}
