namespace Litium.Samples.OrderInspection.Services;

public interface ILitiumOrderInspectorClient
{
    Task<LitiumApiResponse> GetOrderAsync(string relativeEndpoint, string accessToken, CancellationToken cancellationToken);
}
