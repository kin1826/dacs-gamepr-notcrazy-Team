using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillZoneReload : MonoBehaviour
{
    // Shared flag across all KillZone instances — prevents double-reload when both
    // players die simultaneously or when server detects the same trigger twice.
    private static bool reloadRequested;

    private void Awake()
    {
        // Reset every time this scene object is created (i.e., every scene load).
        reloadRequested = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (reloadRequested) return;
        if (LevelComplete.IsCompleting) return;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkObject netObj = collision.GetComponentInParent<NetworkObject>();
            if (netObj == null) return;

            if (NetworkManager.Singleton.IsServer)
            {
                // Server detected a player in kill zone → reload immediately.
                reloadRequested = true;
                NetworkManager.Singleton.SceneManager.LoadScene(
                    SceneManager.GetActiveScene().name, LoadSceneMode.Single);
            }
            else if (netObj.IsOwner)
            {
                // Client detected their OWN player entering the kill zone.
                //
                // Problem without this block:
                //   Client physics fires → server check returns early → player keeps flying
                //   for ~50-100ms (NetworkTransform lag) → both players see jitter.
                //
                // Fix:
                //   1. Freeze own player immediately — stops flying, stops NetworkTransform
                //      from broadcasting the jitter position to host.
                //   2. Send ServerRpc directly — don't wait for server to detect via
                //      NetworkTransform copy entering the trigger with latency.
                reloadRequested = true;
                FreezePlayer(netObj);
                netObj.GetComponent<PlayerMovement>()?.RequestKillZoneReloadServerRpc();
            }
        }
        else
        {
            // Single player.
            reloadRequested = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private static void FreezePlayer(NetworkObject netObj)
    {
        Rigidbody2D rb = netObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        foreach (Renderer r in netObj.GetComponentsInChildren<Renderer>())
            r.enabled = false;
    }
}
