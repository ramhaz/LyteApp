using LyteApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;

namespace LyteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChallengeController : ControllerBase
{
    private readonly AppDbContext _context;

    public ChallengeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var query = _context.Challenges.Where(c => c.IsActive);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(c => c.Category == category);

        var challenges = await query
            .OrderBy(c => c.Points)
            .ToListAsync();

        return Ok(challenges);
    }
    
    // Task 2: Hent brugerens tilmeldte challenges
    [HttpGet("my")]
    [Authorize]  
    public async Task<IActionResult> GetUserChallenges()
    {
        var userIdClaim = User.FindFirst("sub") 
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized(new { message = "Bruger ID ikke fundet i token" });

        var userId = Guid.Parse(userIdClaim.Value);

        var userChallenges = await _context.UserChallenges
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Challenge)
            .ToListAsync();

        return Ok(userChallenges);
    }

    // Task 1 + 3: Tilmeld bruger til en challenge (med duplikat-tjek)
    [HttpPost("join/{challengeId}")]
    [Authorize]  
    public async Task<IActionResult> JoinChallenge(Guid challengeId)
    {
        var userIdClaim = User.FindFirst("sub") 
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized(new { message = "Bruger ID ikke fundet i token" });

        var userId = Guid.Parse(userIdClaim.Value);

        // Task 3: Tjek om brugeren allerede er tilmeldt
        var alreadyJoined = await _context.UserChallenges
            .AnyAsync(uc => uc.UserId == userId && uc.ChallengeId == challengeId);

        if (alreadyJoined)
        {
            return Conflict(new { message = "Du er allerede tilmeldt denne challenge" });
        }

        // Tjek om challenge eksisterer
        var challenge = await _context.Challenges.FindAsync(challengeId);
        if (challenge == null)
        {
            return NotFound(new { message = "Challenge ikke fundet" });
        }

        var userChallenge = new UserChallenge
        {
            UserId = userId,
            ChallengeId = challengeId,
            Progress = 0,
            IsCompleted = false,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserChallenges.Add(userChallenge);
        await _context.SaveChangesAsync();

        return Ok(userChallenge);
    }
    [HttpGet("progress")]
    [Authorize]
    public async Task<IActionResult> GetChallengeProgress() 
    
    {
    var userIdClaim = User.FindFirst("sub") 
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

    if (userIdClaim == null)
        return Unauthorized(new { message = "Bruger ID ikke fundet i token" });

    var userId = Guid.Parse(userIdClaim.Value);
    
    var userChallenges = await _context.UserChallenges
        .Where(uc => uc.UserId == userId)
        .Include(uc => uc.Challenge)
        .ToListAsync();

    var hydrationLogs = await _context.HydrationLogs
        .Where(l => l.UserId == userId)
        .OrderBy(l => l.LogDate)
        .ToListAsync();

    var runningLogs = await _context.RunningLogs
        .Where(l => l.UserId == userId)
        .OrderBy(l => l.LogDate)
        .ToListAsync();

    var sleepLogs = await _context.SleepLogs
        .Where(l => l.UserId == userId)
        .OrderBy(l => l.LogDate)
        .ToListAsync();

    var result = new List<object>();

    foreach (var uc in userChallenges)
    {
        var challenge = uc.Challenge;
        int progress = 0;
        int target = challenge.TargetValue;
        string progressText = "";
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (challenge.Category == "running")
        {
            switch (challenge.Type)
            {
                case "streak":
                    var runSuccessDates = runningLogs
                        .Where(l => l.LoggedKm >= l.TargetKm)
                        .Select(l => l.LogDate)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .ToList();

                    int runStreak = 0;
                    for (int i = 0; i < runSuccessDates.Count; i++)
                    {
                        if (runSuccessDates.Contains(today.AddDays(-i)))
                            runStreak++;
                        else
                            break;
                    }
                    progress = Math.Min(runStreak, target);
                    progressText = $"{progress} af {target} dage i træk";
                    break;

                case "single":
                    var bestRunDay = runningLogs
                        .GroupBy(l => l.LogDate)
                        .Select(g => (int)Math.Round(g.Sum(l => l.LoggedKm) * 10))
                        .DefaultIfEmpty(0)
                        .Max();
                    var targetTenths = target * 10;
                    progress = bestRunDay >= targetTenths ? target : (int)Math.Round(runningLogs
                        .GroupBy(l => l.LogDate)
                        .Select(g => g.Sum(l => l.LoggedKm))
                        .DefaultIfEmpty(0)
                        .Max());
                    progressText = $"{progress} af {target} km";
                    break;

                case "plan":
                    var runDaysCompleted = runningLogs
                        .Where(l => l.LoggedKm >= l.TargetKm)
                        .Select(l => l.LogDate)
                        .Distinct()
                        .Count();
                    progress = Math.Min(runDaysCompleted, target);
                    progressText = $"{progress} af {target} dage klaret";
                    break;
            }
        }
        else if (challenge.Category == "sleep")
        {
            switch (challenge.Type)
            {
                case "streak":
                    var sleepSuccessDates = sleepLogs
                        .Where(l => l.LoggedHours >= l.TargetHours)
                        .Select(l => l.LogDate)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .ToList();

                    int sleepStreak = 0;
                    for (int i = 0; i < sleepSuccessDates.Count; i++)
                    {
                        if (sleepSuccessDates.Contains(today.AddDays(-i)))
                            sleepStreak++;
                        else
                            break;
                    }
                    progress = Math.Min(sleepStreak, target);
                    progressText = $"{progress} af {target} nætter i træk";
                    break;

                case "single":
                    var bestSleepNight = sleepLogs
                        .Select(l => l.LoggedHours)
                        .DefaultIfEmpty(0)
                        .Max();
                    progress = Math.Min((int)bestSleepNight, target);
                    progressText = $"{progress} af {target} timer";
                    break;

                case "plan":
                    var sleepDaysCompleted = sleepLogs
                        .Where(l => l.LoggedHours >= l.TargetHours)
                        .Select(l => l.LogDate)
                        .Distinct()
                        .Count();
                    progress = Math.Min(sleepDaysCompleted, target);
                    progressText = $"{progress} af {target} nætter klaret";
                    break;
            }
        }
        else
        {
            switch (challenge.Type)
            {
                case "streak":
                    var successDates = hydrationLogs
                        .Where(l => l.LoggedMl >= l.TargetMl)
                        .Select(l => l.LogDate)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .ToList();

                    int streak = 0;
                    for (int i = 0; i < successDates.Count; i++)
                    {
                        if (successDates.Contains(today.AddDays(-i)))
                            streak++;
                        else
                            break;
                    }
                    progress = Math.Min(streak, target);
                    progressText = $"{progress} af {target} dage i træk";
                    break;

                case "single":
                    var bestDay = hydrationLogs
                        .GroupBy(l => l.LogDate)
                        .Select(g => g.Sum(l => l.LoggedMl))
                        .DefaultIfEmpty(0)
                        .Max();
                    progress = Math.Min(bestDay, target);
                    progressText = $"{progress} af {target} ml";
                    break;

                case "plan":
                    var daysCompleted = hydrationLogs
                        .Where(l => l.LoggedMl >= l.TargetMl)
                        .Select(l => l.LogDate)
                        .Distinct()
                        .Count();
                    progress = Math.Min(daysCompleted, target);
                    progressText = $"{progress} af {target} dage klaret";
                    break;
            }
        }

        // Opdater progress i databasen
        uc.Progress = progress;
        bool justCompleted = !uc.IsCompleted && progress >= target;
        if (justCompleted)
        {
            uc.IsCompleted = true;
            uc.CompletedAt = DateTime.UtcNow;
        }

        result.Add(new
        {
            challengeId = challenge.Id,
            title = challenge.Title,
            type = challenge.Type,
            points = challenge.Points,
            progress,
            target,
            progressText,
            progressPercent = target > 0 ? Math.Round((double)progress / target * 100) : 0,
            isCompleted = uc.IsCompleted,
            completedAt = uc.CompletedAt
        });
    }
    

    await _context.SaveChangesAsync();
    return Ok(result);
}
    [HttpGet("points")]
    [Authorize]
    public async Task<IActionResult> GetTotalPoints()
    {
        var userIdClaim = User.FindFirst("sub") 
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized(new { message = "Bruger ID ikke fundet i token" });

        var userId = Guid.Parse(userIdClaim.Value);

        var totalPoints = await _context.UserChallenges
            .Where(uc => uc.UserId == userId && uc.IsCompleted)
            .Include(uc => uc.Challenge)
            .SumAsync(uc => uc.Challenge.Points);

        var completedCount = await _context.UserChallenges
            .CountAsync(uc => uc.UserId == userId && uc.IsCompleted);

        return Ok(new
        {
            totalPoints,
            completedChallenges = completedCount
        });
    }
    
    
    
}