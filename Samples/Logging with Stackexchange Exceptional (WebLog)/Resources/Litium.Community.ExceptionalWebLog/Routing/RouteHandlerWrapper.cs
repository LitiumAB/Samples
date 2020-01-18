using System;
using System.Web;
using System.Web.Routing;

namespace Community.WebLog.Exceptional.Routing
{
	/// <summary>
	/// Route handler for the specific http-handler.
	/// </summary>
	public class RouteHandlerWrapper : IRouteHandler
	{
		private readonly IHttpHandler _httpHandler;
		private readonly bool _isReusable;

		/// <summary>
		/// Initializes a new instance of the <see cref="RouteHandlerWrapper" /> class.
		/// </summary>
		/// <param name="httpHandler">The HTTP handler.</param>
		/// <param name="isReusable">if set [is reusable]; otherwise the value is fetched from the <paramref name="httpHandler"></paramref>.</param>
		public RouteHandlerWrapper(IHttpHandler httpHandler, bool? isReusable = null)
		{
			_httpHandler = httpHandler;
			_isReusable = isReusable.GetValueOrDefault(_httpHandler.IsReusable);
		}

		/// <summary>
		/// Provides the object that processes the request.
		/// </summary>
		/// <param name="requestContext">An object that encapsulates information about the request.</param>
		/// <returns>An object that processes the request.</returns>
		IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
		{
			requestContext.HttpContext.Items[requestContext.RouteData.GetType().FullName] = requestContext.RouteData;

			if (_isReusable)
			{
				return _httpHandler;
			}

			return (IHttpHandler)Activator.CreateInstance(_httpHandler.GetType());
		}
	}
}