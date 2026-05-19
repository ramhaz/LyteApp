using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LyteApp.Models;

namespace LyteApi.Models;

[Table("user_challenges")]
public class UserChallenge
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("challenge_id")]
    public Guid ChallengeId { get; set; }

    [ForeignKey("ChallengeId")]
    public Challenge Challenge { get; set; } = null!;

    [Column("progress")]
    public int Progress { get; set; } = 0;

    [Column("is_completed")]
    public bool IsCompleted { get; set; } = false;

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}