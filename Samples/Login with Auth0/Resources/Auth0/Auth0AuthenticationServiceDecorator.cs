using System.Web;
using JetBrains.Annotations;
using Litium.Runtime.DependencyInjection;
using Litium.Security;

namespace Litium.Accelerator.Auth0
{
    /// <summary>
    /// Service decorator for AuthenticationService to intercept any signout from
    /// Litium to also sign out of Auth0.
    /// </summary>
    /// <remarks>
    /// Note that Litium platform has another decorator on the service:
    /// Litium.Web.Security.AuthenticationServiceDecorator
    /// </remarks>
    [ServiceDecorator(serviceType: typeof(AuthenticationService))]
    public class Auth0AuthenticationServiceDecorator : AuthenticationService
    {
        private readonly AuthenticationService _parent;
        private readonly IAuth0LoginService _loginService;

        public Auth0AuthenticationServiceDecorator(AuthenticationService parent, IAuth0LoginService loginService)
        {
            _parent = parent;
            _loginService = loginService;
        }

        public override AuthenticationResult ExternalSignIn([NotNull] string loginProvider, [NotNull] string providerKey)
        {
            return _parent.ExternalSignIn(loginProvider, providerKey);
        }

        public override AuthenticationResult PasswordSignIn([NotNull] string username, [NotNull] string password, string newPassword)
        {
            return _parent.PasswordSignIn(username, password, newPassword);
        }

        public override void RefreshSignIn()
        {
            _parent.RefreshSignIn();
        }

        public override void SignOut()
        {
            _parent.SignOut();

            // Execute the Auth0-signout after the Litium-signout since it does a redirect after logout
            var owinContext = HttpContext.Current?.GetOwinContext();
            if (owinContext is object) // (not null)
                _loginService.SignOut(owinContext);
        }
    }
}