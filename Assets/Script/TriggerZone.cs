using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public PlatformSequence sequence;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sequence.TriggerEvent();
        }
    }
}