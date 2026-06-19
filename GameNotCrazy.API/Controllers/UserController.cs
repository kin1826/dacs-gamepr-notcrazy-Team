using System.Security.Claims;
using GameNotCrazy.API.Data;
using GameNotCrazy.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameNotCrazy.API.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class UserController(AppDbContext context) : ControllerBase
{
    // POST /api/user/update-progress
    // userId lấy từ JWT token — client không thể giả mạo
    [HttpPost("update-progress")]
    public IActionResult UpdateProgress([FromBody] UpdateProgressRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId))
            return Unauthorized();

        var user = context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return NotFound("User not found.");

        user.HighestLevel = Math.Max(user.HighestLevel, request.NewLevel);
        user.UpdatedAt    = DateTime.UtcNow;
        context.SaveChanges();

        return Ok(new AuthResponse
        {
            Id           = user.Id,
            Email        = user.Email,
            Name         = user.Name,
            HighestLevel = user.HighestLevel,
            Gold         = user.Gold,
            Token        = string.Empty
        });
    }

    // GET /api/user/gold
    [HttpGet("gold")]
    public IActionResult GetGold()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();

        var user = context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound();

        return Ok(new { gold = user.Gold });
    }

    // POST /api/user/set-gold — ghi đè gold tuyệt đối (sync từ local lên)
    [HttpPost("set-gold")]
    public IActionResult SetGold([FromBody] SetGoldRequest req)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId)) return Unauthorized();

        var user = context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound();

        user.Gold      = Math.Max(0, req.Gold);
        user.UpdatedAt = DateTime.UtcNow;
        context.SaveChanges();

        return Ok(new { gold = user.Gold });
    }

    // POST /api/user/update-gold
    [HttpPost("update-gold")]
    public IActionResult UpdateGold([FromBody] UpdateGoldRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId))
            return Unauthorized();

        var user = context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return NotFound("User not found.");

        user.Gold      = Math.Max(0, user.Gold + request.Delta);
        user.UpdatedAt = DateTime.UtcNow;
        context.SaveChanges();

        return Ok(new { gold = user.Gold });
    }
}
