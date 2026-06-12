using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
        SetPhysics(false);
        SetVisible(false);
        StartCoroutine(SpawnSequence());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsOwner) return;
        if (scene.name == "Menu") return;

        SetPhysics(false);
        SetVisible(false);
        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        // Frame 1: scene is loaded but NGO hasn't finished syncing in-scene NetworkObjects.
        yield return null;

        // Wait until spawn points exist — guarantees scene objects are initialized.
        yield return new WaitUntil(() =>
            GameObject.FindGameObjectsWithTag("Spawn").Length > 0);

        // Frame 2: let physics engine process the scene's colliders for 1 extra tick.
        yield return new WaitForFixedUpdate();

        // Set position while physics is still disabled (avoids teleport-under-load jitter).
        TryMoveToSpawnPoint();

        // Re-enable physics.
        SetPhysics(true);

        // Wait one FixedUpdate so the physics engine registers the new position
        // before the player becomes visible. Prevents 1-frame pop at wrong position.
        yield return new WaitForFixedUpdate();

        SetVisible(true);
    }

    private void TryMoveToSpawnPoint()
    {
        if (SceneManager.GetActiveScene().name == "Menu") return;

        try
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawn");

            if (spawns == null || spawns.Length == 0)
            {
                Debug.LogError("PlayerSpawner: No spawn points found.");
                return;
            }

            Array.Sort(spawns, (a, b) => string.CompareOrdinal(a.name, b.name));

            int index = (int)OwnerClientId;
            if (index >= spawns.Length)
                index = spawns.Length - 1;

            Vector3 spawnPos = spawns[index].transform.position;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = spawnPos;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            transform.position = spawnPos;
        }
        catch (Exception ex)
        {
            Debug.LogError($"PlayerSpawner: Error spawning player {OwnerClientId}: {ex.Message}");
        }
    }

    private void SetPhysics(bool enabled)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = enabled;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
            SceneManager.sceneLoaded -= OnSceneLoaded;

        base.OnNetworkDespawn();
    }
}
