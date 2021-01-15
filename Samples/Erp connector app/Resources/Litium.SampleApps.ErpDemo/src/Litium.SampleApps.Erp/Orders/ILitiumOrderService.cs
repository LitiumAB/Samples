using Litium.Connect.Erp;
using System.Threading.Tasks;

namespace Litium.SampleApps.Erp.Orders
{
    public interface ILitiumOrderService
    {
        Task<Order> GetOrderAsync(string orderId);
        Task<Order> ExportedToErpAsync(string orderId);
        Task<string> CreateShipmentAsync(string orderId, Shipment shipment);
        Task<Order> NotifyOrderDeliveredAsync(string orderId, string deliveryId);
        Task<Order> ConfirmReturnOrderAsync(string orderId);
        Task<Order> RefundOrderAsync(string orderId);
    }
}
