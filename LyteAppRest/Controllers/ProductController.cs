using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    // US 1.1 – GET /api/product (opdateret)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _db.Products
            .Include(p => p.Ingredients) 
            .ToListAsync();
        return Ok(products);
    }

// US 1.2 – GET /api/product/{id} (opdateret)
// dette er for selve produktet med de givende ingrediencers. så har lidt samme effektivitet -
// som igrediendts api-et
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _db.Products
            .Include(p => p.Ingredients) 
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { message = "Produktet blev ikke fundet." });

        return Ok(product);
    }
}


