namespace Litium.Samples.OrderInspection.Services;

public interface ILitiumAuthenticationService
{
    Task<string> AuthenticateAsync(CancellationToken cancellationToken);
}