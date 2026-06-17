using System.Text.Json;
using GameNotCrazy.API.Data;
using GameNotCrazy.API.DTOs;
using GameNotCrazy.API.Models;
using GameNotCrazy.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameNotCrazy.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext        context,
    TokenService        tokenService,
    IHttpClientFactory  httpClientFactory,
    IConfiguration      config) : ControllerBase
{
    // POST /api/auth/register
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (context.Users.Any(u => u.Email == request.Email))
            return BadRequest("Email already exists.");

        var user = new User
        {
            Email        = request.Email,
            Name         = request.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            HighestLevel = 1,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        context.Users.Add(user);
        context.SaveChanges();

        return Ok(BuildResponse(user));
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
            return BadRequest("User not found.");

        if (user.PasswordHash == "GOOGLE_AUTH")
            return BadRequest("This account uses Google Sign-In. Please login with Google.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return BadRequest("Wrong password.");

        return Ok(BuildResponse(user));
    }

    // POST /api/auth/google
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        // Xác minh token với Google
        var http = httpClientFactory.CreateClient();
        var res  = await http.GetAsync(
            $"https://oauth2.googleapis.com/tokeninfo?id_token={request.IdToken}");

        if (!res.IsSuccessStatusCode)
            return BadRequest("Invalid Google token.");

        var json      = await res.Content.ReadAsStringAsync();
        var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(json);

        if (tokenInfo == null || tokenInfo.EmailVerified != "true")
            return BadRequest("Google token verification failed.");

        // Kiểm tra audience khớp với ClientId của app (bảo mật)
        var expectedClientId = config["Google:ClientId"];
        if (!string.IsNullOrEmpty(expectedClientId)
            && expectedClientId != "YOUR_WEB_CLIENT_ID.apps.googleusercontent.com"
            && tokenInfo.Aud != expectedClientId)
            return BadRequest("Token audience mismatch.");

        // Tìm user theo email, hoặc tạo mới nếu chưa có
        var user = context.Users.FirstOrDefault(u => u.Email == tokenInfo.Email);

        if (user == null)
        {
            user = new User
            {
                Email        = tokenInfo.Email,
                Name         = tokenInfo.Name ?? tokenInfo.Email,
                PasswordHash = "GOOGLE_AUTH",
                HighestLevel = 1,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            };
            context.Users.Add(user);
            context.SaveChanges();
        }

        return Ok(BuildResponse(user));
    }

    // ── HELPER ───────────────────────────────────────────────────────────────

    private AuthResponse BuildResponse(User user) => new()
    {
        Id           = user.Id,
        Email        = user.Email,
        Name         = user.Name,
        HighestLevel = user.HighestLevel,
        Gold         = user.Gold,
        Token        = tokenService.GenerateToken(user)
    };
}
