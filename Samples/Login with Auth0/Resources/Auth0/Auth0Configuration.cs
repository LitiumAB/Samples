using System;
using System.Configuration;

namespace Litium.Accelerator.Auth0
{
    public class Auth0Configuration : IAuth0Configuration
    {
        public Auth0Configuration()
        {
            ProviderId = "Auth0";
            Domain = GetRequiredAppsetting("Auth0:Domain");
            ClientId = GetRequiredAppsetting("Auth0:ClientId");
            ClientSecret = GetRequiredAppsetting("Auth0:ClientSecret");
            RedirectUri = GetRequiredAppsetting("Auth0:RedirectUri");
            PostLogoutRedirectUri = GetRequiredAppsetting("Auth0:PostLogoutRedirectUri");
            PersonTemplate = GetRequiredAppsetting("Auth0:PersonTemplate");
        }

        public string Domain { get; }
        public string PersonTemplate { get; }
        public string ProviderId { get; }
        public string ClientId { get; }
        public string ClientSecret { get; }
        public string RedirectUri { get; }
        public string PostLogoutRedirectUri { get; }

        private string GetRequiredAppsetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception($"AppSetting not defined for key '{key}'");
            return value;
        }
    }
}