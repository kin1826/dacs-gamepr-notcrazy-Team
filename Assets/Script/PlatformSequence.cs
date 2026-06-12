using Unity.Netcode;
using UnityEngine;

public class PlatformSequence : NetworkBehaviour
{
    public Transform platform;
    public Transform targetPos;
    public float speed = 5f;

    private NetworkVariable<bool> isTriggered = new NetworkVariable<bool>(false);

    private NetworkVariable<Vector3> syncedPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Vector3 startPosition;

    // Client-side smoothing target — updated when NetworkVariable changes.
    private Vector3 clientTargetPosition;

    private void Awake()
    {
        if (platform != null)
            startPosition = platform.position;
    }

    public override void OnNetworkSpawn()
    {
        syncedPosition.OnValueChanged += OnSyncedPositionChanged;

        if (IsServer)
        {
            syncedPosition.Value = startPosition;
        }
        else
        {
            // Use scene-baked startPosition — syncedPosition.Value may still be
            // Vector3.zero if the server's initial tick hasn't arrived yet.
            clientTargetPosition = startPosition;
            if (platform != null)
                platform.position = startPosition;
        }
    }

    public override void OnNetworkDespawn()
    {
        syncedPosition.OnValueChanged -= OnSyncedPositionChanged;
    }

    private void OnSyncedPositionChanged(Vector3 _, Vector3 newPos)
    {
        if (IsServer) return;
        // Update target only — do NOT set position directly.
        clientTargetPosition = newPos;
    }

    void Update()
    {
        if (IsSpawned && !IsServer)
        {
            // Client: smooth movement toward server's position.
            if (platform != null)
            {
                platform.position = Vector3.MoveTowards(
                    platform.position,
                    clientTargetPosition,
                    speed * Time.deltaTime
                );
            }
            return;
        }

        // Server: authoritative movement.
        if (!isTriggered.Value) return;

        platform.position = Vector2.MoveTowards(
            platform.position,
            targetPos.position,
            speed * Time.deltaTime
        );

        if (IsSpawned)
            syncedPosition.Value = platform.position;
    }

    public void TriggerEvent()
    {
        if (!IsSpawned) return;
        TriggerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerServerRpc()
    {
        if (!isTriggered.Value)
            isTriggered.Value = true;
    }
}
