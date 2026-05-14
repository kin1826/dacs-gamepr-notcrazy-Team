using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Centralized network error handling for better debugging.
/// Catches and logs all network-related errors with context.
/// </summary>
public class NetworkErrorHandler : MonoBehaviour
{
    public static NetworkErrorHandler Instance { get; private set; }

    [SerializeField] private bool logErrors = true;
    [SerializeField] private bool logWarnings = true;
    [SerializeField] private bool pauseOnError = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate NetworkErrorHandler found! Destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            RegisterNetworkCallbacks();
        }
    }

    private void OnDisable()
    {
        UnregisterNetworkCallbacks();
    }

    /// <summary>
    /// Register all network event callbacks for error handling.
    /// </summary>
    private void RegisterNetworkCallbacks()
    {
        var nm = NetworkManager.Singleton;

        // Client disconnect handling
        nm.OnClientDisconnectCallback += OnClientDisconnect;

        // Connection approval
        nm.ConnectionApprovalCallback += OnConnectionApproval;

        // Transport errors
        if (nm.NetworkConfig.NetworkTransport != null)
        {
            // Could add transport-specific error handling here
        }
    }

    /// <summary>
    /// Unregister network event callbacks.
    /// </summary>
    private void UnregisterNetworkCallbacks()
    {
        if (NetworkManager.Singleton == null) return;

        var nm = NetworkManager.Singleton;
        nm.OnClientDisconnectCallback -= OnClientDisconnect;
        nm.ConnectionApprovalCallback -= OnConnectionApproval;
    }

    /// <summary>
    /// Handle client disconnection.
    /// </summary>
    private void OnClientDisconnect(ulong clientId)
    {
        string msg = $"Client {clientId} disconnected";
        if (logWarnings)
        {
            Debug.LogWarning(msg);
        }

        // Cleanup for this client
        HandleClientCleanup(clientId);
    }

    /// <summary>
    /// Handle connection approval/rejection.
    /// </summary>
    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Accept all connections (can add validation here for relay)
        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    /// <summary>
    /// Cleanup when client disconnects.
    /// </summary>
    private void HandleClientCleanup(ulong clientId)
    {
        // Destroy player objects for disconnected client
        var spawnedObjects = Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        foreach (var obj in spawnedObjects)
        {
            if (obj.OwnerClientId == clientId && obj.IsSpawned)
            {
                Debug.Log($"Despawning object {obj.gameObject.name} owned by client {clientId}");
                obj.Despawn();
            }
        }
    }

    /// <summary>
    /// Log a network error with context.
    /// </summary>
    public void LogNetworkError(string context, string errorMessage)
    {
        if (logErrors)
        {
            string fullMessage = $"[Network Error - {context}] {errorMessage}";
            Debug.LogError(fullMessage);

            if (pauseOnError)
            {
                Time.timeScale = 0;
                Debug.Log("Game paused due to network error");
            }
        }
    }

    /// <summary>
    /// Log a network warning.
    /// </summary>
    public void LogNetworkWarning(string context, string warningMessage)
    {
        if (logWarnings)
        {
            string fullMessage = $"[Network Warning - {context}] {warningMessage}";
            Debug.LogWarning(fullMessage);
        }
    }

    /// <summary>
    /// Check if NetworkManager is properly initialized.
    /// </summary>
    public static bool IsNetworkManagerReady()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is NULL!");
            return false;
        }

        if (!NetworkManager.Singleton.IsListening)
        {
            Debug.LogError("NetworkManager is not listening!");
            return false;
        }

        return true;
    }
}
