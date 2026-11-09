using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySweetshop.Api.Contracts;
using MySweetshop.Api.Entities;
using MySweetShop.Api.Contracts;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using MySweetShop.Api.Options;
using MySweetShop.Api.Services;
using System.Security.Claims;

namespace MySweetShop.Api.Controllers;

[ApiController]
[Route("api/qr")]
[Authorize]
public class QrController(AppDbContext db, IOptions<LoyaltyOptions> options) : ControllerBase
{
    private readonly LoyaltyOptions _o = options.Value;

    // Клиент открыл главный экран -> получает свежий токен на 120 секунд
    [HttpPost("generate")]
    [EnableRateLimiting("qr")]
    public async Task<IActionResult> Generate()
    {
        if (!Guid.TryParse(User.FindFirstValue("uid"), out var userId))
            return Unauthorized();

        var now = DateTimeOffset.UtcNow;

        // Гасим прежние неиспользованные QR этого клиента: активен всегда только один
        await db.UserQrTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ExpiresAt, now));

        var token = QrTokenHelper.NewToken();
        var entity = new UserQrToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = QrTokenHelper.Hash(token),
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(_o.QrLifetimeSeconds)
        };

        db.UserQrTokens.Add(entity);
        await db.SaveChangesAsync();

        return Ok(new QrTokenResponse(token, entity.ExpiresAt));
    }
}
