namespace GameNotCrazy.API.DTOs;

public class AuthResponse
{
    public long   Id           { get; set; }
    public string Email        { get; set; } = null!;
    public string Name         { get; set; } = null!;
    public int    HighestLevel { get; set; }
    public int    Gold         { get; set; }
    public string Token        { get; set; } = null!;
}
