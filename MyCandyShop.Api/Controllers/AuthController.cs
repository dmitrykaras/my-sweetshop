using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Contracts;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Entities;
using MyCandyShop.Api.Services;
namespace MyCandyShop.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) => _db = db;

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromBody] AuthRequestCodeRequest request)
    {
        var email = request.Email.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return BadRequest("Invalid email");

        // cooldown: 2 минуты
        var last = await _db.EmailVerificationCodes
            .Where(x => x.Email == email)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (last != null)
        {
            var seconds = (int)(DateTimeOffset.UtcNow - last.CreatedAt).TotalSeconds;
            if (seconds < 120)
                return BadRequest($"Wait {120 - seconds} seconds");
        }

        var code = CodeGenerator.Generate4Digits();
        var hash = HashService.Sha256($"{email}:{code}");

        var entity = new EmailVerificationCode
        {
            Id = Guid.NewGuid(),
            Email = email,
            CodeHash = hash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Attempts = 0,
            IsUsed = false
        };

        _db.EmailVerificationCodes.Add(entity);
        await _db.SaveChangesAsync();

        // Временно: выводим код в консоль (потом заменим на отправку email)
        Console.WriteLine($"[AUTH CODE] {email} => {code}");

        return Ok(new { cooldownSeconds = 120 });
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] AuthVerifyCodeRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return BadRequest("Invalid email");

        if (code.Length != 4 || !code.All(char.IsDigit))
            return BadRequest("Invalid code");

        var entity = await _db.EmailVerificationCodes
            .Where(x => x.Email == email)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (entity == null)
            return BadRequest("Code not found");

        if (entity.IsUsed)
            return BadRequest("Code already used");

        if (DateTimeOffset.UtcNow > entity.ExpiresAt)
            return BadRequest("Code expired");

        if (entity.Attempts >= 5)
            return BadRequest("Too many attempts");

        entity.Attempts++;

        var inputHash = HashService.Sha256($"{email}:{code}");

        if (entity.CodeHash != inputHash)
        {
            await _db.SaveChangesAsync();
            return BadRequest("Invalid code");
        }

        entity.IsUsed = true;
        await _db.SaveChangesAsync();

        // TODO: тут позже будет JWT
        return Ok(new { success = true });
    }
}