using System.Web;
using System.Web.Mvc;

namespace Litium.SampleApps.ErpDemo
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
