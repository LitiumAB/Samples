using Litium.Accelerator.ViewModels.Product;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Litium.Accelerator.Services
{
    [Service(ServiceType = typeof(IRecentlyVisitedService))]
    public interface IRecentlyVisitedService
    {
        void Add(ProductModel item);
        void Add(Guid producySystemId);
        List<ProductModel> Get();
    }
}
