using System;
using System.Threading.Tasks;
using Litium.Owin.Lifecycle;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

namespace Litium.Accelerator.Auth0
{
    public class Auth0Setup : IOwinStartupConfiguration
    {
        private readonly IAuth0Configuration _configuration;

        public Auth0Setup(IAuth0Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Configure Auth0 authentication according to https://auth0.com/docs/quickstart/webapp/aspnet-owin/01-login
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
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
                    RedirectToIdentityProvider = notification =>
                    {
                        if (notification.ProtocolMessage.RequestType == OpenIdConnectRequestType.Logout)
                        {
                            var logoutUri = $"https://{_configuration.Domain}/v2/logout?client_id={_configuration.ClientId}";
                            var postLogoutUri = notification.ProtocolMessage.PostLogoutRedirectUri;
                            if (!string.IsNullOrEmpty(postLogoutUri))
                            {
                                if (postLogoutUri.StartsWith("/"))
                                {
                                    // transform to absolute
                                    var request = notification.Request;
                                    postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                                }

                                logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
                            }

                            notification.Response.Redirect(logoutUri);
                            notification.HandleResponse();
                        }

                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}