using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;
using LyteApp.Models;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HydrationPlanController : ControllerBase
{
    private readonly AppDbContext _db;

    public HydrationPlanController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/hydrationplan/active/{userId}
    [HttpGet("active/{userId}")]
    public async Task<IActionResult> GetActivePlan(Guid userId)
    {
        var plan = await _db.HydrationPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (plan == null)
            return NotFound(new { message = "Ingen aktiv plan fundet." });

        return Ok(plan);
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartPlan([FromBody] StartPlanRequest request)
    {
        try
        {
            var existing = await _db.HydrationPlans
                .AnyAsync(p => p.UserId == request.UserId && p.IsActive);

            if (existing)
                return BadRequest(new { message = "Du har allerede en aktiv plan." });

            var plan = new HydrationPlan
            {
                UserId = request.UserId,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
            };

            _db.HydrationPlans.Add(plan);
            await _db.SaveChangesAsync();

            var logs = new List<HydrationLog>();
            for (int day = 1; day <= 30; day++)
            {
                logs.Add(new HydrationLog
                {
                    PlanId = plan.Id,
                    UserId = request.UserId,
                    DayNumber = day,
                    TargetMl = GetTargetMl(day),
                    LoggedMl = 0,
                    LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(day - 1)),
                });
            }

            _db.HydrationLogs.AddRange(logs);
            await _db.SaveChangesAsync();

            return Ok(plan);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }
    private static int GetTargetMl(int dayNumber)
    {
        if (dayNumber <= 10) return 2000;
        if (dayNumber <= 20) return 2500;
        return 3000;
    }
    
    
    // POST /api/hydrationplan/restart
    [HttpPost("restart")]
    public async Task<IActionResult> RestartPlan([FromBody] StartPlanRequest request)
    {
        var oldPlan = await _db.HydrationPlans
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.IsActive);

        if (oldPlan != null)
        {
            oldPlan.IsActive = false;
        }

        var newPlan = new HydrationPlan
        {
            UserId = request.UserId,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
        };

        _db.HydrationPlans.Add(newPlan);
        await _db.SaveChangesAsync();

        var logs = new List<HydrationLog>();
        for (int day = 1; day <= 30; day++)
        {
            logs.Add(new HydrationLog
            {
                PlanId = newPlan.Id,
                UserId = request.UserId,
                DayNumber = day,
                TargetMl = GetTargetMl(day),
                LoggedMl = 0,
                LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(day - 1)),
            });
        }

        _db.HydrationLogs.AddRange(logs);
        await _db.SaveChangesAsync();

        return Ok(newPlan);
    }
    
}

