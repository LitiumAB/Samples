namespace Litium.Accelerator.Auth0
{
    public class Auth0Configuration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Domain { get; set; }
        public string PersonTemplate { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        // Set unique providerid per Auth0 app so that switching between Auth0 applications will still work
        public string ProviderId => $"Auth0-{ClientId}";
        public string RedirectUri { get; set; }
    }
}
