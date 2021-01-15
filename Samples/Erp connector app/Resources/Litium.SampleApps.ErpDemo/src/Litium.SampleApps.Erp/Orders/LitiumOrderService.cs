using System.Threading.Tasks;
using Litium.Connect.Erp;
using Litium.SampleApps.Erp.LitiumClients;

namespace Litium.SampleApps.Erp.Orders
{
    internal class LitiumOrderService : ILitiumOrderService
    {
        private readonly LitiumClientFactory _litiumClientFactory;

        public LitiumOrderService(LitiumClientFactory litiumClientFactory)
        {
            _litiumClientFactory = litiumClientFactory;
        }

        public async Task<Order> GetOrderAsync(string orderId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Orders_GetAsync(orderId, null, null);
        }

        public async Task<Order> ExportedToErpAsync(string orderId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Orders_OrderExportedToErpAsync(orderId, null, null);
        }

        public async Task<string> CreateShipmentAsync(string orderId, Shipment shipment)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Orders_CreateShipmentAsync(orderId, null, null, shipment);
        }

        public async Task<Order> NotifyOrderDeliveredAsync(string orderId, string deliveryId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.Orders_NotifyOrderDeliveredAsync(orderId, deliveryId, null, null);
        }

        public async Task<Order> ConfirmReturnOrderAsync(string orderId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.SalesReturnOrders_ConfirmReturnAsync(orderId, null, null);
        }

        public async Task<Order> RefundOrderAsync(string orderId)
        {
            var client = await _litiumClientFactory.CreateConnectClient();
            return await client.SalesReturnOrders_RefundAsync(orderId, null, null);
        }
    }
}