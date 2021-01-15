using Litium.Connect.Erp;
using Litium.SampleApps.Erp.LitiumClients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.PriceLists
{
    internal class LitiumPriceListService : ILitiumPriceListService
    {
        private readonly LitiumClientFactory _litiumClientFactory;

        public LitiumPriceListService(LitiumClientFactory litiumClientFactory)
        {
            _litiumClientFactory = litiumClientFactory;
        }

        public async Task Delete(string id, IEnumerable<PriceListItem> items)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            await client.Prices_DeleteAsync(id, null, null, items);
        }

        public async Task<PriceList> Get(string id)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Prices_GetAsync(id, null, null);
        }

        public async Task Update(string id, IEnumerable<PriceListItem> items)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            await client.Prices_UpdateAsync(id, null, null, items);
        }
    }
}
