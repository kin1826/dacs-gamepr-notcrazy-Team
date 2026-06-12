// Quản lý trạng thái đăng nhập trong runtime và local storage.
public static class UserSession
{
    public static AuthResponse Current { get; private set; }

    public static bool IsLoggedIn => Current != null && !string.IsNullOrEmpty(Current.token);

    // Gọi sau khi login/register thành công — lưu vào RAM và disk
    public static void Set(AuthResponse user)
    {
        Current = user;
        GameData.SelectedLevel = user.highestLevel;

        // Lưu xuống file để auto-login lần sau
        SaveManager.Data.savedUserId       = user.id;
        SaveManager.Data.savedEmail        = user.email;
        SaveManager.Data.savedName         = user.name;
        SaveManager.Data.savedToken        = user.token;
        SaveManager.Data.savedHighestLevel = user.highestLevel;
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
            highestLevel = data.savedHighestLevel
        };

        GameData.SelectedLevel = Current.highestLevel;
        return true;
    }

    // Gọi khi logout
    public static void Clear()
    {
        Current = null;
        SaveManager.Data.savedUserId  = 0;
        SaveManager.Data.savedEmail   = "";
        SaveManager.Data.savedName    = "";
        SaveManager.Data.savedToken   = "";
        SaveManager.Save();
    }
}
