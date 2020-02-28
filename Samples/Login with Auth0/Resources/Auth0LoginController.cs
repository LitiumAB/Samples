using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Litium.Accelerator.Auth0;
using Litium.Accelerator.Routing;
using Litium.ComponentModel;
using Litium.Security;
using Litium.Web;
using Litium.Web.Security.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Litium.Accelerator.Mvc.Controllers.Login
{
    public class Auth0LoginController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly IAuth0Configuration _configuration;
        private readonly IAuth0LoginService _loginService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly UrlService _urlService;

        public Auth0LoginController(AuthenticationService authenticationService,
            IAuth0LoginService auth0LoginService, IAuth0Configuration auth0Configuration,
            RequestModelAccessor requestModelAccessor, UrlService urlService)
        {
            _authenticationService = authenticationService;
            _loginService = auth0LoginService;
            _configuration = auth0Configuration;
            _requestModelAccessor = requestModelAccessor;
            _urlService = urlService;
        }

        private string GetChannelRootPageUrl()
        {
            return _urlService.GetUrl(_requestModelAccessor?.RequestModel?.ChannelModel?.Channel, new ChannelUrlArgs
            {
                AbsoluteUrl = true
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RedirectToProvider(string redirectUrl)
        {
            var owinContext = HttpContext.GetOwinContext();
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(ProviderLogin), new {redirectUrl})
            };

            HttpContext.SkipAuthorization = true;
            owinContext.Authentication.Challenge(properties, _configuration.ProviderId);
            return Content("OK");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ProviderLogin(string redirectUrl)
        {
            try
            {
                var owinContext = HttpContext.GetOwinContext();
                var userManager = owinContext.Get<ApplicationUserManager>();
                var authenticationManager = owinContext.Authentication;
                var externalLoginInfo = authenticationManager.GetExternalLoginInfo();
                var identity = externalLoginInfo.ExternalIdentity;

                if (!identity.IsAuthenticated)
                    throw new Exception("User is not authenticated");

                var id = identity.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(id))
                    throw new Exception($"Could not get required claim '{ClaimTypes.NameIdentifier}' from identity");

                var person = _loginService.GetOrCreatePerson(identity);

                var loginExists = (await userManager.GetLoginsAsync(person.SystemId)).Any(login => login.LoginProvider.Equals(_configuration.ProviderId));
                if (!loginExists)
                {
                    var userLogin = new UserLoginInfo(_configuration.ProviderId, id);
                    var result = await userManager.AddLoginAsync(person.SystemId, userLogin);
                    if (!result.Succeeded)
                        throw new Exception($"Person {person.Id} ({person.SystemId}) could not connect with Auth0 external login: {string.Join(", ", result.Errors)}");
                }

                var authenticationResult = _authenticationService.ExternalSignIn(_configuration.ProviderId, id);
                if (authenticationResult != AuthenticationResult.Success)
                {
                    throw new Exception($"Authentication failed for '{id}' with provider '{_configuration.ProviderId}' : '{authenticationResult}'");
                }
            }
            catch (Exception exception)
            {
                this.Log().Error("Failed to sign in with Auth0", exception);
                // Redirect to root on error to prevent redirect loop
                return Redirect(GetChannelRootPageUrl());
            }

            return Redirect(redirectUrl.NullIfEmpty() ?? GetChannelRootPageUrl());
        }

        [HttpGet]
        public RedirectResult Logout(string redirectUrl = "")
        {
            if (string.IsNullOrWhiteSpace(redirectUrl))
                redirectUrl = GetChannelRootPageUrl();

            _authenticationService.SignOut();
            return new RedirectResult(redirectUrl);
        }
    }
}