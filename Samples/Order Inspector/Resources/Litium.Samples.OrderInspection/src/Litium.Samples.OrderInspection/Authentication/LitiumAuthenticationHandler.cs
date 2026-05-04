using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Litium.Samples.OrderInspection.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Litium.Samples.OrderInspection.Authentication;

public sealed class LitiumAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ILitiumAuthenticationService litiumAuthenticationService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly ILitiumAuthenticationService _litiumAuthenticationService = litiumAuthenticationService;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeaders))
        {
            return AuthenticateResult.NoResult();
        }

        var authorization = authorizationHeaders.ToString();
        if (!authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Authorization header must use Basic scheme.");
        }

        var encodedCredentials = authorization["Basic ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(encodedCredentials))
        {
            return AuthenticateResult.Fail("Missing Basic credentials.");
        }

        string credentials;
        try
        {
            credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail("Invalid Basic credential encoding.");
        }

        var separatorIndex = credentials.IndexOf(':');
        if (separatorIndex <= 0)
        {
            return AuthenticateResult.Fail("Invalid Basic credential format.");
        }

        var username = credentials[..separatorIndex];
        var password = credentials[(separatorIndex + 1)..];

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return AuthenticateResult.Fail("Username and password are required.");
        }

        string accessToken;
        try
        {
            accessToken = await _litiumAuthenticationService.AuthenticateAsync(username, password, Context.RequestAborted);
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail($"Litium authentication failed: {ex.Message}");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new(LitiumAuthenticationDefaults.AccessTokenClaimType, accessToken)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
}