// Quản lý trạng thái đăng nhập trong runtime và local storage.
public static class UserSession
{
    public static AuthResponse Current { get; private set; }
    public static bool IsGuest { get; private set; }

    public static bool IsLoggedIn => Current != null && !string.IsNullOrEmpty(Current.token);

    // Gọi sau khi login/register thành công — lưu vào RAM và disk
    public static void Set(AuthResponse user)
    {
        Current = user;
        GameData.SelectedLevel = user.highestLevel;

        SaveManager.Data.savedUserId       = user.id;
        SaveManager.Data.savedEmail        = user.email;
        SaveManager.Data.savedName         = user.name;
        SaveManager.Data.savedToken        = user.token;
        SaveManager.Data.savedHighestLevel = user.highestLevel;
        SaveManager.Data.savedGold         = user.gold;
        SaveManager.Save();
    }

    // Gọi khi app khởi động — thử khôi phục session từ disk
    public static bool TryRestore()
    {
        var data = SaveManager.Data;
        if (string.IsNullOrEmpty(data.savedToken) || data.savedUserId == 0)
            return false;

        Current = new AuthResponse
        {
            id           = data.savedUserId,
            email        = data.savedEmail,
            name         = data.savedName,
            token        = data.savedToken,
            highestLevel = data.savedHighestLevel,
            gold         = data.savedGold
        };

        GameData.SelectedLevel = Current.highestLevel;
        return true;
    }

    public static bool SpendGold(int amount)
    {
        if (Current == null || Current.gold < amount) return false;
        Current.gold -= amount;
        SaveManager.Data.savedGold = Current.gold;
        SaveManager.Save();
        ApiManager.Instance?.UpdateGold(-amount);
        return true;
    }

    // Gọi khi chơi chế độ khách — chỉ set RAM, không lưu disk
    public static void SetGuest()
    {
        IsGuest = true;
        Current = new AuthResponse
        {
            id           = 0,
            email        = "",
            name         = "Khách",
            token        = "",
            highestLevel = 1
        };
        GameData.SelectedLevel = 1;
    }

    // Gọi khi logout
    public static void Clear()
    {
        Current = null;
        IsGuest = false;
        SaveManager.Data.savedUserId  = 0;
        SaveManager.Data.savedEmail   = "";
        SaveManager.Data.savedName    = "";
        SaveManager.Data.savedToken   = "";
        SaveManager.Save();
    }
}
