using System;
using System.Net;
using System.Web;
using Litium.Foundation.Security;
using StackExchange.Exceptional;

namespace Community.WebLog.Exceptional.Routing
{
    internal class ExceptionalHandler : IHttpAsyncHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests. </param>
        public void ProcessRequest(HttpContext context)
        {
            context.ApplicationInstance.CompleteRequest();
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            if (!SecurityToken.CurrentSecurityToken.HasSolutionPermission(BuiltInSolutionPermissionTypes.PERMISSION_ID_LANGUAGE_ALL, true, true))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.ApplicationInstance.CompleteRequest();
                return null;
            }

            var page = (IHttpAsyncHandler)new HandlerFactory().GetHandler(context, context.Request.RequestType, context.Request.Url.ToString(), context.Request.PathInfo);
            return page.BeginProcessRequest(context, cb, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            if (result is object && !result.IsCompleted)
            {
                result?.AsyncWaitHandle.WaitOne();
            }
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable => true;
    }
}