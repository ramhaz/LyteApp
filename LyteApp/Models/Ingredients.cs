using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LyteApp.Models;

[Table("ingredients")]
public class Ingredient
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("amount")]
    public double Amount { get; set; }

    [Column("unit")]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    [Column("type")]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Column("product_id")]
    public int ProductId { get; set; }

    [JsonIgnore]
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}