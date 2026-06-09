// US 5.5 – Datamodel for ordre (oprettes ved køb)
// US 5.6 – Bruges til at hente brugerens købshistorik
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LyteApp.Models;

[Table("orders")]
public class Order
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    [Required]
    public Guid UserId { get; set; }

    [Column("total_price")]
    public double TotalPrice { get; set; }

    [Column("status")]
    [MaxLength(50)]
    public string Status { get; set; } = "confirmed";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}
