using System.ComponentModel.DataAnnotations;

namespace GameNotCrazy.API.DTOs;

public class GoogleLoginRequest
{
    [Required]
    public string IdToken { get; set; } = null!;
}
