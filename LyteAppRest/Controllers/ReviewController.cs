using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReviewController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reviews = await _db.Reviews
            .OrderByDescending(r => r.Date)
            .ToListAsync();
        return Ok(reviews);
    }
}