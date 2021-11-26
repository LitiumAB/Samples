using System;
using System.Threading.Tasks;
using IdentityModel;
using Litium.Runtime;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Litium.Accelerator.Auth0
{
    public class Auth0Setup : IApplicationConfiguration
    {
        /// <summary>
        ///     Configure Auth0 authentication according to https://auth0.com/docs/quickstart/webapp/aspnet-core-2/01-login
        /// </summary>
        /// <param name="app">The application builder</param>
        public void Configure(ApplicationConfigurationBuilder app)
        {
            var config = app.Configuration.GetSection("Auth0").Get<Auth0Configuration>();
            if (string.IsNullOrEmpty(config?.ClientId))
            {
                return;
            }

            app.ConfigureServices(services =>
            {
                services.AddSingleton(Options.Create(config));

                services
                    .AddAuthentication()
                    .AddOpenIdConnect(config.ProviderId, options =>
                    {
                        // Set the authority to your Auth0 domain
                        options.Authority = $"https://{config.Domain}";

                        // Configure the Auth0 Client ID and Client Secret
                        options.ClientId = config.ClientId;
                        options.ClientSecret = config.ClientSecret;

                        // Set response type to code
                        options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                        // Configure the scope
                        options.Scope.Clear();
                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("email");

                        options.SignedOutRedirectUri = config.RedirectUri;

                        // Set the callback path, so Auth0 will call back to https://auth0.localtest.me:5001/auth0-callback
                        // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
                        options.CallbackPath = new PathString("/auth0-callback");
                        // Set the callback path, so Auth0 will call back to https://auth0.localtest.me:5001/auth0-signout-callback
                        // Also ensure that you have added the URL as an Allowed Logout URLs in your Auth0 dashboard
                        options.SignedOutCallbackPath = new PathString("/auth0-signout-callback");

                        // Configure the Claims Issuer to be Auth0
                        options.ClaimsIssuer = "Auth0";

                        options.TokenValidationParameters = new()
                        {
                            NameClaimType = JwtClaimTypes.Name
                        };

                        options.Events = new()
                        {
                            OnAuthenticationFailed = OnAuthenticationFailed,
                            OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut,
                            OnSignedOutCallbackRedirect = OnSignedOutCallbackRedirect
                        };
                    });
            });
        }

        private string GetLogoutUri(Auth0Configuration config, string postLogoutUri, HttpRequest request)
        {
            var logoutUri = $"https://{config.Domain}/v2/logout?client_id={config.ClientId}";
            if (string.IsNullOrEmpty(postLogoutUri))
                return logoutUri;

            if (postLogoutUri.StartsWith("/"))
                // transform to absolute:
                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;

            logoutUri += $"&returnTo={Uri.EscapeDataString(postLogoutUri)}";
            return logoutUri;
        }

        private Task OnRedirectToIdentityProviderForSignOut(RedirectContext notification)
        {
            var config = notification.HttpContext.RequestServices.GetRequiredService<IOptions<Auth0Configuration>>().Value;

            notification.Response.Redirect(GetLogoutUri(config, notification.ProtocolMessage.PostLogoutRedirectUri, notification.Request));
            notification.HandleResponse();

            return Task.FromResult(0);
        }

        private Task OnSignedOutCallbackRedirect(RemoteSignOutContext notification)
        {
            var config = notification.HttpContext.RequestServices.GetRequiredService<IOptions<Auth0Configuration>>().Value;

            notification.Response.Redirect(notification.ProtocolMessage.RedirectUri ?? config.RedirectUri ?? "/");
            notification.HandleResponse();

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Handle failed authentication requests by redirecting the user to the home page with an error message in the query string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>From https://docs.microsoft.com/sv-se/azure/active-directory/develop/tutorial-v2-asp-webapp </remarks>
        private Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            var msg = context.Exception?.Data["error_description"] as string;
            if (string.IsNullOrEmpty(msg))
                msg = "Authentication failed";

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Auth0Setup>>();
            logger.LogError(context.Exception, "Auth0Setup.OnAuthenticationFailed, {Message}", msg);

            var config = context.HttpContext.RequestServices.GetRequiredService<IOptions<Auth0Configuration>>().Value;
            var logoutUri = GetLogoutUri(config, $"/?err={Uri.EscapeUriString(msg)}", context.Request);

            context.HandleResponse();
            context.Response.Redirect(logoutUri);
            return Task.FromResult(0);
        }
    }
}
