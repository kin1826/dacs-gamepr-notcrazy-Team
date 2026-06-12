using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Transition Panel")]
    public GameObject transitionPanel;
    public RectTransform topImage;
    public RectTransform bottomImage;
    private float transitionDuration = 0.3f;
    private float delayBeforeOpen = 0.2f;

    private float halfHeight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        halfHeight = Screen.height / 2f;

        // Đặt 2 ảnh ra ngoài màn hình (trạng thái mở sẵn)
        topImage.anchoredPosition = new Vector2(0, halfHeight);
        bottomImage.anchoredPosition = new Vector2(0, -halfHeight);
        transitionPanel.SetActive(false);

        if (GameData.IsMultiplayer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // Mở màn hình sau khi scene load xong
        StartCoroutine(OpenTransition());
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!GameData.IsMultiplayer) return;
        if (SceneManager.GetActiveScene().name == "Menu") return;

        bool isLocalClient = NetworkManager.Singleton != null
            && clientId == NetworkManager.Singleton.LocalClientId;

        if (isLocalClient)
        {
            SceneManager.LoadScene("Menu");
        }
    }

    // ── LOAD LEVEL ────────────────────────────────────────

    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("LevelManager: sceneName is null or empty");
            return;
        }

        StartCoroutine(TransitionAndLoad(sceneName));
    }

    // ── TRANSITION ────────────────────────────────────────

    private IEnumerator TransitionAndLoad(string sceneName)
    {
        if (transitionPanel != null && topImage != null && bottomImage != null)
            yield return StartCoroutine(CloseTransition());

        Debug.Log($"LevelManager: Loading scene '{sceneName}'");

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("LevelManager: Only the host can load the next level.");
                yield break;
            }
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private IEnumerator CloseTransition()
    {
        transitionPanel.SetActive(true);

        Vector2 topStart = new Vector2(0, halfHeight);
        Vector2 topEnd = new Vector2(0, 0);
        Vector2 bottomStart = new Vector2(0, -halfHeight);
        Vector2 bottomEnd = new Vector2(0, 0);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            topImage.anchoredPosition = Vector2.Lerp(topStart, topEnd, t);
            bottomImage.anchoredPosition = Vector2.Lerp(bottomStart, bottomEnd, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        topImage.anchoredPosition = topEnd;
        bottomImage.anchoredPosition = bottomEnd;
    }

    private IEnumerator OpenTransition()
    {
        transitionPanel.SetActive(true);

        // Đặt ở giữa trước khi mở
        topImage.anchoredPosition = Vector2.zero;
        bottomImage.anchoredPosition = Vector2.zero;

        yield return new WaitForSecondsRealtime(delayBeforeOpen);

        Vector2 topEnd = new Vector2(0, halfHeight);
        Vector2 bottomEnd = new Vector2(0, -halfHeight);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            topImage.anchoredPosition = Vector2.Lerp(Vector2.zero, topEnd, t);
            bottomImage.anchoredPosition = Vector2.Lerp(Vector2.zero, bottomEnd, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        topImage.anchoredPosition = topEnd;
        bottomImage.anchoredPosition = bottomEnd;
        transitionPanel.SetActive(false);
    }
}
