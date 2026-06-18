using System.ComponentModel.DataAnnotations.Schema;

namespace GameNotCrazy.API.Models;

[Table("users")]
public class User
{
    [Column("id")]
    public long Id { get; set; }

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("highest_level")]
    public int HighestLevel { get; set; } = 1;

    [Column("gold")]
    public int Gold { get; set; } = 0;

    [Column("is_no_ads")]
    public bool IsNoAds { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
