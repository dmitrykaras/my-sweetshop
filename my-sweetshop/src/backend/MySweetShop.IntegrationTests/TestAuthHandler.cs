using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Если заголовок X-Test-UserId не передан — считаем пользователя неавторизованным
        if (!Context.Request.Headers.TryGetValue("X-Test-UserId", out var userIdHeader)
            || string.IsNullOrEmpty(userIdHeader))
        {
            return Task.FromResult(AuthenticateResult.Fail("No test user ID header provided"));
        }

        var userId = userIdHeader.ToString();

        var claims = new[]
        {
            // Именно это значение вытягивается через User.FindFirstValue(ClaimTypes.NameIdentifier)
            new Claim("uid", userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "TestUser")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}