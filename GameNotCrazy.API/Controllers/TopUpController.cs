using System.Security.Claims;
using GameNotCrazy.API.Data;
using GameNotCrazy.API.DTOs;
using GameNotCrazy.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameNotCrazy.API.Controllers;

[ApiController]
[Route("api/topup")]
[Authorize]
public class TopUpController(AppDbContext context) : ControllerBase
{
    private static readonly Dictionary<int, int> PriceMap = new()
    {
        { 10,  29999   },
        { 100, 279999  },
        { 500, 1399999 }
    };

    // POST /api/topup/create
    [HttpPost("create")]
    public IActionResult Create([FromBody] CreateTopUpRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdStr, out long userId))
            return Unauthorized();

        if (!PriceMap.TryGetValue(request.GoldAmount, out int price))
            return BadRequest("Gói không hợp lệ.");

        string code = GenerateUniqueCode();

        context.PaymentRequests.Add(new PaymentRequest
        {
            UserId     = userId,
            Code       = code,
            GoldAmount = request.GoldAmount,
            Price      = price,
            Status     = "pending",
            CreatedAt  = DateTime.UtcNow
        });
        context.SaveChanges();

        return Ok(new TopUpResponse { Code = code, GoldAmount = request.GoldAmount, Price = price });
    }

    private string GenerateUniqueCode()
    {
        var rng = new Random();
        string code;
        do { code = "NC" + rng.Next(1000, 9999); }
        while (context.PaymentRequests.Any(p => p.Code == code && p.Status == "pending"));
        return code;
    }
}
