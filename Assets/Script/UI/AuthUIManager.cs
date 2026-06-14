using UnityEngine;
using TMPro;

// Gắn trực tiếp lên GameObject LoginPanel trong Canvas của Menu scene.
// Panel này tự ẩn khi user đã login trước đó.
public class AuthUIManager : MonoBehaviour
{
    [Header("Sub-panels")]
    public GameObject loginSubPanel;
    public GameObject registerSubPanel;
    public GameObject loadingOverlay;

    [Header("Login")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public TMP_Text       loginError;

    [Header("Register")]
    public TMP_InputField registerName;
    public TMP_InputField registerEmail;
    public TMP_InputField registerPassword;
    public TMP_Text       registerError;

    // ── LIFECYCLE ─────────────────────────────────────────────────────────────

    private void Start()
    {
        // SaveManager.Load() đã được gọi trong MainManager.Awake()
        // nên TryRestore() chạy được luôn ở đây
        if (UserSession.TryRestore())
        {
            gameObject.SetActive(false);
            MainManager.Instance?.RefreshPlayerName();
            return;
        }

        ShowLogin();
    }

    // ── PANEL SWITCHING ───────────────────────────────────────────────────────

    public void ShowLogin()
    {
        loginSubPanel.SetActive(true);
        registerSubPanel.SetActive(false);
        loadingOverlay.SetActive(false);
        loginError.text = "";
    }

    public void ShowRegister()
    {
        loginSubPanel.SetActive(false);
        registerSubPanel.SetActive(true);
        loadingOverlay.SetActive(false);
        registerError.text = "";
    }

    // ── LOGIN ─────────────────────────────────────────────────────────────────

    public void OnLoginClick()
    {
        string email = loginEmail.text.Trim();
        string pass  = loginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            loginError.text = "Vui lòng nhập đầy đủ thông tin.";
            return;
        }

        loadingOverlay.SetActive(true);

        ApiManager.Instance.Login(
            new LoginRequest { email = email, password = pass },
            onSuccess: res =>
            {
                UserSession.Set(res);
                MainManager.Instance?.RefreshPlayerName();
                gameObject.SetActive(false);
            },
            onError: err =>
            {
                loadingOverlay.SetActive(false);
                loginError.text = err;
            });
    }

    // ── REGISTER ──────────────────────────────────────────────────────────────

    public void OnRegisterClick()
    {
        string name  = registerName.text.Trim();
        string email = registerEmail.text.Trim();
        string pass  = registerPassword.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            registerError.text = "Vui lòng nhập đầy đủ thông tin.";
            return;
        }

        if (pass.Length < 6)
        {
            registerError.text = "Mật khẩu phải ít nhất 6 ký tự.";
            return;
        }

        loadingOverlay.SetActive(true);

        ApiManager.Instance.Register(
            new RegisterRequest { name = name, email = email, password = pass },
            onSuccess: res =>
            {
                UserSession.Set(res);
                MainManager.Instance?.RefreshPlayerName();
                gameObject.SetActive(false);
            },
            onError: err =>
            {
                loadingOverlay.SetActive(false);
                registerError.text = err;
            });
    }

    // ── CLOSE PANEL ──────────────────────────────────────────────────────────

    public void ClosePanel()
    {
        if (!UserSession.IsLoggedIn && !UserSession.IsGuest)
        {
            ShowLogin();
            return;
        }

        gameObject.SetActive(false);
    }

    // ── GUEST LOGIN ───────────────────────────────────────────────────────────

    public void OnGuestLoginClick()
    {
        UserSession.SetGuest();
        MainManager.Instance?.RefreshPlayerName();
        gameObject.SetActive(false);
    }

    // ── GOOGLE SIGN-IN ────────────────────────────────────────────────────────

    public void OnGoogleSignInClick()
    {
#if UNITY_ANDROID || UNITY_IOS
        loadingOverlay.SetActive(true);
        StartGoogleSignIn();
#else
        loginError.text = "Google Sign-In chỉ hoạt động trên Android/iOS.";
#endif
    }

    private void StartGoogleSignIn()
    {
        // Bỏ comment sau khi cài Google Sign-In plugin
        // GoogleSignIn.Configuration = new GoogleSignInConfiguration
        // {
        //     WebClientId    = "YOUR_WEB_CLIENT_ID.apps.googleusercontent.com",
        //     RequestIdToken = true
        // };
        // GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
        // {
        //     if (task.IsFaulted || task.IsCanceled)
        //     {
        //         UnityMainThreadDispatcher.Instance.Enqueue(() =>
        //         {
        //             loadingOverlay.SetActive(false);
        //             loginError.text = "Google Sign-In thất bại.";
        //         });
        //         return;
        //     }
        //     UnityMainThreadDispatcher.Instance.Enqueue(() =>
        //         SendGoogleToken(task.Result.IdToken));
        // });

        loadingOverlay.SetActive(false);
        loginError.text = "Chưa cài Google Sign-In plugin.";
    }

    public void SendGoogleToken(string idToken)
    {
        loadingOverlay.SetActive(true);

        ApiManager.Instance.GoogleLogin(
            new GoogleLoginRequest { idToken = idToken },
            onSuccess: res =>
            {
                UserSession.Set(res);
                gameObject.SetActive(false);
            },
            onError: err =>
            {
                loadingOverlay.SetActive(false);
                loginError.text = err;
            });
    }
}
