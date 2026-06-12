using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameNotCrazy.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace GameNotCrazy.API.Services;

public class TokenService(IConfiguration config)
{
    public string GenerateToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,           user.Email),
            new Claim(ClaimTypes.Name,            user.Name),
        };

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddDays(double.Parse(config["Jwt:ExpiryDays"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
