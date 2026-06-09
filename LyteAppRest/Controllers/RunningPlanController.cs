using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;
using LyteApp.Models;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunningPlanController : ControllerBase
{
    private readonly AppDbContext _db;

    public RunningPlanController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("active/{userId}")]
    public async Task<IActionResult> GetActivePlan(Guid userId)
    {
        var plan = await _db.RunningPlans
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (plan == null)
            return NotFound(new { message = "Ingen aktiv løbeplan fundet." });

        return Ok(plan);
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartPlan([FromBody] StartPlanRequest request)
    {
        try
        {
            var existing = await _db.RunningPlans
                .AnyAsync(p => p.UserId == request.UserId && p.IsActive);

            if (existing)
                return BadRequest(new { message = "Du har allerede en aktiv løbeplan." });

            var plan = new RunningPlan
            {
                UserId = request.UserId,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
            };

            _db.RunningPlans.Add(plan);
            await _db.SaveChangesAsync();

            var logs = new List<RunningLog>();
            for (int day = 1; day <= 30; day++)
            {
                logs.Add(new RunningLog
                {
                    PlanId = plan.Id,
                    UserId = request.UserId,
                    DayNumber = day,
                    TargetKm = GetTargetKm(day),
                    LoggedKm = 0,
                    LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(day - 1)),
                });
            }

            _db.RunningLogs.AddRange(logs);
            await _db.SaveChangesAsync();

            return Ok(plan);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
        }
    }

    private static decimal GetTargetKm(int dayNumber)
    {
        if (dayNumber <= 10) return 1.0m;
        if (dayNumber <= 20) return 1.5m;
        return 2.0m;
    }

    [HttpPost("restart")]
    public async Task<IActionResult> RestartPlan([FromBody] StartPlanRequest request)
    {
        var oldPlan = await _db.RunningPlans
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.IsActive);

        if (oldPlan != null)
        {
            oldPlan.IsActive = false;
        }

        var newPlan = new RunningPlan
        {
            UserId = request.UserId,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
        };

        _db.RunningPlans.Add(newPlan);
        await _db.SaveChangesAsync();

        var logs = new List<RunningLog>();
        for (int day = 1; day <= 30; day++)
        {
            logs.Add(new RunningLog
            {
                PlanId = newPlan.Id,
                UserId = request.UserId,
                DayNumber = day,
                TargetKm = GetTargetKm(day),
                LoggedKm = 0,
                LogDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(day - 1)),
            });
        }

        _db.RunningLogs.AddRange(logs);
        await _db.SaveChangesAsync();

        return Ok(newPlan);
    }
}
