using System.ComponentModel.DataAnnotations;

namespace GameNotCrazy.API.DTOs;

public class UpdateGoldRequest
{
    // Dương = cộng vàng, âm = trừ vàng. Server tự clamp về 0 nếu âm.
    [Required]
    public int Delta { get; set; }
}

public class SetGoldRequest
{
    [Required]
    public int Gold { get; set; }
}
