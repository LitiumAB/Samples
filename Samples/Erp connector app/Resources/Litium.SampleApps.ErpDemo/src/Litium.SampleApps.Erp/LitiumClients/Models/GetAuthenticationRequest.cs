using System.Collections.Generic;
using System.Net.Http;

namespace Litium.SampleApps.Erp.LitiumClients.Models
{
    /// <summary>
    /// Represents a request which is used to get authentication.
    /// </summary>
    internal class GetAuthenticationRequest : BaseRequest
    {
        private readonly string _clientId;
        private readonly string _clientSecret;

        /// <summary>
        /// Initializes an instance of <see cref="GetAuthenticationRequest"/> class.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public GetAuthenticationRequest(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <inheritdoc/>
        public override string GetEndpoint()
        {
            return "/Litium/oauth/token";
        }

        /// <inheritdoc/>
        public override HttpRequestMessage GetHttpRequestMessage()
        {
            return new HttpRequestMessage(HttpMethod.Post, GetEndpoint())
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "grant_type", "client_credentials" },
                        { "client_id", _clientId },
                        { "client_secret", _clientSecret },
                    })
            };
        }
    }
}
