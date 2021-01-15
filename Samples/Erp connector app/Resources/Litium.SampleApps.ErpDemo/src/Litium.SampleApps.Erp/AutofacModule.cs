using Autofac;
using Litium.SampleApps.Erp.Inventories;
using Litium.SampleApps.Erp.LitiumClients;
using Litium.SampleApps.Erp.Orders;
using Litium.SampleApps.Erp.PriceLists;
using Litium.SampleApps.Erp.Rmas;
using Litium.SampleApps.Erp.WebHooks;

namespace Litium.SampleApps.Erp
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LitiumWebHookService>().As<ILitiumWebHookService>();
            builder.RegisterType<LitiumOrderService>().As<ILitiumOrderService>();
            builder.RegisterType<LitiumRmaService>().As<ILitiumRmaService>();
            builder.RegisterType<LitiumInventoryService>().As<ILitiumInventoryService>();
            builder.RegisterType<LitiumPriceListService>().As<ILitiumPriceListService>();
            builder.RegisterType<LitiumClientFactory>().SingleInstance();
            builder.RegisterType<WebHookStorage>().SingleInstance();
        }
    }
}
