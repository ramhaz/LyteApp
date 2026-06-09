using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace LyteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private static readonly HttpClient _http = new();

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var projectId = _config["Supabase:ProjectId"];
        var anonKey = _config["Supabase:AnonKey"];
        var url = $"https://{projectId}.supabase.co/auth/v1/token?grant_type=password";

        var body = JsonSerializer.Serialize(new { email = request.Email, password = request.Password });
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Content = new StringContent(body, Encoding.UTF8, "application/json");
        httpRequest.Headers.Add("apikey", anonKey);

        var response = await _http.SendAsync(httpRequest);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, result);

        return Ok(JsonSerializer.Deserialize<JsonElement>(result));
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}