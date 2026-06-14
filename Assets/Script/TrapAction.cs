using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrapAction : NetworkBehaviour
{
    public enum ActionType
    {
        MoveToTarget,
        MoveBackToStart,
        SetObjectActive,
        SetColliderEnabled,
        ReloadScene
    }

    [Header("Action")]
    public ActionType actionType = ActionType.MoveToTarget;
    public bool triggerOnlyOnce = true;

    [Header("Movement")]
    public Transform objectToMove;
    public Transform target;
    public float moveSpeed = 5f;

    [Header("Object State")]
    public GameObject targetObject;
    public bool activeState = true;
    public Collider2D targetCollider;
    public bool colliderState = true;

    private NetworkVariable<bool> isMovingToTarget = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isMovingToStart  = new NetworkVariable<bool>(false);

    private NetworkVariable<Vector3> syncedPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool hasTriggered;
    private bool localMovingToTarget;
    private bool localMovingToStart;
    private Vector3 startPosition;

    private Vector3 clientTargetPosition;

    // Track position ourselves so we don't depend on Transform readback after assignment.
    private Vector3 physicsCurrentPosition;

    // Exposed so PlayerMovement can read how much the platform moved this frame
    // and add that delta to the player's own position (carry-on-platform logic).
    public Vector2 LastFrameDelta { get; private set; }

    private void Awake()
    {
        if (objectToMove == null)
            objectToMove = transform;

        startPosition          = objectToMove.position;
        physicsCurrentPosition = startPosition;
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
            clientTargetPosition  = startPosition;
            objectToMove.position = startPosition;
        }
    }

    public override void OnNetworkDespawn()
    {
        syncedPosition.OnValueChanged -= OnSyncedPositionChanged;
    }

    private void OnSyncedPositionChanged(Vector3 _, Vector3 newPos)
    {
        if (IsServer) return;
        clientTargetPosition = newPos;
    }

    private void Update()
    {
        // Client: visually interpolate toward server position.
        // LastFrameDelta is published so PlayerMovement can carry the local player.
        if (IsSpawned && !IsServer)
        {
            Vector3 prev = objectToMove.position;
            objectToMove.position = Vector3.MoveTowards(
                objectToMove.position,
                clientTargetPosition,
                moveSpeed * Time.deltaTime
            );
            LastFrameDelta = (Vector2)(objectToMove.position - prev);
        }
    }

    private void FixedUpdate()
    {
        // Server / single-player: authoritative movement.
        if (IsSpawned && !IsServer) return;

        bool movingToTarget = IsSpawned ? isMovingToTarget.Value : localMovingToTarget;
        bool movingToStart  = IsSpawned ? isMovingToStart.Value  : localMovingToStart;

        if (movingToTarget)
            MovePlatform(target != null ? target.position : startPosition);
        else if (movingToStart)
            MovePlatform(startPosition);
    }

    private void MovePlatform(Vector3 destination)
    {
        if (objectToMove == null) return;

        Vector3 prev = physicsCurrentPosition;
        physicsCurrentPosition = Vector3.MoveTowards(
            physicsCurrentPosition,
            destination,
            moveSpeed * Time.fixedDeltaTime
        );

        objectToMove.position = physicsCurrentPosition;

        if (IsSpawned)
            syncedPosition.Value = physicsCurrentPosition;

        // Publish delta so PlayerMovement.OnCollisionStay2D can carry the player.
        LastFrameDelta = (Vector2)(physicsCurrentPosition - prev);

        if (Vector3.Distance(physicsCurrentPosition, destination) < 0.01f)
        {
            physicsCurrentPosition = destination;
            bool wasMovingToStart  = IsSpawned ? isMovingToStart.Value  : localMovingToStart;
            bool wasMovingToTarget = IsSpawned ? isMovingToTarget.Value : localMovingToTarget;

            if (destination == startPosition)
                SetMoving(false, wasMovingToTarget);
            else
                SetMoving(wasMovingToStart, false);
        }
    }

    public void RequestActivate()
    {
        if (IsMultiplayer())
        {
            if (IsSpawned && IsServer)  { Activate(); return; }
            if (IsSpawned && !IsServer) { ActivateServerRpc(); return; }
            if (!NetworkManager.Singleton.IsServer) return;
        }
        Activate();
    }

    public void RequestDeactivate()
    {
        if (IsMultiplayer())
        {
            if (IsSpawned && IsServer)  { Deactivate(); return; }
            if (IsSpawned && !IsServer) { DeactivateServerRpc(); return; }
            if (!NetworkManager.Singleton.IsServer) return;
        }
        Deactivate();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateServerRpc() => Activate();

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateServerRpc() => Deactivate();

    private void Activate()
    {
        if (triggerOnlyOnce && hasTriggered) return;
        hasTriggered = true;

        switch (actionType)
        {
            case ActionType.MoveToTarget:       SetMoving(false, true); break;
            case ActionType.MoveBackToStart:    SetMoving(true,  false); break;
            case ActionType.SetObjectActive:    SetObjectActiveNetwork(activeState); break;
            case ActionType.SetColliderEnabled: SetColliderEnabledNetwork(colliderState); break;
            case ActionType.ReloadScene:        ReloadScene(); break;
        }
    }

    private void Deactivate()
    {
        switch (actionType)
        {
            case ActionType.MoveToTarget:       SetMoving(true,  false); break;
            case ActionType.MoveBackToStart:    SetMoving(false, true); break;
            case ActionType.SetObjectActive:    SetObjectActiveNetwork(!activeState); break;
            case ActionType.SetColliderEnabled: SetColliderEnabledNetwork(!colliderState); break;
        }
    }

    private void SetMoving(bool moveToStart, bool moveToTarget)
    {
        if (IsSpawned)
        {
            if (!IsServer) return;
            isMovingToStart.Value  = moveToStart;
            isMovingToTarget.Value = moveToTarget;
            return;
        }
        localMovingToStart  = moveToStart;
        localMovingToTarget = moveToTarget;
    }

    private void SetObjectActiveNetwork(bool value)
    {
        if (IsSpawned && IsServer) { SetObjectActiveClientRpc(value); return; }
        SetObjectActive(value);
    }

    private void SetColliderEnabledNetwork(bool value)
    {
        if (IsSpawned && IsServer) { SetColliderEnabledClientRpc(value); return; }
        SetColliderEnabled(value);
    }

    private void SetObjectActive(bool value)
    {
        if (targetObject != null) targetObject.SetActive(value);
    }

    private void SetColliderEnabled(bool value)
    {
        if (targetCollider != null) targetCollider.enabled = value;
    }

    [ClientRpc] private void SetObjectActiveClientRpc(bool value)    => SetObjectActive(value);
    [ClientRpc] private void SetColliderEnabledClientRpc(bool value) => SetColliderEnabled(value);

    private void ReloadScene()
    {
        if (IsMultiplayer())
        {
            if (!NetworkManager.Singleton.IsServer) return;
            NetworkManager.Singleton.SceneManager.LoadScene(
                SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            return;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private bool IsMultiplayer()
        => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
}
