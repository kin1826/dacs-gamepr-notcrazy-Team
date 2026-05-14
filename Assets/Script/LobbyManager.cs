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
}