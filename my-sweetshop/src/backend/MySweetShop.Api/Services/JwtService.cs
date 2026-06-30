using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySweetShop.Api.Entities;
using MySweetShop.Api.Options;

namespace MySweetShop.Api.Services;

public class JwtService(IOptions<JwtOptions> opt)
{
    private readonly JwtOptions _opt = opt.Value;

    // Метод создания токена
    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // ID пользователя
            new("uid", user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email), // Email
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opt.ExpiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}