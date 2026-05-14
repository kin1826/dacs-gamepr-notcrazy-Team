using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network-aware platform that moves to target position when triggered.
/// All clients see the same platform animation synchronized via NetworkVariable.
/// </summary>
public class PlatformSequence : NetworkBehaviour
{
    public Transform platform;
    public Transform targetPos;
    public float speed = 5f;

    private NetworkVariable<bool> isTriggered = new NetworkVariable<bool>(false);

    void Update()
    {
        if (!isTriggered.Value) return;

        platform.position = Vector2.MoveTowards(
            platform.position,
            targetPos.position,
            speed * Time.deltaTime
        );
    }

    /// <summary>
    /// Called when trigger zone detects player. Synchronizes across network.
    /// </summary>
    public void TriggerEvent()
    {
        if (!IsSpawned)
        {
            Debug.LogWarning("PlatformSequence not networked yet!");
            return;
        }
        
        Debug.Log("Platform triggered!");
        TriggerServerRpc();
    }

    /// <summary>
    /// Server RPC to synchronize platform trigger across all 
    /// </summary>
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TriggerServerRpc()
    {
        if (!isTriggered.Value)
        {
            isTriggered.Value = true;
        }
    }
}