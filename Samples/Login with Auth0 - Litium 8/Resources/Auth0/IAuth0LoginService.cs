using System.Security.Claims;
using System.Threading.Tasks;
using Litium.Customers;
using Litium.Runtime.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Litium.Accelerator.Auth0
{
    [Service(ServiceType = typeof(IAuth0LoginService))]
    public interface IAuth0LoginService
    {
        Person GetOrCreatePerson(ClaimsPrincipal identity);
    }
}
