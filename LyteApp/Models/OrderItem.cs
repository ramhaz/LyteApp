// US 5.5 – Datamodel for en enkelt vare i en ordre
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LyteApp.Models;

[Table("order_items")]
public class OrderItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("order_id")]
    [Required]
    public int OrderId { get; set; }

    [Column("product_id")]
    [Required]
    public int ProductId { get; set; }

    [Column("product_name")]
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("unit_price")]
    public double UnitPrice { get; set; }

    [ForeignKey("OrderId")]
    public Order? Order { get; set; }
}
