using Litium.Connect.Erp;
using Litium.SampleApps.Erp.LitiumClients;
using Litium.SampleApps.Erp.Loggers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.WebHooks
{
    internal class LitiumWebHookService : ILitiumWebHookService
    {
        private readonly LitiumClientFactory _litiumClientFactory;
        private readonly ILogger<LitiumWebHookService> _logger;

        public LitiumWebHookService(LitiumClientFactory litiumClientFactory, ILogger<LitiumWebHookService> logger)
        {
            _litiumClientFactory = litiumClientFactory;
            _logger = logger;
        }

        public void AddOrUpdate(params string[] events)
        {
            Task.Run(async () => {
                    try
                    {
                    await AddOrUpdateInternal(events).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Could not add/update webhook registration.", ex);
                    }
                }
            );
        }

        private async Task AddOrUpdateInternal(params string[] events)
        {
            var client = await _litiumClientFactory.CreateWebHookClient();
            var args = new RequestData()
            {
                Id = "ErpDemo",
                WebHookUri = new System.Uri($"{LitiumSettings.CallbackHost}/api/webhooks/incoming/litium/"),
                Secret = LitiumSettings.Secret,
                Filters = new List<string>(events),
                Description = "ERP Demo"
            };

            try
            {
                await client.WebHookRegistrations_LookupAsync(args.Id);

                await client.WebHookRegistrations_PutAsync(args.Id, args);

                _logger.Info($"SelfRegistration - Updated to {string.Join(", ", args.Filters)} succesfully.");

            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    await client.WebHookRegistrations_PostAsync(args);

                    _logger.Info($"SelfRegistration - Added to {string.Join(", ", args.Filters)} succesfully.");
                }
                throw;
            }
        }
    }
}
