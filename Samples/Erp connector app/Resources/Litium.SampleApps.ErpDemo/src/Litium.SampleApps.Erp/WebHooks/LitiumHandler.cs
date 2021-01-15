using System.Threading.Tasks;
using Litium.SampleApps.Erp.Loggers;
using Microsoft.AspNet.WebHooks;
using CommonServiceLocator;
using Litium.SampleApps.Erp.Orders;
using System;
using Newtonsoft.Json;

namespace Litium.SampleApps.Erp.WebHooks
{
    public class LitiumHandler : WebHookHandler
    {
        private readonly ILogger<LitiumHandler> _logger = ServiceLocator.Current.GetInstance<ILogger<LitiumHandler>>();
        private readonly WebHookStorage _storage = ServiceLocator.Current.GetInstance<WebHookStorage>();
        private readonly ILitiumOrderService _litiumOrderService = ServiceLocator.Current.GetInstance<ILitiumOrderService>();

        public LitiumHandler()
        {
            Receiver = LitiumReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            // Get data from WebHook
            var data = context.GetDataOrDefault<WebHookData>();

            Save(data);
            Handle(data);

            return Task.FromResult(true);
        }

        private void Save(WebHookData data)
        {
            try
            {
                _storage.Add(data);
            }
            catch (Exception ex)
            {
                _logger.Error($"Can not store web hook data.", ex);
            }
        }

        private void Handle(WebHookData data)
        {
            switch (data.Action)
            {
                case "Litium.Connect.Erp.Events.OrderConfirmed" :
                    var orderConfirmed = Cast<OrderConfirmed>(data);
                    if(orderConfirmed != null)
                    {
                        try
                        {
                            Task.Run(async () => await _litiumOrderService.ExportedToErpAsync(orderConfirmed.Item.Id));
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"There are errors", ex);
                        }
                        
                    }
                    break;
                default:
                    break;
            }
        }

        private T Cast<T>(WebHookData data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data.Resource);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch(Exception ex)
            {
                _logger.Error($"Can not cast web hook data to {typeof(T)}", ex);

                return default;
            }
        }
    }
}
