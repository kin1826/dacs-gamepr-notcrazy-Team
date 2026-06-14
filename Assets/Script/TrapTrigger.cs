using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrapTrigger : NetworkBehaviour
{
    public enum TriggerMode
    {
        Once,
        AfterEnterCount,
        UniquePlayers,
        AllPlayersInside
    }

    [Header("Trigger")]
    public TriggerMode triggerMode = TriggerMode.Once;
    public int requiredCount = 1;
    public bool triggerOnlyOnce = true;

    [Header("Actions")]
    public TrapAction[] actions;

    private readonly HashSet<ulong> playersInside = new HashSet<ulong>();
    private readonly HashSet<ulong> playersSeen   = new HashSet<ulong>();
    private int  enterCount;
    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsPlayer(collision, out ulong clientId)) return;

        if (IsMultiplayer())
        {
            if (!IsServer)
            {
                if (IsSpawned)
                    PlayerEnteredServerRpc(clientId);
                else
                    // TrapTrigger chưa có NetworkObject → route qua TrapAction
                    foreach (var a in actions) a?.RequestActivate();
                return;
            }
            if (!IsHostPlayer(clientId)) return;
        }

        HandlePlayerEnter(clientId);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsPlayer(collision, out ulong clientId)) return;

        if (IsMultiplayer())
        {
            if (!IsServer)
            {
                if (IsSpawned)
                    PlayerExitedServerRpc(clientId);
                return;
            }
            if (!IsHostPlayer(clientId)) return;
        }

        HandlePlayerExit(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerEnteredServerRpc(ulong clientId) => HandlePlayerEnter(clientId);

    [ServerRpc(RequireOwnership = false)]
    private void PlayerExitedServerRpc(ulong clientId) => HandlePlayerExit(clientId);

    private void HandlePlayerEnter(ulong clientId)
    {
        enterCount++;
        playersInside.Add(clientId);
        playersSeen.Add(clientId);
        if (ShouldActivate()) ActivateActions();
    }

    private void HandlePlayerExit(ulong clientId)
    {
        playersInside.Remove(clientId);
    }

    private bool ShouldActivate()
    {
        if (triggerOnlyOnce && hasTriggered) return false;

        int countNeeded = Mathf.Max(1, requiredCount);

        return triggerMode switch
        {
            TriggerMode.Once             => true,
            TriggerMode.AfterEnterCount  => enterCount >= countNeeded,
            TriggerMode.UniquePlayers    => playersSeen.Count >= countNeeded,
            TriggerMode.AllPlayersInside => playersInside.Count >= GetRequiredPlayerCount(),
            _                            => false
        };
    }

    private void ActivateActions()
    {
        hasTriggered = true;
        foreach (TrapAction action in actions)
            action?.RequestActivate();
    }

    private int GetRequiredPlayerCount()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            return Mathf.Max(1, NetworkManager.Singleton.ConnectedClientsIds.Count);
        return Mathf.Max(1, requiredCount);
    }

    private bool IsHostPlayer(ulong clientId)
        => clientId == NetworkManager.Singleton.LocalClientId;

    private bool IsMultiplayer()
        => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;

    private bool IsPlayer(Collider2D collision, out ulong clientId)
    {
        clientId = 0;

        if (!collision.CompareTag("Player") && collision.GetComponentInParent<PlayerMovement>() == null)
            return false;

        NetworkObject networkObject = collision.GetComponentInParent<NetworkObject>();
        if (networkObject == null) return false;

        clientId = networkObject.OwnerClientId;
        return true;
    }
}
