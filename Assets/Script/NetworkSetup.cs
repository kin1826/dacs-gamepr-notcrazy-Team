using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Ensures NetworkManager has UnityTransport component for relay support.
/// Attach this to the same GameObject as NetworkManager.
/// </summary>
[RequireComponent(typeof(NetworkManager))]
public class NetworkSetup : MonoBehaviour
{
    private void Awake()
    {
        NetworkManager networkManager = GetComponent<NetworkManager>();

        // Ensure UnityTransport is attached
        UnityTransport transport = GetComponent<UnityTransport>();
        if (transport == null)
        {
            transport = gameObject.AddComponent<UnityTransport>();
            Debug.Log("Added UnityTransport component to NetworkManager");
        }

        // Configure transport for relay
        transport.ConnectionData.Address = "127.0.0.1"; // Default, will be overridden by relay
        transport.ConnectionData.Port = 7777; // Default, will be overridden by relay

        Debug.Log("Network setup complete - ready for relay connections");
    }
}