using CommonServiceLocator;
using Litium.SampleApps.Erp.WebHooks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Litium.SampleApps.ErpDemo
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutofacConfig.Register();

            RegisterLitiumWebHook();
        }

        public static void RegisterLitiumWebHook()
        {
            ServiceLocator.Current.GetInstance<ILitiumWebHookService>().AddOrUpdate("Litium.Connect.Erp.Events.OrderConfirmed", "Litium.Connect.Erp.Events.ReadyToShip");
        }
    }
}
