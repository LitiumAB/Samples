using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Products;
using System;
using System.Collections.Generic;

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
