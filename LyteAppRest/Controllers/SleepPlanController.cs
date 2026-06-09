using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;
using LyteApp.Models;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SleepPlanController : ControllerBase
{
    private readonly AppDbContext _db;

    public SleepPlanController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("active/{userId}")]
    public async Task<IActionResult> GetActivePlan(Guid userId)
    {
        var plan = await _db.SleepPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (plan == null)
            return NotFound(new { message = "Ingen aktiv søvnplan fundet." });

        return Ok(plan);
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartPlan([FromBody] StartPlanRequest request)
    {
        try
        {
            var existing = await _db.SleepPlans
                .AnyAsync(p => p.UserId == request.UserId && p.IsActive);

            if (existing)
                return BadRequest(new { message = "Du har allerede en aktiv søvnplan." });

            var plan = new SleepPlan
            {
                UserId = request.UserId,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
            };

            _db.SleepPlans.Add(plan);
            await _db.SaveChangesAsync();

            var logs = new List<SleepLog>();
            for (int day = 1; day <= 30; day++)
            {
                logs.Add(new SleepLog
                {
                    PlanId = plan.Id,
                    UserId = request.UserId,
                    DayNumber = day,
                    TargetHours = GetTargetHours(day),
                    LoggedHours = 0,
                    LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(day - 1)),
                });
            }

            _db.SleepLogs.AddRange(logs);
            await _db.SaveChangesAsync();

            return Ok(plan);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    private static decimal GetTargetHours(int dayNumber)
    {
        if (dayNumber <= 10) return 6.0m;
        if (dayNumber <= 20) return 6.5m;
        return 7.0m;
    }

    [HttpPost("restart")]
    public async Task<IActionResult> RestartPlan([FromBody] StartPlanRequest request)
    {
        var oldPlan = await _db.SleepPlans
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.IsActive);

        if (oldPlan != null)
        {
            oldPlan.IsActive = false;
        }

        var newPlan = new SleepPlan
        {
            UserId = request.UserId,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
        };

        _db.SleepPlans.Add(newPlan);
        await _db.SaveChangesAsync();

        var logs = new List<SleepLog>();
        for (int day = 1; day <= 30; day++)
        {
            logs.Add(new SleepLog
            {
                PlanId = newPlan.Id,
                UserId = request.UserId,
                DayNumber = day,
                TargetHours = GetTargetHours(day),
                LoggedHours = 0,
                LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(day - 1)),
            });
        }

        _db.SleepLogs.AddRange(logs);
        await _db.SaveChangesAsync();

        return Ok(newPlan);
    }
}
