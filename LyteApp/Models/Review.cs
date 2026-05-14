using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LyteApp.Models;

[Table("reviews")]
public class Review
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("rating")]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("text")]
    [Required]
    [MaxLength(500)]
    public string Text { get; set; } = string.Empty;

    [Column("date")]
    public DateTime Date { get; set; }
}