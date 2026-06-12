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
            Token        = string.Empty  // không cấp token mới khi update progress
        });
    }
}
