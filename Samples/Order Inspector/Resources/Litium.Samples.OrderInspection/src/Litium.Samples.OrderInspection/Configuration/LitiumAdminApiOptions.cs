using System.ComponentModel.DataAnnotations;

namespace Litium.Samples.OrderInspection.Configuration;

public class LitiumAdminApiOptions
{
    public const string SectionName = "LitiumAdminApi";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    public string TokenEndpoint { get; set; } = "/connect/token";

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? Scope { get; set; }
}
