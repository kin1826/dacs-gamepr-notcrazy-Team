using System.ComponentModel.DataAnnotations;

namespace GameNotCrazy.API.DTOs;

public class UpdateProgressRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Level must be at least 1.")]
    public int NewLevel { get; set; }
}
