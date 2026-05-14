using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// Network UI controller with relay support.
/// Provides buttons for hosting/joining with relay or direct connection.
/// </summary>
public class NetworkUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button hostButton;
    public Button joinButton;
    public TMP_InputField joinCodeInput;
    public TMP_Text statusText;
    public TMP_Text joinCodeDisplay;
    public Toggle useRelayToggle;

    private async void Start()
    {
        // Wait for NetworkConfig to initialize
        await Task.Delay(1000);
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (NetworkConfig.Instance == null) return;

        bool isConnected = NetworkManager.Singleton != null &&
                          (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient);

        // Update status
        if (statusText != null)
        {
            if (isConnected)
            {
                statusText.text = NetworkConfig.Instance.isHost ? "Hosting..." : "Connected!";
            }
            else
            {
                statusText.text = "Disconnected";
            }
        }

        // Update join code display
        if (joinCodeDisplay != null && NetworkConfig.Instance.isHost)
        {
            joinCodeDisplay.text = $"Join Code: {NetworkConfig.Instance.relayCode}";
        }
    }

    public async void StartHost()
    {
        if (NetworkConfig.Instance == null)
        {
            Debug.LogError("NetworkConfig not found!");
            return;
        }

        try
        {
            if (useRelayToggle != null && useRelayToggle.isOn)
            {
                // Use relay
                string code = await NetworkConfig.Instance.CreateRelay();
                if (code != null)
                {
                    Debug.Log($"Host started with relay! Join code: {code}");
                }
            }
            else
            {
                // Direct connection
                NetworkConfig.Instance.StartHostDirect();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to start host: {ex.Message}");
        }
    }

    public async void StartClient()
    {
        if (NetworkConfig.Instance == null)
        {
            Debug.LogError("NetworkConfig not found!");
            return;
        }

        string code = joinCodeInput != null ? joinCodeInput.text.Trim() : "";

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogError("Please enter a join code!");
            return;
        }

        try
        {
            if (useRelayToggle != null && useRelayToggle.isOn)
            {
                // Use relay
                bool success = await NetworkConfig.Instance.JoinRelay(code);
                if (success)
                {
                    Debug.Log("Client joined relay successfully!");
                }
            }
            else
            {
                // Direct connection (would need IP/port, not implemented)
                Debug.LogError("Direct connection requires IP/Port - use relay instead!");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to start client: {ex.Message}");
        }
    }

    public void Shutdown()
    {
        if (NetworkConfig.Instance != null)
        {
            NetworkConfig.Instance.Shutdown();
        }
    }
}