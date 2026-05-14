using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Kill zone that reloads current scene when player touches it.
/// Uses network-synchronized scene loading for multiplayer.
/// </summary>
public class KillZoneReload : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // For multiplayer, use NetworkManager scene loading
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("Requesting scene reload via network...");
                NetworkManager.Singleton.SceneManager.LoadScene(
                    SceneManager.GetActiveScene().name,
                    LoadSceneMode.Single
                );
            }
            else
            {
                // Single player - load directly
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}