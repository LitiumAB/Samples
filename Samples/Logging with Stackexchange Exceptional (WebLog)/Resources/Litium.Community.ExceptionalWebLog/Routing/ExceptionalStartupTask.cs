using System;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using Litium.Foundation.Configuration;
using Litium.Owin.Lifecycle;

namespace Community.WebLog.Exceptional.Routing
{
    internal class ExceptionalStartupTask : IStartupTask
    {
        void IStartupTask.Start()
        {
#if NET472
            var litiumFolder = "Litium";
#else
            var litiumFolder = "LitiumStudio";
#endif

            RouteTable.Routes.Insert(0, new RouteNoOutboundPath($"{litiumFolder}/exceptional", new RouteHandlerWrapper(new ExceptionalHandler())));
            RouteTable.Routes.Insert(0, new RouteNoOutboundPath($"{litiumFolder}/exceptional/{{resource}}", new RouteHandlerWrapper(new ExceptionalHandler())));
            RouteTable.Routes.Insert(0, new RouteNoOutboundPath($"{litiumFolder}/exceptional/{{resource}}/{{subResource}}", new RouteHandlerWrapper(new ExceptionalHandler())));

            var location = $"%2F{litiumFolder}%2Fexceptional";
            var version = new Version(typeof(Litium.Foundation.Solution).Assembly.GetCustomAttributes<AssemblyFileVersionAttribute>().First().Version);
            var framePage = version.Major < 5
                ? $"~/{litiumFolder}/Foundation/Exceptional.aspx"
                : version.Major < 7 || version.Minor < 4
                ? $"~/{litiumFolder}/Framework/Frame.aspx/{location}"
                : $"~/{litiumFolder}/UI/settings/iframe/{location}";

#if NET472
            var link = new ControlPanelPage(framePage, "ExceptionalMenuTitle", ControlPanelPagePermission.All);
#else
            var link = new ControlPanelPage(framePage, null, "ExceptionalMenuTitle", ControlPanelPagePermission.All);
#endif
            ControlPanelPagesConfig.Instance.SystemSettings.Add(link);
        }
    }
}