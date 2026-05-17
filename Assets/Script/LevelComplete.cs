using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : NetworkBehaviour
{
    [Header("Exit")]
    public string groupId = "default";
    public float slideSpeed = 3f;
    public float delayBeforeLoad = 1.5f;
    public string nextSceneName = "";

    private static readonly Dictionary<string, Dictionary<ulong, LevelComplete>> ReadyByGroup = new Dictionary<string, Dictionary<ulong, LevelComplete>>();
    private static readonly HashSet<string> CompletingGroups = new HashSet<string>();

    private bool activated;
    private Transform player;
    private Vector3 playerOffset;

    private void OnDestroy()
    {
        if (!ReadyByGroup.TryGetValue(groupId, out Dictionary<ulong, LevelComplete> readyPlayers))
        {
            return;
        }

        List<ulong> clientsToRemove = new List<ulong>();
        foreach (KeyValuePair<ulong, LevelComplete> pair in readyPlayers)
        {
            if (pair.Value == this)
            {
                clientsToRemove.Add(pair.Key);
            }
        }

        foreach (ulong clientId in clientsToRemove)
        {
            readyPlayers.Remove(clientId);
        }

        if (readyPlayers.Count == 0)
        {
            ReadyByGroup.Remove(groupId);
            CompletingGroups.Remove(groupId);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!TryGetPlayer(collision, out NetworkObject playerNetworkObject, out Transform playerTransform))
        {
            return;
        }

        if (!IsMultiplayer())
        {
            BeginCompletionVisual(playerTransform);
            StartCoroutine(LoadNextSceneAfterDelay());
            return;
        }

        if (IsSpawned)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                MarkPlayerReady(playerNetworkObject.OwnerClientId, this);
            }
            else if (playerNetworkObject.IsOwner)
            {
                EnterExitServerRpc();
            }

            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            MarkPlayerReady(playerNetworkObject.OwnerClientId, this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!TryGetPlayer(collision, out NetworkObject playerNetworkObject, out _))
        {
            return;
        }

        if (!IsMultiplayer() || CompletingGroups.Contains(groupId))
        {
            return;
        }

        if (IsSpawned)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                RemovePlayerReady(playerNetworkObject.OwnerClientId, this);
            }
            else if (playerNetworkObject.IsOwner)
            {
                ExitExitServerRpc();
            }

            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            RemovePlayerReady(playerNetworkObject.OwnerClientId, this);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void EnterExitServerRpc(ServerRpcParams rpcParams = default)
    {
        MarkPlayerReady(rpcParams.Receive.SenderClientId, this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExitExitServerRpc(ServerRpcParams rpcParams = default)
    {
        RemovePlayerReady(rpcParams.Receive.SenderClientId, this);
    }

    private static void MarkPlayerReady(ulong clientId, LevelComplete exit)
    {
        if (!ReadyByGroup.TryGetValue(exit.groupId, out Dictionary<ulong, LevelComplete> readyPlayers))
        {
            readyPlayers = new Dictionary<ulong, LevelComplete>();
            ReadyByGroup.Add(exit.groupId, readyPlayers);
        }

        if (readyPlayers.ContainsValue(exit) && !readyPlayers.ContainsKey(clientId))
        {
            Debug.Log("This exit already has another player waiting.");
            return;
        }

        readyPlayers[clientId] = exit;
        Debug.Log($"Player {clientId} ready at level exit. Ready: {readyPlayers.Count}/{GetRequiredPlayers()}");

        if (readyPlayers.Count >= GetRequiredPlayers())
        {
            StartGroupCompletion(exit.groupId, readyPlayers);
        }
    }

    private static void RemovePlayerReady(ulong clientId, LevelComplete exit)
    {
        if (!ReadyByGroup.TryGetValue(exit.groupId, out Dictionary<ulong, LevelComplete> readyPlayers))
        {
            return;
        }

        if (readyPlayers.TryGetValue(clientId, out LevelComplete currentExit) && currentExit == exit)
        {
            readyPlayers.Remove(clientId);
            Debug.Log($"Player {clientId} left level exit. Ready: {readyPlayers.Count}/{GetRequiredPlayers()}");
        }
    }

    private static void StartGroupCompletion(string groupId, Dictionary<ulong, LevelComplete> readyPlayers)
    {
        if (CompletingGroups.Contains(groupId))
        {
            return;
        }

        CompletingGroups.Add(groupId);

        LevelComplete loadOwner = null;
        foreach (KeyValuePair<ulong, LevelComplete> pair in readyPlayers)
        {
            if (loadOwner == null)
            {
                loadOwner = pair.Value;
            }

            pair.Value.StartCompletionForPlayer(pair.Key);
        }

        if (loadOwner != null && loadOwner.NetworkManager != null && loadOwner.NetworkManager.IsServer)
        {
            loadOwner.StartCoroutine(loadOwner.LoadNextSceneAfterDelay());
        }
    }

    private void StartCompletionForPlayer(ulong clientId)
    {
        if (IsSpawned)
        {
            StartCompletionClientRpc(clientId);
            return;
        }

        Transform targetPlayer = FindPlayerByClientId(clientId);
        if (targetPlayer != null)
        {
            BeginCompletionVisual(targetPlayer);
        }
    }

    [ClientRpc]
    private void StartCompletionClientRpc(ulong clientId)
    {
        Transform targetPlayer = FindPlayerByClientId(clientId);
        if (targetPlayer != null)
        {
            BeginCompletionVisual(targetPlayer);
        }
    }

    private void BeginCompletionVisual(Transform targetPlayer)
    {
        if (activated)
        {
            return;
        }

        activated = true;
        player = targetPlayer;

        player.position = new Vector3(transform.position.x, player.position.y, player.position.z);
        playerOffset = player.position - transform.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        StartCoroutine(SlideDown());
    }

    private IEnumerator SlideDown()
    {
        float timer = 0f;

        while (timer < delayBeforeLoad)
        {
            transform.position += Vector3.down * slideSpeed * Time.deltaTime;

            if (player != null)
            {
                player.position = transform.position + playerOffset;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (IsMultiplayer())
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            string targetScene = GetNextSceneName();
            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogWarning("No next scene available.");
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
            return;
        }

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        int nextBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextBuildIndex);
        }
    }

    private string GetNextSceneName()
    {
        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            return nextSceneName;
        }

        int nextBuildIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextBuildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            return "";
        }

        string scenePath = SceneUtility.GetScenePathByBuildIndex(nextBuildIndex);
        return System.IO.Path.GetFileNameWithoutExtension(scenePath);
    }

    private static int GetRequiredPlayers()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            return Mathf.Max(1, NetworkManager.Singleton.ConnectedClientsIds.Count);
        }

        return 1;
    }

    private bool TryGetPlayer(Collider2D collision, out NetworkObject playerNetworkObject, out Transform playerTransform)
    {
        playerNetworkObject = collision.GetComponentInParent<NetworkObject>();
        PlayerMovement movement = collision.GetComponentInParent<PlayerMovement>();
        playerTransform = movement != null ? movement.transform : collision.transform;

        if (!collision.CompareTag("Player") && movement == null)
        {
            return false;
        }

        return !IsMultiplayer() || playerNetworkObject != null;
    }

    private static Transform FindPlayerByClientId(ulong clientId)
    {
        NetworkObject[] networkObjects = FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);
        foreach (NetworkObject networkObject in networkObjects)
        {
            if (networkObject.OwnerClientId == clientId && networkObject.GetComponent<PlayerMovement>() != null)
            {
                return networkObject.transform;
            }
        }

        return null;
    }

    private bool IsMultiplayer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;
    }
}
