using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Litium.Accelerator.Routing;
using Litium.ComponentModel;
using Litium.Configuration;
using Litium.Security;
using Litium.Web;
using Litium.Web.Routing;
using Litium.Web.Security.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using AuthenticationService = Litium.Security.AuthenticationService;

namespace Litium.Accelerator.Auth0
{
    [Route("auth0-authentication")]
    public class Auth0LoginController : Controller
    {
        private readonly ILogger<Auth0LoginController> _logger;
        private readonly AuthenticationService _authenticationService;
        private readonly IOptions<Auth0Configuration> _configuration;
        private readonly IAuth0LoginService _loginService;
        private readonly RequestModelAccessor _requestModelAccessor;
        private readonly UrlService _urlService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<AdministrationSecurityConfig> _administrationSecurityConfig;
        private readonly SecurityContextService _securityContextService;
        private readonly IChannelResolver _channelResolver;

        public Auth0LoginController(
            ILogger<Auth0LoginController> logger,
            AuthenticationService authenticationService,
            IAuth0LoginService auth0LoginService,
            IOptions<Auth0Configuration> auth0Configuration,
            RequestModelAccessor requestModelAccessor,
            UrlService urlService,
            UserManager<ApplicationUser> userManager,
            IOptions<AdministrationSecurityConfig> administrationSecurityConfig,
            SecurityContextService securityContextService,
            IChannelResolver channelResolver)
        {
            _logger = logger;
            _authenticationService = authenticationService;
            _loginService = auth0LoginService;
            _configuration = auth0Configuration;
            _requestModelAccessor = requestModelAccessor;
            _urlService = urlService;
            _userManager = userManager;
            _administrationSecurityConfig = administrationSecurityConfig;
            _securityContextService = securityContextService;
            _channelResolver = channelResolver;
        }

        private string GetChannelRootPageUrl()
        {
            var channel = _requestModelAccessor?.RequestModel?.ChannelModel?.Channel;
            if (channel is null)
            {
                if (_channelResolver.TryGet(out var info))
                {
                    channel = info.Channel;
                }
                else
                {
                    return "/";
                }
            }

            return _urlService.GetUrl(channel, new ChannelUrlArgs
            {
                AbsoluteUrl = true
            });
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public async Task<ActionResult> RedirectToProvider(string redirectUrl)
        {
            var cfg = _configuration.Value;
            if (string.IsNullOrEmpty(cfg?.ClientId))
            {
                throw new Exception("Auth0 is not configured.");
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(ProviderLogin), new { redirectUrl })
            };

            await HttpContext.ChallengeAsync(cfg.ProviderId, properties);
            return Content("OK");
        }

        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<ActionResult> ProviderLogin(string redirectUrl)
        {
            try
            {
                var authenticate = await HttpContext.AuthenticateAsync(_configuration.Value.ProviderId);
                if (!authenticate.Succeeded)
                {
                    throw new Exception("User is not authenticated");
                }

                var identity = authenticate.Principal;
                var id = identity.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(id))
                    throw new Exception($"Could not get required claim '{ClaimTypes.NameIdentifier}' from identity");

                var login = await _userManager.FindByLoginAsync(_configuration.Value.ProviderId, id);
                if (login is null)
                {
                    var person = _loginService.GetOrCreatePerson(identity);
                    login = await _userManager.FindByIdAsync(person.SystemId.ToString());
                    var userLogin = new UserLoginInfo(_configuration.Value.ProviderId, id, _configuration.Value.ProviderId);
                    var result = await _userManager.AddLoginAsync(login, userLogin);
                    if (!result.Succeeded)
                        throw new Exception($"Person {person.Id} ({person.SystemId}) could not connect with Auth0 external login: {string.Join(", ", result.Errors)}");
                }

                var authenticationResult = _authenticationService.ExternalSignIn(_configuration.Value.ProviderId, id);
                if (authenticationResult != AuthenticationResult.Success)
                {
                    throw new Exception($"Authentication failed for '{id}' with provider '{_configuration.Value.ProviderId}' : '{authenticationResult}'");
                }

                // If we have configure to only allow identities with specific claim, add the required claims to the identity
                if (_administrationSecurityConfig.Value.RequiredClaims?.Count > 0)
                {
                    var claimIdentity = _securityContextService.CreateClaimsIdentity(login.UserName, login.Id);
                    foreach (var item in _administrationSecurityConfig.Value.RequiredClaims)
                    {
                        claimIdentity.AddClaim(new Claim(item.Key, item.Value));
                    }
                    _securityContextService.ActAs(claimIdentity);
                    _authenticationService.RefreshSignIn();
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to sign in with Auth0");
                // Redirect to root on error to prevent redirect loop
                return Redirect(GetChannelRootPageUrl());
            }

            return Redirect(redirectUrl.NullIfEmpty() ?? GetChannelRootPageUrl());
        }

        [HttpGet("logout")]
        public async Task<ActionResult> Logout(string redirectUrl = "")
        {
            if (string.IsNullOrWhiteSpace(redirectUrl))
                redirectUrl = GetChannelRootPageUrl();

            _authenticationService.SignOut();

            await HttpContext.SignOutAsync(_configuration.Value.ProviderId, new()
            {
                RedirectUri = redirectUrl,
            });
            return Content("OK");
        }
    }
}
