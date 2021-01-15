using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Litium.SampleApps.Erp.LitiumClients.Models
{
    /// <summary>
    /// Represents a base request.
    /// </summary>
    internal abstract class BaseRequest
    {
        /// <summary>
        /// Get the endpoint of litium connect.
        /// </summary>
        /// <returns>The endpoint.</returns>
        public abstract string GetEndpoint();

        /// <summary>
        /// Get the http request message.
        /// </summary>
        /// <returns>The http request message.</returns>
        public abstract HttpRequestMessage GetHttpRequestMessage();

        /// <summary>
        /// Create a json content.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected static HttpContent CreateJsonContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }
    }
}