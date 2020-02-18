using System.Security.Claims;
using Litium.Customers;
using Litium.Runtime.DependencyInjection;
using Microsoft.Owin;

namespace Litium.Accelerator.Auth0
{
    [Service(ServiceType = typeof(IAuth0LoginService))]
    public interface IAuth0LoginService
    {
        Person GetOrCreatePerson(ClaimsIdentity identity);
        void SignOut(IOwinContext owinContext);
    }
}