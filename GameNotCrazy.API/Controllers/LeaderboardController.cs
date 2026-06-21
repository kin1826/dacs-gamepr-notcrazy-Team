using GameNotCrazy.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace GameNotCrazy.API.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController(AppDbContext context) : ControllerBase
{
    // GET /api/leaderboard
    [HttpGet]
    public IActionResult Get([FromQuery] int top = 20)
    {
        var list = context.Users
            .OrderByDescending(u => u.HighestLevel)
            .ThenBy(u => u.UpdatedAt)
            .Take(top)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.HighestLevel
            })
            .ToList();

        return Ok(list);
    }
}
