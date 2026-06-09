// US 5.5 – POST /api/order: Opret en ny ordre (køb)
// US 5.6 – GET /api/order: Hent brugerens købshistorik (nyeste først)
// US 5.7 – Fejlhåndtering: tom kurv og ugyldige ordrer returnerer 400
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LyteApp.Data;
using LyteApp.Models;
using System.Security.Claims;

namespace LyteAppRest.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrderController(AppDbContext db)
    {
        _db = db;
    }

    // US 5.6 – Hent brugerens ordrer sorteret nyeste først
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var orders = await _db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    // US 5.5 – Opret ny ordre fra kurv
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            return BadRequest(new { message = "Kurven er tom – tilføj mindst ét produkt." });

        if (request.Items.Any(i => i.Quantity <= 0))
            return BadRequest(new { message = "Antal skal være mindst 1 for alle produkter." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var order = new Order
        {
            UserId = userId,
            TotalPrice = request.Items.Sum(i => i.UnitPrice * i.Quantity),
            Status = "confirmed",
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList(),
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}

public class CreateOrderRequest
{
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
}
