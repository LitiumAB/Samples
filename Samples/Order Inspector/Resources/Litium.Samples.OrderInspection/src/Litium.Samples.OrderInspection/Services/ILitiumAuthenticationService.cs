namespace Litium.Samples.OrderInspection.Services;

public interface ILitiumAuthenticationService
{
    Task<string> AuthenticateAsync(string username, string password, CancellationToken cancellationToken);
}