namespace LyteApp;

    
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column("price")]
    public double Price { get; set; }

    [Column("image_url")]
    [MaxLength(300)]
    public string ImageUrl { get; set; } = string.Empty;
}
    

