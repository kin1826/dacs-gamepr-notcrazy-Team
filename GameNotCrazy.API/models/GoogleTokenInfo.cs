using System.Text.Json.Serialization;

namespace GameNotCrazy.API.Models;

// Map JSON từ https://oauth2.googleapis.com/tokeninfo
public class GoogleTokenInfo
{
    [JsonPropertyName("sub")]            public string Sub           { get; set; } = null!;
    [JsonPropertyName("email")]          public string Email         { get; set; } = null!;
    [JsonPropertyName("email_verified")] public string EmailVerified { get; set; } = null!;
    [JsonPropertyName("name")]           public string? Name         { get; set; }
    [JsonPropertyName("aud")]            public string Aud           { get; set; } = null!;
}
