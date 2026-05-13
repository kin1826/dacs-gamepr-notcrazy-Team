using UnityEngine;

public class PlatformSequence : MonoBehaviour
{
    public Transform platform;
    public Transform targetPos;
    public float speed = 5f;

    private bool triggered = false;

    void Update()
    {
        if (!triggered) return;

        platform.position = Vector2.MoveTowards(
            platform.position,
            targetPos.position,
            speed * Time.deltaTime
        );
    }

    public void TriggerEvent()
    {
        Debug.Log("Triggered!");
        triggered = true;
    }
}