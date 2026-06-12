// ── REQUEST MODELS ────────────────────────────────────────────────────────────

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string password;
    public string name;
}

[System.Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[System.Serializable]
public class UpdateProgressRequest
{
    public int newLevel;
}

[System.Serializable]
public class GoogleLoginRequest
{
    public string idToken;
}

// ── RESPONSE MODELS ───────────────────────────────────────────────────────────

[System.Serializable]
public class AuthResponse
{
    public long   id;
    public string email;
    public string name;
    public int    highestLevel;
    public string token;
}
