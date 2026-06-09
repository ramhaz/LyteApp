using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;
using LyteApp.Models;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunningLogController : ControllerBase
{
    private readonly AppDbContext _db;

    public RunningLogController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("today/{planId}")]
    public async Task<IActionResult> GetTodayLog(int planId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var log = await _db.RunningLogs
            .FirstOrDefaultAsync(l => l.PlanId == planId && l.LogDate == today);

        if (log == null)
            return NotFound(new { message = "Ingen løbe-log fundet for i dag." });

        return Ok(log);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddRunIntake([FromBody] AddRunRequest request)
    {
        var log = await _db.RunningLogs
            .FirstOrDefaultAsync(l => l.PlanId == request.PlanId && l.DayNumber == request.DayNumber);

        if (log == null)
            return NotFound(new { message = "Log ikke fundet." });

        log.LoggedKm += request.AmountKm;
        await _db.SaveChangesAsync();

        return Ok(log);
    }

    [HttpGet("all/{planId}")]
    public async Task<IActionResult> GetAllLogs(int planId)
    {
        var logs = await _db.RunningLogs
            .Where(l => l.PlanId == planId)
            .OrderBy(l => l.DayNumber)
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("history/{planId}")]
    public async Task<IActionResult> GetHistory(int planId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var logs = await _db.RunningLogs
            .Where(l => l.PlanId == planId && l.LogDate <= today)
            .OrderByDescending(l => l.LogDate)
            .ToListAsync();

        return Ok(logs);
    }
}
