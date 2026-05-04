using System.Net.Http.Headers;
using Litium.Samples.OrderInspection.Configuration;
using Microsoft.Extensions.Options;

namespace Litium.Samples.OrderInspection.Services;

public sealed class LitiumOrderInspectorClient(
    HttpClient httpClient,
    IOptions<LitiumAdminApiOptions> options) : ILitiumOrderInspectorClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly LitiumAdminApiOptions _options = options.Value;

    public async Task<LitiumApiResponse> GetOrderAsync(string relativeEndpoint, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildAbsoluteUri(relativeEndpoint));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        return new LitiumApiResponse
        {
            StatusCode = (int)response.StatusCode,
            ContentType = response.Content.Headers.ContentType?.MediaType,
            Body = payload
        };
    }

    private Uri BuildAbsoluteUri(string endpoint)
    {
        var baseUri = new Uri(_options.BaseUrl, UriKind.Absolute);
        return new Uri(baseUri, endpoint);
    }
}
