using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChangeManagement.Api.Tests.Infrastructure;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string UserIdHeader = "X-Test-UserId";
    public const string UserNameHeader = "X-Test-Name";
    public const string RoleHeader = "X-Test-Role";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.FirstOrDefault();
        var hasTestHeaders = Request.Headers.ContainsKey(UserIdHeader)
            || Request.Headers.ContainsKey(UserNameHeader)
            || Request.Headers.ContainsKey(RoleHeader);

        if (!hasTestHeaders && (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith(SchemeName, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userIdRaw = Request.Headers[UserIdHeader].FirstOrDefault();
        var userName = Request.Headers[UserNameHeader].FirstOrDefault() ?? "test-user";
        var role = Request.Headers[RoleHeader].FirstOrDefault() ?? "Admin";

        var userId = Guid.TryParse(userIdRaw, out var parsed)
            ? parsed
            : Guid.Parse("11111111-1111-1111-1111-111111111111");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
