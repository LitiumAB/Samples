using Litium.Connect.Erp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.Inventories
{
    public interface ILitiumInventoryService
    {
        Task<Inventory> Get(string id);
        Task Update(string id, IEnumerable<InventoryItem> items);
        Task Delete(string id, IEnumerable<InventoryItem> items);
    }
}