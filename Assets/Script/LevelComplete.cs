using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level completion portal that moves down and loads next scene.
/// Uses network-synchronized scene loading for multiplayer.
/// Host makes the decision to load next scene for all clients.
/// </summary>
public class LevelComplete : MonoBehaviour
{
    public float slideSpeed = 3f;
    public float delayBeforeLoad = 1.5f;

    private bool activated = false;
    private Transform player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated) return;

        if (collision.CompareTag("Player"))
        {
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsConnectedClient &&
                !NetworkManager.Singleton.IsServer)
            {
                return;
            }

            activated = true;
            player = collision.transform;

            // Disable player controls
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
                {
                    rb.simulated = false;
                }
            }
            else
            {
                Debug.LogWarning("Player has no Rigidbody2D component!");
            }

            // Keep player centered on X axis
            player.position = new Vector3(transform.position.x, player.position.y, player.position.z);

            // Start animation
            StartCoroutine(FinishLevel());
        }
    }

    System.Collections.IEnumerator FinishLevel()
    {
        float timer = 0;

        while (timer < delayBeforeLoad)
        {
            // Slide down
            transform.position += Vector3.down * slideSpeed * Time.deltaTime;

            // Keep player centered on X axis without re-parenting NetworkObjects.
            if (player != null)
                player.position = new Vector3(transform.position.x, player.position.y, player.position.z);

            timer += Time.deltaTime;
            yield return null;
        }

        // Load next scene - use network-synchronized loading if multiplayer
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.Log("Loading next scene via network...");
            // Only host initiates scene load to prevent conflicts
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(
                    "Level_" + (int.Parse(SceneManager.GetActiveScene().name.Replace("Level_", "")) + 1).ToString("D2"),
                    LoadSceneMode.Single
                );
            }
        }
        else
        {
            // Single player - load directly
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
