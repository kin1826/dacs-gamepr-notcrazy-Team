using System.ComponentModel.DataAnnotations.Schema;

namespace GameNotCrazy.API.Models;

[Table("payment_requests")]
public class PaymentRequest
{
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public long UserId { get; set; }

    [Column("code")]
    public string Code { get; set; } = null!;

    [Column("gold_amount")]
    public int GoldAmount { get; set; }

    [Column("price")]
    public int Price { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("note")]
    public string? Note { get; set; }
}
