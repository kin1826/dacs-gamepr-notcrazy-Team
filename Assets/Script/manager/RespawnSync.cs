using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Multiplayer respawn gate: both players must tap "Continue" before the scene reloads.
/// Attach this script + NetworkObject to a scene GameObject in every game level.
/// </summary>
public class RespawnSync : NetworkBehaviour
{
    public static RespawnSync Instance { get; private set; }

    private readonly HashSet<ulong> readyPlayers = new();
    private bool panelTriggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── SERVER ENTRY POINT ────────────────────────────────────────────────────

    public void TriggerShowPanel(ulong dyingClientId)
    {
        if (!IsServer) return;
        if (panelTriggered) { Debug.Log("[RespawnSync] TriggerShowPanel blocked (already triggered)"); return; }
        panelTriggered = true;
        readyPlayers.Clear();
        Debug.Log($"[RespawnSync] TriggerShowPanel for client {dyingClientId}");
        ShowPanelClientRpc(dyingClientId);
    }

    // ── CLIENT RPC ────────────────────────────────────────────────────────────

    [ClientRpc]
    private void ShowPanelClientRpc(ulong dyingClientId)
    {
        bool iAmDead = NetworkManager.Singleton.LocalClientId == dyingClientId;
        LevelManager.Instance?.ShowRespawnPanel(iAmDead);
    }

    [ClientRpc]
    private void ShowWaitingOnClientRpc(ClientRpcParams rpcParams = default)
    {
        LevelManager.Instance?.ShowWaiting();
    }

    [ClientRpc]
    private void DoReloadClientRpc()
    {
        LevelManager.Instance?.PlayCloseTransitionOnly();
    }

    // ── SERVER RPC ────────────────────────────────────────────────────────────

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (readyPlayers.Contains(clientId)) return;
        readyPlayers.Add(clientId);

        Debug.Log($"[RespawnSync] Player {clientId} ready. readyPlayers={readyPlayers.Count}, connectedIds={string.Join(",", NetworkManager.Singleton.ConnectedClientsIds)}");

        ShowWaitingOnClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        });

        // Require every connected client ID to have tapped.
        // ConnectedClients.Count is unreliable in NGO 2.x (may or may not include host).
        bool allReady = true;
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!readyPlayers.Contains(id)) { allReady = false; break; }
        }

        if (allReady)
            StartCoroutine(ReloadAfterTransition());
    }

    private IEnumerator ReloadAfterTransition()
    {
        DoReloadClientRpc();
        // WaitForSecondsRealtime because Time.timeScale is 0 while panel is shown.
        yield return new WaitForSecondsRealtime(0.35f);
        string sceneName = SceneManager.GetActiveScene().name;
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
