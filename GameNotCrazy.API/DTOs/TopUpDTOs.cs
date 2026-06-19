using System.ComponentModel.DataAnnotations;

namespace GameNotCrazy.API.DTOs;

public class CreateTopUpRequest
{
    [Required]
    public int GoldAmount { get; set; }
}

public class TopUpResponse
{
    public string Code       { get; set; } = null!;
    public int    GoldAmount { get; set; }
    public int    Price      { get; set; }
}

public class RejectTopUpRequest
{
    public string? Note { get; set; }
}
