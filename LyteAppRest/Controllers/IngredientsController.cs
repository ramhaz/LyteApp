using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngredientController : ControllerBase
{
    private readonly AppDbContext _db;

    public IngredientController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/ingredient
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ingredients = await _db.Ingredients.ToListAsync();
        return Ok(ingredients);
    }

    // GET /api/ingredient/product/{productId}
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)

    {
        var productExists = await _db.Products.AnyAsync(p => p.Id == productId);

        
        var ingredients = await _db.Ingredients
            .Where(i => i.ProductId == productId)
            .ToListAsync();
       
        if (!productExists)
            return NotFound(new { message = "Produktet blev ikke fundet." });

        return Ok(ingredients);
    }
}