using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LyteApp.Models;

[Table(name: "sleep_logs")]
public class SleepLog
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

    [Column(name: "target_hours", TypeName = "numeric(3,1)")]
    public decimal TargetHours { get; set; }

    [Column(name: "logged_hours", TypeName = "numeric(3,1)")]
    public decimal LoggedHours { get; set; } = 0;

    [Column(name: "log_date")]
    [Required]
    public DateOnly LogDate { get; set; }

    [Column(name: "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PlanId")]
    public SleepPlan? SleepPlan { get; set; }
}
