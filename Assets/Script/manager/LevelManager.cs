using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple level manager for scene loading.
/// Copy logic from MainManager.StartGame - no fade, no animation.
/// Host use Netcode, single player use local load.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Load a scene using the same logic as MainManager.StartGame.
    /// </summary>
    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LevelManager: sceneName is null or empty");
            return;
        }

        Debug.Log($"LevelManager: Loading scene '{sceneName}'");

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("Only the host can load the next level.");
                return;
            }

            Debug.Log("LevelManager: Loading via Netcode (multiplayer)");
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log("LevelManager: Loading locally (single player)");
            SceneManager.LoadScene(sceneName);
        }
    }

}
