using Litium.Products;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Products;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Litium.Accelerator.Services
{
    [Service(Lifetime = DependencyLifetime.Transient)]
    public class RecentlyVisitedService : IRecentlyVisitedService
    {
        private const int NumberOfItemsToDisplay = 4; // this could be made into a website setting
        private const string Key = "LitiumRecentlyVisited";

        private readonly ProductModelBuilder _productModelBuilder;
        private readonly VariantService _variantService;
        private readonly BaseProductService _baseProductService;

        private List<Item> _items;

        public List<Item> Items {
            get {
                if (_items == null)
                {
                    var cookie = HttpContext.Current.Request.Cookies[Key];
                    _items = cookie == null ? new List<Item>() : JsonConvert.DeserializeObject<List<Item>>(cookie.Value);
                }
                return _items;
            }
            set { _items = value; }
        }

        public RecentlyVisitedService(ProductModelBuilder productModelBuilder,
            VariantService variantService,
            BaseProductService baseProductService)
        {
            _productModelBuilder = productModelBuilder;
            _variantService = variantService;
            _baseProductService = baseProductService;
        }

        public void Add(ProductModel item)
        {
            if (item == null)
                return;

            var productSystemId = item.UseVariantUrl ? item.SelectedVariant.SystemId : item.BaseProduct.SystemId;

            Add(productSystemId);
        }

        public void Add(Guid productSystemId)
        {
            // remove any existing item
            var existingItem = Items.Where(x => x.Id == productSystemId).FirstOrDefault();
            if (existingItem != null)
            {
                Items.Remove(existingItem);
            }

            Items.Add(new Item { Id = productSystemId, LastVisited = DateTime.Now });
            
            // trim the list
            if (Items.Count > NumberOfItemsToDisplay)
            {
                Items.RemoveRange(0, Items.Count - NumberOfItemsToDisplay);
            }

            var cookie = new HttpCookie(Key) { Value = JsonConvert.SerializeObject(Items) };
            HttpContext.Current.Response.Cookies.Set(cookie);
        }

        public List<ProductModel> Get()
        {
            return Items
                .OrderByDescending(x => x.LastVisited)
                .Take(NumberOfItemsToDisplay)
                .Select(
                    item => _productModelBuilder.BuildFromVariant(_variantService.Get(item.Id))
                    ?? _productModelBuilder.BuildFromBaseProduct(_baseProductService.Get(item.Id)))
                .ToList();
        }

        public class Item
        {
            public Guid Id { get; set; }
            public DateTime LastVisited { get; set; }
        }
    }
}
