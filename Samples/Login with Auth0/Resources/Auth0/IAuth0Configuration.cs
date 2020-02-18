using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Auth0
{
    [Service(ServiceType = typeof(IAuth0Configuration))]
    public interface IAuth0Configuration
    {
        string Domain { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string RedirectUri { get; }
        string PostLogoutRedirectUri { get; }
        string PersonTemplate { get; }
        string ProviderId { get; }
    }
}