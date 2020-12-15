using System;
using System.Threading.Tasks;
using Litium.Owin.Lifecycle;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace Litium.Accelerator.Auth0
{
    public class Auth0Setup : IOwinStartupStageConfiguration
    {
        private readonly IAuth0Configuration _configuration;
        private readonly ILogger<Auth0Setup> _logger;

        public Auth0Setup(IAuth0Configuration configuration, ILogger<Auth0Setup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public PipelineStage PipelineStage => PipelineStage.PostAuthorize;

        /// <summary>
        ///     Configure Auth0 authentication according to https://auth0.com/docs/quickstart/webapp/aspnet-owin/01-login
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.UseExternalSignInCookie();

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                // Passive = Do not automatically log in with the OpenId cookie
                // Login is done manually in Auth0LoginController
                AuthenticationMode = AuthenticationMode.Passive,
                AuthenticationType = _configuration.ProviderId,
                Authority = $"https://{_configuration.Domain}",
                ClientId = _configuration.ClientId,
                ClientSecret = _configuration.ClientSecret,
                RedirectUri = _configuration.RedirectUri,
                PostLogoutRedirectUri = _configuration.PostLogoutRedirectUri,
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                Scope = "openid profile email",
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name"
                },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = OnAuthenticationFailed,
                    RedirectToIdentityProvider = OnRedirectToIdentityProvider
                }
            });
        }

        private string GetLogoutUri(string postLogoutUri, IOwinRequest request)
        {
            var logoutUri = $"https://{_configuration.Domain}/v2/logout?client_id={_configuration.ClientId}";
            if (string.IsNullOrEmpty(postLogoutUri))
                return logoutUri;

            if (postLogoutUri.StartsWith("/"))
                // transform to absolute:
                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;

            logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
            return logoutUri;
        }

        private Task OnRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            if (notification.ProtocolMessage.RequestType != OpenIdConnectRequestType.Logout)
                return Task.FromResult(0);

            notification.Response.Redirect(GetLogoutUri(notification.ProtocolMessage.PostLogoutRedirectUri, notification.Request));
            notification.HandleResponse();

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Handle failed authentication requests by redirecting the user to the home page with an error message in the query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>From https://docs.microsoft.com/sv-se/azure/active-directory/develop/tutorial-v2-asp-webapp </remarks>
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> context)
        {
            var msg = context.Exception?.Data["error_description"] as string;
            if (string.IsNullOrEmpty(msg))
                msg = "Authentication failed";

            _logger.LogError(context.Exception, "Auth0Setup.OnAuthenticationFailed", msg);
            var logoutUri = GetLogoutUri($"/?err={Uri.EscapeUriString(msg)}", context.Request);

            context.HandleResponse();
            context.Response.Redirect(logoutUri);
            return Task.FromResult(0);
        }
    }
}