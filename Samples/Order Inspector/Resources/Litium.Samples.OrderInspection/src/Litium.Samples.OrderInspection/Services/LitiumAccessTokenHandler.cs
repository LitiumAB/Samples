using System.Net.Http.Headers;

namespace Litium.Samples.OrderInspection.Services;

public sealed class LitiumAccessTokenHandler(ILitiumAuthenticationService litiumAuthenticationService) : DelegatingHandler
{
    private readonly ILitiumAuthenticationService _litiumAuthenticationService = litiumAuthenticationService;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _litiumAuthenticationService.AuthenticateAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}