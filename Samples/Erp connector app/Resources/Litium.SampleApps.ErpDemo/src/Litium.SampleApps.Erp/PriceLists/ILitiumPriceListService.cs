using Litium.Connect.Erp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.PriceLists
{
    public interface ILitiumPriceListService
    {
        Task<PriceList> Get(string id);
        Task Update(string id, IEnumerable<PriceListItem> items);
        Task Delete(string id, IEnumerable<PriceListItem> items);
    }
}
