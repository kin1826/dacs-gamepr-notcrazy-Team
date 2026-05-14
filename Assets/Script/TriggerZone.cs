using UnityEngine;

/// <summary>
/// Trigger zone that activates networked platform sequences when player enters.
/// Works with PlatformSequence to trigger platform movements on all clients.
/// </summary>
public class TriggerZone : MonoBehaviour
{
    public PlatformSequence sequence;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (sequence == null)
            {
                Debug.LogError("TriggerZone: PlatformSequence reference not assigned!");
                return;
            }
            
            sequence.TriggerEvent();
        }
    }
}