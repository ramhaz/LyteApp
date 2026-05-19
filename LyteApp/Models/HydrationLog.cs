using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LyteApp.Models;

[Table(name: "hydration_logs")]
public class HydrationLog
{
    [Key]
    [Column(name: "id")]
    public int Id { get; set; }

    [Column(name: "plan_id")]
    [Required]
    public int PlanId { get; set; }

    [Column(name: "user_id")]
    [Required]
    public Guid UserId { get; set; }

    [Column(name: "day_number")]
    [Required]
    public int DayNumber { get; set; }

    [Column(name: "target_ml")]
    public int TargetMl { get; set; } = 2500;

    [Column(name: "logged_ml")]
    public int LoggedMl { get; set; } = 0;

    [Column(name: "log_date")]
    [Required]
    public DateOnly LogDate { get; set; }

    [Column(name: "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PlanId")]
    public HydrationPlan? HydrationPlan { get; set; }
}