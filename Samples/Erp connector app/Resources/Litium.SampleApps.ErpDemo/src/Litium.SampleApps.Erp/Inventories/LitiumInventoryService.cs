using Litium.Connect.Erp;
using Litium.SampleApps.Erp.LitiumClients;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.Inventories
{
    internal class LitiumInventoryService : ILitiumInventoryService
    {
        private readonly LitiumClientFactory _litiumClientFactory;

        public LitiumInventoryService(LitiumClientFactory litiumClientFactory)
        {
            _litiumClientFactory = litiumClientFactory;
        }

        public async Task Delete(string id, IEnumerable<InventoryItem> items)
        {
            var client = await _litiumClientFactory.CreateConnectClient();

            await client.Inventories_DeleteAsync(id, null, null, items);
        }

        public async Task<Inventory> Get(string id)
        {
            var client = await _litiumClientFactory.CreateConnectClient();

            return await client.Inventories_GetAsync(id, null, null);
        }

        public async Task Update(string id, IEnumerable<InventoryItem> items)
        {
            var client = await _litiumClientFactory.CreateConnectClient();

            client.Inventories_UpdateAsync(id, null, null, items);
        }
    }
}
