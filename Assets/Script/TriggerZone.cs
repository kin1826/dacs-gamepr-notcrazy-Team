using Unity.Netcode;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public PlatformSequence sequence;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Only server evaluates trigger logic (same pattern as TrapTrigger/TrapButton).
        if (NetworkManager.Singleton != null
            && NetworkManager.Singleton.IsConnectedClient
            && !NetworkManager.Singleton.IsServer) return;

        if (sequence == null) return;
        sequence.TriggerEvent();
    }
}