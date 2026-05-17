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
    private NetworkVariable<bool> isMovingToStart = new NetworkVariable<bool>(false);
    private bool hasTriggered;
    private bool localMovingToTarget;
    private bool localMovingToStart;
    private Vector3 startPosition;

    private void Awake()
    {
        if (objectToMove == null)
        {
            objectToMove = transform;
        }

        startPosition = objectToMove.position;
    }

    private void Update()
    {
        bool movingToTarget = IsSpawned ? isMovingToTarget.Value : localMovingToTarget;
        bool movingToStart = IsSpawned ? isMovingToStart.Value : localMovingToStart;

        if (movingToTarget)
        {
            MoveTowards(target != null ? target.position : startPosition);
        }
        else if (movingToStart)
        {
            MoveTowards(startPosition);
        }
    }

    public void RequestActivate()
    {
        if (IsMultiplayer())
        {
            if (IsSpawned)
            {
                ActivateServerRpc();
                return;
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }
        }

        Activate();
    }

    public void RequestDeactivate()
    {
        if (IsMultiplayer())
        {
            if (IsSpawned)
            {
                DeactivateServerRpc();
                return;
            }

            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }
        }

        Deactivate();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateServerRpc()
    {
        Activate();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeactivateServerRpc()
    {
        Deactivate();
    }

    private void Activate()
    {
        if (triggerOnlyOnce && hasTriggered)
        {
            return;
        }

        hasTriggered = true;

        switch (actionType)
        {
            case ActionType.MoveToTarget:
                SetMoving(false, true);
                break;
            case ActionType.MoveBackToStart:
                SetMoving(true, false);
                break;
            case ActionType.SetObjectActive:
                SetObjectActiveNetwork(activeState);
                break;
            case ActionType.SetColliderEnabled:
                SetColliderEnabledNetwork(colliderState);
                break;
            case ActionType.ReloadScene:
                ReloadScene();
                break;
        }
    }

    private void Deactivate()
    {
        switch (actionType)
        {
            case ActionType.MoveToTarget:
                SetMoving(true, false);
                break;
            case ActionType.MoveBackToStart:
                SetMoving(false, true);
                break;
            case ActionType.SetObjectActive:
                SetObjectActiveNetwork(!activeState);
                break;
            case ActionType.SetColliderEnabled:
                SetColliderEnabledNetwork(!colliderState);
                break;
        }
    }

    private void MoveTowards(Vector3 destination)
    {
        if (objectToMove == null)
        {
            return;
        }

        objectToMove.position = Vector3.MoveTowards(
            objectToMove.position,
            destination,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(objectToMove.position, destination) <= 0.01f)
        {
            if (destination == startPosition)
            {
                SetMoving(false, IsSpawned ? isMovingToTarget.Value : localMovingToTarget);
            }
            else
            {
                SetMoving(IsSpawned ? isMovingToStart.Value : localMovingToStart, false);
            }
        }
    }

    private void SetMoving(bool moveToStart, bool moveToTarget)
    {
        if (IsSpawned)
        {
            if (!IsServer)
            {
                return;
            }

            isMovingToStart.Value = moveToStart;
            isMovingToTarget.Value = moveToTarget;
            return;
        }

        localMovingToStart = moveToStart;
        localMovingToTarget = moveToTarget;
    }

    private void SetObjectActive(bool value)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(value);
        }
    }

    private void SetColliderEnabled(bool value)
    {
        if (targetCollider != null)
        {
            targetCollider.enabled = value;
        }
    }

    private void SetObjectActiveNetwork(bool value)
    {
        if (IsSpawned && IsServer)
        {
            SetObjectActiveClientRpc(value);
            return;
        }

        SetObjectActive(value);
    }

    private void SetColliderEnabledNetwork(bool value)
    {
        if (IsSpawned && IsServer)
        {
            SetColliderEnabledClientRpc(value);
            return;
        }

        SetColliderEnabled(value);
    }

    [ClientRpc]
    private void SetObjectActiveClientRpc(bool value)
    {
        SetObjectActive(value);
    }

    [ClientRpc]
    private void SetColliderEnabledClientRpc(bool value)
    {
        SetColliderEnabled(value);
    }

    private void ReloadScene()
    {
        if (IsMultiplayer())
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene(
                SceneManager.GetActiveScene().name,
                LoadSceneMode.Single
            );
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private bool IsMultiplayer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
    }
}
