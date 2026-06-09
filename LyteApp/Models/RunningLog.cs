using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LyteApp.Models;

[Table(name: "running_logs")]
public class RunningLog
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

    [Column(name: "target_km", TypeName = "numeric(4,1)")]
    public decimal TargetKm { get; set; }

    [Column(name: "logged_km", TypeName = "numeric(5,2)")]
    public decimal LoggedKm { get; set; } = 0;

    [Column(name: "log_date")]
    [Required]
    public DateOnly LogDate { get; set; }

    [Column(name: "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PlanId")]
    public RunningPlan? RunningPlan { get; set; }
}
