using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LyteApp.Models;

[Table(name: "running_plans")]
public class RunningPlan
{
    [Key]
    [Column(name: "id")]
    public int Id { get; set; }

    [Column(name: "user_id")]
    [Required]
    public Guid UserId { get; set; }

    [Column(name: "start_date")]
    [Required]
    public DateOnly StartDate { get; set; }

    [Column(name: "is_active")]
    public bool IsActive { get; set; } = true;

    [Column(name: "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RunningLog> RunningLogs { get; set; } = new();
}
