using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
[Route("api/cashier")]
[Authorize]
public class CashierController(AppDbContext db, IOptions<LoyaltyOptions> options) : ControllerBase
{
    private readonly LoyaltyOptions _o = options.Value;

    private const string SessionGone = "Сессия истекла или уже завершена. Отсканируйте QR-код заново";

    // Роль проверяем по БД на каэжый запрос, а не по JWT (отзыв прав кассира действует сразу)
    private async Task<Guid?> GetCashierIdAsync()
    {
        if (!Guid.TryParse(User.FindFirstValue("uid"), out var id)) return null;
        return await db.Users.AnyAsync(u => u.Id == id && u.IsCashier) ? id : null;
    }

    private IActionResult NotCashier() =>
        StatusCode(StatusCodes.Status403Forbidden, new { message = "Нет прав кассира" });

    private IActionResult Fail(string message) => BadRequest(new { message });

    private bool IsValidAmount(decimal a) =>
        a > 0 && a <= _o.MaxOrderAmount && decimal.Round(a, 2) == a;

    private IQueryable<UserQrToken> ActiveSession(Guid sessionId, Guid cashierId, DateTimeOffset now) =>
        db.UserQrTokens.Where(t =>
            t.Id == sessionId &&
            t.ScannedByCashierId == cashierId &&
            t.CompletedAt == null &&
            t.SessionExpiresAt > now);

    // Кассир сканирует QR
    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] ScanRequest req)
    {
        var cashierId = await GetCashierIdAsync();
        if (cashierId is null) return NotCashier();

        if (string.IsNullOrWhiteSpace(req.Token) || req.Token.Length > 128)
            return Fail("Некорректный QR-код");

        var hash = QrTokenHelper.Hash(req.Token.Trim());
        var now = DateTimeOffset.UtcNow;
        var sessionEnd = now.AddMinutes(_o.CashierSessionMinutes);
        var cashier = cashierId.Value;

        // Атомарно "сжигаем" токен (два кассира одновременно не смогут отсканировать один QR)
        // Кассир не может начислять баллы сам себе (t.UserId != cashier).
        var affected = await db.UserQrTokens
            .Where(t => t.TokenHash == hash && !t.IsUsed && t.ExpiresAt > now && t.UserId != cashier)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.IsUsed, true)
                .SetProperty(t => t.UsedAt, (DateTimeOffset?)now)
                .SetProperty(t => t.ScannedByCashierId, (Guid?)cashier)
                .SetProperty(t => t.SessionExpiresAt, (DateTimeOffset?)sessionEnd));

        if (affected == 0)
            return Fail("QR-код недействителен, просрочен или уже использован");

        var session = await db.UserQrTokens
            .Where(t => t.TokenHash == hash)
            .Select(t => new { t.Id, t.UserId })
            .FirstAsync();

        var user = await db.Users
            .Where(u => u.Id == session.UserId)
            .Select(u => new { u.FirstName, u.Points })
            .FirstAsync();

        return Ok(new ScanResponse(
            session.Id,
            string.IsNullOrWhiteSpace(user.FirstName) ? "Клиент" : user.FirstName!,
            user.Points,
            _o.RubPerPoint,
            _o.MaxRedeemShare,
            sessionEnd));
    }

    // Копим: сумма заказа в баллы клиенту
    [HttpPost("sessions/{sessionId:guid}/accrue")]
    public async Task<IActionResult> Accrue(Guid sessionId, [FromBody] AccrueRequest req)
    {
        var cashierId = await GetCashierIdAsync();
        if (cashierId is null) return NotCashier();

        if (!IsValidAmount(req.OrderAmount))
            return Fail("Некорректная сумма заказа");

        var earned = (int)Math.Floor(req.OrderAmount * _o.AccrualRate);
        if (earned <= 0)
            return Fail("Сумма слишком мала для начисления баллов");

        var now = DateTimeOffset.UtcNow;
        await using var tx = await db.Database.BeginTransactionAsync();

        var session = await ActiveSession(sessionId, cashierId.Value, now)
            .Select(t => new { t.UserId }).FirstOrDefaultAsync();
        if (session is null) return Fail(SessionGone);

        // Закрываем сессию атомарно: повторный тап/запрос не начислит дважды
        var closed = await ActiveSession(sessionId, cashierId.Value, now)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.CompletedAt, (DateTimeOffset?)now));
        if (closed == 0) return Fail(SessionGone);

        await db.Users.Where(u => u.Id == session.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Points, u => u.Points + earned));

        db.PointsTransactions.Add(new PointsTransaction
        {
            Id = Guid.NewGuid(),
            UserId = session.UserId,
            CashierId = cashierId.Value,
            QrTokenId = sessionId,
            Type = PointsTransactionType.Accrual,
            OrderAmount = req.OrderAmount,
            Points = earned,
            CreatedAt = now
        });
        await db.SaveChangesAsync();

        var balance = await db.Users.Where(u => u.Id == session.UserId)
            .Select(u => u.Points).FirstAsync();

        await tx.CommitAsync();
        return Ok(new AccrueResponse(earned, balance));
    }

    // Списываем: баллы в скидку, сервер возвращает итоговую сумму для терминала
    [HttpPost("sessions/{sessionId:guid}/redeem")]
    public async Task<IActionResult> Redeem(Guid sessionId, [FromBody] RedeemRequest req)
    {
        var cashierId = await GetCashierIdAsync();
        if (cashierId is null) return NotCashier();

        if (!IsValidAmount(req.OrderAmount))
            return Fail("Некорректная сумма заказа");

        if (req.PointsToRedeem <= 0)
            return Fail("Укажите, сколько баллов списать");

        var maxByShare = (int)Math.Floor(req.OrderAmount * _o.MaxRedeemShare / _o.RubPerPoint);
        if (req.PointsToRedeem > maxByShare)
            return Fail($"Можно списать не более {maxByShare} баллов ({(int)(_o.MaxRedeemShare * 100)}% от суммы заказа)");

        var now = DateTimeOffset.UtcNow;
        var points = req.PointsToRedeem;
        await using var tx = await db.Database.BeginTransactionAsync();

        var session = await ActiveSession(sessionId, cashierId.Value, now)
            .Select(t => new { t.UserId }).FirstOrDefaultAsync();
        if (session is null) return Fail(SessionGone);

        var closed = await ActiveSession(sessionId, cashierId.Value, now)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.CompletedAt, (DateTimeOffset?)now));
        if (closed == 0) return Fail(SessionGone);

        // Условный UPDATE: баланс не уйдёт в минус даже при гонке запросов
        var debited = await db.Users
            .Where(u => u.Id == session.UserId && u.Points >= points)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Points, u => u.Points - points));
        if (debited == 0) return Fail("У клиента недостаточно баллов");

        db.PointsTransactions.Add(new PointsTransaction
        {
            Id = Guid.NewGuid(),
            UserId = session.UserId,
            CashierId = cashierId.Value,
            QrTokenId = sessionId,
            Type = PointsTransactionType.Redemption,
            OrderAmount = req.OrderAmount,
            Points = -points,
            CreatedAt = now
        });
        await db.SaveChangesAsync();

        var balance = await db.Users.Where(u => u.Id == session.UserId)
            .Select(u => u.Points).FirstAsync();

        await tx.CommitAsync();

        var finalAmount = decimal.Round(req.OrderAmount - points * _o.RubPerPoint, 2);
        return Ok(new RedeemResponse(finalAmount, points, balance));
    }
}