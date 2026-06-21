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

// ── LEADERBOARD ───────────────────────────────────────────────────────────────

[System.Serializable]
public class LeaderboardEntry
{
    public long   id;
    public string name;
    public int    highestLevel;
}

[System.Serializable]
public class LeaderboardResponse
{
    public LeaderboardEntry[] items;
}

// ── GOLD SYNC ─────────────────────────────────────────────────────────────────

[System.Serializable]
public class GoldResponse { public int gold; }

[System.Serializable]
public class UpdateGoldApiRequest { public int delta; }

// ── TOPUP ─────────────────────────────────────────────────────────────────────

[System.Serializable]
public class CreateTopUpRequest
{
    public int goldAmount;
}

[System.Serializable]
public class TopUpResponse
{
    public string code;
    public int    goldAmount;
    public int    price;
}

// ── RESPONSE MODELS ───────────────────────────────────────────────────────────

[System.Serializable]
public class AuthResponse
{
    public long   id;
    public string email;
    public string name;
    public int    highestLevel;
    public int    gold;
    public string token;
}
