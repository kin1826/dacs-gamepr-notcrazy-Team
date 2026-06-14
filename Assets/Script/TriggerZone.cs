using Unity.Netcode;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public PlatformSequence sequence;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (sequence == null) return;

        // PlatformSequence.TriggerEvent() gọi ServerRpc nội bộ — client gọi được
        sequence.TriggerEvent();
    }
}