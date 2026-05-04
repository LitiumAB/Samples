using System.Text.Json;
using Litium.Samples.OrderInspection.Configuration;
using Microsoft.Extensions.Options;

namespace Litium.Samples.OrderInspection.Services;

public sealed class LitiumAuthenticationService(
    HttpClient httpClient,
    IOptions<LitiumAdminApiOptions> options,
    ILogger<LitiumAuthenticationService> logger) : ILitiumAuthenticationService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly LitiumAdminApiOptions _options = options.Value;
    private readonly ILogger<LitiumAuthenticationService> _logger = logger;

    public async Task<string> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildTokenUri())
        {
            Content = new FormUrlEncodedContent(BuildTokenRequestValues(username, password))
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Litium token request failed with status {StatusCode}.", response.StatusCode);
            throw new InvalidOperationException($"Failed to authenticate against Litium. Status: {(int)response.StatusCode}.");
        }

        using var json = JsonDocument.Parse(payload);
        if (!json.RootElement.TryGetProperty("access_token", out var tokenElement))
        {
            throw new InvalidOperationException("Litium token response did not contain 'access_token'.");
        }

        var token = tokenElement.GetString();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Litium token response contained an empty 'access_token'.");
        }

        return token;
    }

    private Dictionary<string, string> BuildTokenRequestValues(string username, string password)
    {
        var values = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = username,
            ["password"] = password
        };

        if (!string.IsNullOrWhiteSpace(_options.ClientId))
        {
            values["client_id"] = _options.ClientId;
        }

        if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            values["client_secret"] = _options.ClientSecret;
        }

        if (!string.IsNullOrWhiteSpace(_options.Scope))
        {
            values["scope"] = _options.Scope;
        }

        return values;
    }

    private Uri BuildTokenUri()
    {
        var baseUri = new Uri(_options.BaseUrl, UriKind.Absolute);
        return new Uri(baseUri, _options.TokenEndpoint);
    }
}