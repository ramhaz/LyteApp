using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;
using LyteApp.Models;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SleepLogController : ControllerBase
{
    private readonly AppDbContext _db;

    public SleepLogController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("today/{planId}")]
    public async Task<IActionResult> GetTodayLog(int planId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var log = await _db.SleepLogs
            .FirstOrDefaultAsync(l => l.PlanId == planId && l.LogDate == today);

        if (log == null)
            return NotFound(new { message = "Ingen søvn-log fundet for i dag." });

        return Ok(log);
    }

    // Søvn overskriver i stedet for at akkumulere
    [HttpPost("add")]
    public async Task<IActionResult> AddSleepEntry([FromBody] AddSleepRequest request)
    {
        var log = await _db.SleepLogs
            .FirstOrDefaultAsync(l => l.PlanId == request.PlanId && l.DayNumber == request.DayNumber);

        if (log == null)
            return NotFound(new { message = "Log ikke fundet." });

        log.LoggedHours = request.Hours;
        await _db.SaveChangesAsync();

        return Ok(log);
    }

    [HttpGet("all/{planId}")]
    public async Task<IActionResult> GetAllLogs(int planId)
    {
        var logs = await _db.SleepLogs
            .Where(l => l.PlanId == planId)
            .OrderBy(l => l.DayNumber)
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("history/{planId}")]
    public async Task<IActionResult> GetHistory(int planId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var logs = await _db.SleepLogs
            .Where(l => l.PlanId == planId && l.LogDate <= today)
            .OrderByDescending(l => l.LogDate)
            .ToListAsync();

        return Ok(logs);
    }
}
