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
    [AllowAnonymous]  // ← midlertidigt igen
    public async Task<IActionResult> GetAll()
    {
        var challenges = await _context.Challenges
            .Where(c => c.IsActive)
            .OrderBy(c => c.Points)
            .ToListAsync();

        return Ok(challenges);
    }
    
    // Task 2: Hent brugerens tilmeldte challenges
    [HttpGet("my")]
    [Authorize]  
    public async Task<IActionResult> GetUserChallenges()
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);

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
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);

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
}