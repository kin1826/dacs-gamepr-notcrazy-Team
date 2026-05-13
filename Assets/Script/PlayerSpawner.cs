using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");

        int index = OwnerClientId == 0 ? 0 : 1;

        transform.position = spawns[index].transform.position;
    }
}