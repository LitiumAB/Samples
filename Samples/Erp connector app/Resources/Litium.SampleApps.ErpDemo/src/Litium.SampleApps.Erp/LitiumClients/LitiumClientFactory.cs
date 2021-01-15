using Litium.Connect.Erp;
using Litium.SampleApps.Erp.LitiumClients.Models;
using Litium.SampleApps.Erp.Loggers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.LitiumClients
{
    public class LitiumClientFactory
    {
        private readonly Lazy<HttpClient> _clientLazyHolder;
        private HttpClient _client => _clientLazyHolder.Value;

        private OAuthResponse _oAuthResponse;
        private readonly SemaphoreSlim _authRequestSemaphore = new SemaphoreSlim(1, 1);

        private readonly ILogger<LitiumClientFactory> _logger;

        public LitiumClientFactory(ILogger<LitiumClientFactory> logger)
        {
            _logger = logger;

            _clientLazyHolder = new Lazy<HttpClient>(() =>
            {
                var client = new HttpClient();
                ConfigureHttpClient(client, LitiumSettings.Host);

                return client;

            });
        }

        public async Task<ILitiumConnectClient> CreateConnectClient()
        {
            await GetAuthenticationAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_oAuthResponse.TokenType, _oAuthResponse.AccessToken);

            return new LitiumConnectClient(_client);
        }

        public async Task<ILitiumWebHookClient> CreateWebHookClient()
        {
            await GetAuthenticationAsync();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_oAuthResponse.TokenType, _oAuthResponse.AccessToken);

            return new LitiumWebHookClient(_client);
        }

        private async Task GetAuthenticationAsync()
        {
            if (NeedToGetAuthentication())
            {
                await _authRequestSemaphore.WaitAsync();

                try
                {
                    if (NeedToGetAuthentication())
                    {
                        _logger.Info("Requesting new token.");
                        
                        var request = new GetAuthenticationRequest(LitiumSettings.ClientId, LitiumSettings.ClientSecret);
                        _oAuthResponse = await MakeRequestAsync<OAuthResponse>(request);
                    }
                }
                finally
                {
                    _authRequestSemaphore.Release();
                }
            }
        }

        private async Task<TResult> MakeRequestAsync<TResult>(BaseRequest request)
        {
            using (var message = request.GetHttpRequestMessage())
            {
                return await MakeRequestInternalAsync<TResult>(message);
            }
        }

        private async Task<TResult> MakeRequestInternalAsync<TResult>(HttpRequestMessage httpRequestMessage)
        {
            var response = await _client.SendAsync(httpRequestMessage).ConfigureAwait(false);

            var stream = await response.Content.ReadAsStreamAsync();

            if (response.IsSuccessStatusCode)
                return DeserializeJsonFromStream<TResult>(stream);

            var headers = Enumerable.ToDictionary(response.Headers, h => h.Key, h => h.Value);
            if (response.Content != null && response.Content.Headers != null)
            {
                foreach (var item_ in response.Content.Headers)
                    headers[item_.Key] = item_.Value;
            }

            var responseData = await StreamToStringAsync(stream);

            throw new ApiException("The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").", (int)response.StatusCode, responseData, headers, null);
        }

        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default;

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                var searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }

        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();

            return content;
        }

        private bool NeedToGetAuthentication()
        {
            if (_oAuthResponse == null)
            {
                return true;
            }

            var remainingSeconds = (_oAuthResponse.ExpiresAt - DateTimeOffset.Now).TotalSeconds;

            return remainingSeconds < 10;
        }

        private static void ConfigureHttpClient(HttpClient client, string serverUrl)
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }
    }
}
