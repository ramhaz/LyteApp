using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;
using LyteApp.Models;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HydrationLogController : ControllerBase
{
    private readonly AppDbContext _db;

    public HydrationLogController(AppDbContext db)
    {
        _db = db;
    }

    // US 3.2 - GET 
    [HttpGet("today/{planId}")]
    public async Task<IActionResult> GetTodayLog(int planId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var log = await _db.HydrationLogs
            .FirstOrDefaultAsync(l => l.PlanId == planId && l.LogDate == today);

        if (log == null)
            return NotFound(new { message = "Ingen log fundet for i dag." });

        return Ok(log);
    }

    // US 3.3 - POST 
    [HttpPost("add")]
    public async Task<IActionResult> AddWaterIntake([FromBody] AddWaterRequest request)
    {
        var log = await _db.HydrationLogs
            .FirstOrDefaultAsync(l => l.PlanId == request.PlanId && l.DayNumber == request.DayNumber);

        if (log == null)
            return NotFound(new { message = "Log ikke fundet." });

        log.LoggedMl += request.AmountMl;
        await _db.SaveChangesAsync();

        return Ok(log);
    }

    // US 3.5 - GET
    [HttpGet("all/{planId}")]
    public async Task<IActionResult> GetAllLogs(int planId)
    {
        var logs = await _db.HydrationLogs
            .Where(l => l.PlanId == planId)
            .OrderBy(l => l.DayNumber)
            .ToListAsync();

        return Ok(logs);
    }

    // US 3.6 - GET
    [HttpGet("history/{planId}")]
    public async Task<IActionResult> GetHistory(int planId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var logs = await _db.HydrationLogs
            .Where(l => l.PlanId == planId && l.LogDate <= today)
            .OrderByDescending(l => l.LogDate)
            .ToListAsync();

        return Ok(logs);
    }
}

