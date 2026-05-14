using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Spawns player at correct position based on ownership (Host=0, Client=1, etc).
/// Validates spawn points exist before positioning.
/// </summary>
public class PlayerSpawner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        TryMoveToSpawnPoint();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsOwner) return;

        TryMoveToSpawnPoint();
    }

    private void TryMoveToSpawnPoint()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            return;
        }

        try
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");

            // Validate spawn points exist
            if (spawns == null || spawns.Length == 0)
            {
                Debug.LogError("No spawn points found! Tag your spawn positions with 'Spawn' tag.");
                return;
            }

            Array.Sort(spawns, (a, b) => string.CompareOrdinal(a.name, b.name));

            int index = (int)OwnerClientId;

            // Validate spawn point exists for this client
            if (index >= spawns.Length)
            {
                Debug.LogWarning($"Not enough spawn points! OwnerClientId={OwnerClientId} but only {spawns.Length} spawn points exist. " +
                    $"Add {index + 1} spawn points to level or limit player count.");
                // Fallback to last spawn point
                index = spawns.Length - 1;
            }

            transform.position = spawns[index].transform.position;
            Debug.Log($"Player {OwnerClientId} spawned at spawn point {index}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error spawning player {OwnerClientId}: {ex.Message}");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        base.OnNetworkDespawn();
    }
}
