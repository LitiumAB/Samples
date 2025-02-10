using Litium.Customers;
using Litium.Runtime.DependencyInjection;
using System.Security.Claims;

namespace Auth0
{
    [Service(ServiceType = typeof(IAuth0LoginService))]
    public interface IAuth0LoginService
    {
        Person GetOrCreatePerson(ClaimsPrincipal identity);
    }
}
