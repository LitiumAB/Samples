using CommonServiceLocator;
using Litium.SampleApps.Erp.WebHooks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Litium.SampleApps.Erp
{
    public class LitiumTokenAuthorizeAttribute : AuthorizeAttribute
    {
        private LitiumTokenValidator LitiumTokenValidator => ServiceLocator.Current.GetInstance<LitiumTokenValidator>();

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return base.IsAuthorized(actionContext) && LitiumTokenValidator.Validate(actionContext.Request);
        }
    }
}
