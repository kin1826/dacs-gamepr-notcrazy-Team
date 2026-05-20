using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages multiplayer lobby state (dino selections). Ensures single instance.
/// Syncs dino selections between host and clients using NetworkVariables.
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public NetworkVariable<int> HostDino =
        new NetworkVariable<int>(-1);

    public NetworkVariable<int> ClientDino =
        new NetworkVariable<int>(-1);

    public NetworkVariable<int> MinLevelLobby =
        new NetworkVariable<int>(-1);

    private void Awake()
    {
        // Singleton pattern: prevent multiple instances
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate LobbyManager found! Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Rpc(SendTo.Server)]
    public void SetHostDinoRpc(int index)
    {
        HostDino.Value = index;
    }

    [Rpc(SendTo.Server)]
    public void SetClientDinoRpc(int index)
    {
        ClientDino.Value = index;
    }

    [Rpc(
        SendTo.Server,
        InvokePermission =
        RpcInvokePermission.Everyone
    )]
    public void SetMinLevelRpc(
        int level
    )
    {
        if(
            MinLevelLobby.Value == -1
        )
        {
            MinLevelLobby.Value =
                level;

            return;
        }

        MinLevelLobby.Value =
            Mathf.Min(
                MinLevelLobby.Value,
                level
            );
    }

    public int GetMinLevel()
    {
        return MinLevelLobby.Value;
    }
}