using System;

[Serializable]
public class SaveData
{
    public int    highestLevel  = 1;
    public int    selectedDino  = 0;

    // Auth — lưu để auto-login khi mở lại app
    public long   savedUserId   = 0;
    public string savedEmail    = "";
    public string savedName     = "";
    public string savedToken    = "";
    public int    savedHighestLevel = 1;
    public int    savedGold         = 0;
}
