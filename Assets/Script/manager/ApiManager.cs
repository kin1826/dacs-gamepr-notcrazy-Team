using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    public static ApiManager Instance { get; private set; }

    private const string BaseUrl = "https://dacs-gamepr-notcrazy-team-production.up.railway.app";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── PUBLIC API ────────────────────────────────────────────────────────────

    public void Register(RegisterRequest req,
        Action<AuthResponse> onSuccess, Action<string> onError)
        => StartCoroutine(Post<RegisterRequest, AuthResponse>(
            "/api/auth/register", req, onSuccess, onError));

    public void Login(LoginRequest req,
        Action<AuthResponse> onSuccess, Action<string> onError)
        => StartCoroutine(Post<LoginRequest, AuthResponse>(
            "/api/auth/login", req, onSuccess, onError));

    public void GoogleLogin(GoogleLoginRequest req,
        Action<AuthResponse> onSuccess, Action<string> onError)
        => StartCoroutine(Post<GoogleLoginRequest, AuthResponse>(
            "/api/auth/google", req, onSuccess, onError));

    public void UpdateProgress(UpdateProgressRequest req,
        Action<AuthResponse> onSuccess, Action<string> onError)
        => StartCoroutine(Post<UpdateProgressRequest, AuthResponse>(
            "/api/user/update-progress", req, onSuccess, onError));

    public void CreateTopUp(CreateTopUpRequest req,
        Action<TopUpResponse> onSuccess, Action<string> onError)
        => StartCoroutine(Post<CreateTopUpRequest, TopUpResponse>(
            "/api/topup/create", req, onSuccess, onError));

    public void GetGold(Action<int> onSuccess, Action<string> onError = null)
        => StartCoroutine(Get<GoldResponse>(
            "/api/user/gold",
            res => onSuccess?.Invoke(res.gold),
            onError));

    private Coroutine _goldPolling;
    private const float GoldPollInterval = 30f;

    public void StartGoldPolling()
    {
        StopGoldPolling();
        _goldPolling = StartCoroutine(GoldPollingLoop());
    }

    public void StopGoldPolling()
    {
        if (_goldPolling != null) { StopCoroutine(_goldPolling); _goldPolling = null; }
    }

    private IEnumerator GoldPollingLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(GoldPollInterval);
            if (!UserSession.IsLoggedIn) yield break;

            GetGold(serverGold =>
            {
                if (serverGold == UserSession.Current.gold) return;
                UserSession.Current.gold   = serverGold;
                SaveManager.Data.savedGold = serverGold;
                SaveManager.Save();
                MainManager.Instance?.RefreshGold();
            });
        }
    }

    public void UpdateGold(int delta, Action<int> onSuccess = null, Action<string> onError = null)
        => StartCoroutine(Post<UpdateGoldApiRequest, GoldResponse>(
            "/api/user/update-gold",
            new UpdateGoldApiRequest { delta = delta },
            res => onSuccess?.Invoke(res.gold),
            err => { Debug.LogWarning($"[ApiManager] UpdateGold failed: {err}"); onError?.Invoke(err); }));

    // ── PRIVATE HELPER ────────────────────────────────────────────────────────

    private IEnumerator Get<TRes>(
        string endpoint,
        Action<TRes> onSuccess, Action<string> onError)
        where TRes : class
    {
        using var req = new UnityWebRequest(BaseUrl + endpoint, "GET");
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (UserSession.IsLoggedIn)
            req.SetRequestHeader("Authorization", $"Bearer {UserSession.Current.token}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(JsonUtility.FromJson<TRes>(req.downloadHandler.text));
        else
        {
            string err = req.downloadHandler.text;
            if (string.IsNullOrEmpty(err)) err = req.error;
            onError?.Invoke(err);
        }
    }

    private IEnumerator Post<TReq, TRes>(
        string endpoint, TReq data,
        Action<TRes> onSuccess, Action<string> onError)
        where TReq : class where TRes : class
    {
        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));

        using var req = new UnityWebRequest(BaseUrl + endpoint, "POST");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (UserSession.IsLoggedIn)
            req.SetRequestHeader("Authorization", $"Bearer {UserSession.Current.token}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(JsonUtility.FromJson<TRes>(req.downloadHandler.text));
        }
        else
        {
            string err = req.downloadHandler.text;
            if (string.IsNullOrEmpty(err)) err = req.error;
            Debug.LogWarning($"[ApiManager] {endpoint}: {err}");
            onError?.Invoke(err);
        }
    }
}
