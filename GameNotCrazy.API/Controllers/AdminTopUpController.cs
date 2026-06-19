using GameNotCrazy.API.Data;
using GameNotCrazy.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GameNotCrazy.API.Controllers;

// TODO: thêm xác thực admin sau khi làm web admin
[ApiController]
[Route("api/admin/topup")]
public class AdminTopUpController(AppDbContext context) : ControllerBase
{
    // GET /api/admin/topup/list
    [HttpGet("list")]
    public IActionResult List()
    {
        var list = context.PaymentRequests
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id, p.Code, p.UserId, p.GoldAmount,
                p.Price, p.Status, p.CreatedAt, p.ApprovedAt, p.Note
            })
            .ToList();

        return Ok(list);
    }

    // POST /api/admin/topup/approve/{code}
    [HttpPost("approve/{code}")]
    public IActionResult Approve(string code)
    {
        var payment = context.PaymentRequests.FirstOrDefault(p => p.Code == code);
        if (payment == null)          return NotFound("Không tìm thấy yêu cầu.");
        if (payment.Status != "pending") return BadRequest("Yêu cầu đã được xử lý.");

        payment.Status     = "approved";
        payment.ApprovedAt = DateTime.UtcNow;

        var user = context.Users.FirstOrDefault(u => u.Id == payment.UserId);
        if (user != null) user.Gold += payment.GoldAmount;

        context.SaveChanges();
        return Ok(new { message = $"Đã duyệt. Cộng {payment.GoldAmount} xu cho user {payment.UserId}." });
    }

    // POST /api/admin/topup/reject/{code}
    [HttpPost("reject/{code}")]
    public IActionResult Reject(string code, [FromBody] RejectTopUpRequest? request)
    {
        var payment = context.PaymentRequests.FirstOrDefault(p => p.Code == code);
        if (payment == null)             return NotFound("Không tìm thấy yêu cầu.");
        if (payment.Status != "pending") return BadRequest("Yêu cầu đã được xử lý.");

        payment.Status     = "rejected";
        payment.ApprovedAt = DateTime.UtcNow;
        payment.Note       = request?.Note;

        context.SaveChanges();
        return Ok(new { message = "Đã từ chối yêu cầu." });
    }
}
