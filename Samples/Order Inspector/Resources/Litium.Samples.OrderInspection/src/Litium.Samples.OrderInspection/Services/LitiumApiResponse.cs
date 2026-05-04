namespace Litium.Samples.OrderInspection.Services;

public sealed class LitiumApiResponse
{
    public required int StatusCode { get; init; }

    public required string? ContentType { get; init; }

    public required string Body { get; init; }
}
