using Newtonsoft.Json;
using System;

namespace Litium.SampleApps.Erp.LitiumClients.Models
{
    /// <summary>
    /// Represents an OAuth response.
    /// </summary>
    public class OAuthResponse
    {
        private DateTimeOffset _createdAt;

        public OAuthResponse()
        {
            _createdAt = DateTimeOffset.Now;
        }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        public DateTimeOffset ExpiresAt => _createdAt.AddSeconds(ExpiresIn);
    }
}