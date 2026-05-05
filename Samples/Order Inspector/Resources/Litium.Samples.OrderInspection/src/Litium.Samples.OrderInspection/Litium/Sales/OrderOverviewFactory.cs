using Guid = System.Guid;
using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;
using Microsoft.Extensions.Logging;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    /// <summary>
    /// Creates order overview data by composing data from the Litium Admin Web API.
    /// </summary>
    public class OrderOverviewFactory
    {
        private readonly ISales_sales_orderClient _salesOrderClient;
        private readonly ISales_shipmentClient _shipmentClient;
        private readonly ISales_paymentClient _paymentClient;
        private readonly ISales_sales_return_orderClient _salesReturnOrderClient;
        private readonly ISales_return_authorizationClient _returnAuthorizationClient;
        private readonly global::Litium.Samples.OrderInspection.LitiumApis.Generated.ILitiumConnectErpClient _litiumConnectErpClient;
        private readonly ILogger<OrderOverviewFactory> _logger;

        public OrderOverviewFactory(
            ISales_sales_orderClient salesOrderClient,
            ISales_shipmentClient shipmentClient,
            ISales_paymentClient paymentClient,
            ISales_sales_return_orderClient salesReturnOrderClient,
            ISales_return_authorizationClient returnAuthorizationClient,
            global::Litium.Samples.OrderInspection.LitiumApis.Generated.ILitiumConnectErpClient litiumConnectErpClient,
            ILogger<OrderOverviewFactory> logger)
        {
            _salesOrderClient = salesOrderClient;
            _shipmentClient = shipmentClient;
            _paymentClient = paymentClient;
            _salesReturnOrderClient = salesReturnOrderClient;
            _returnAuthorizationClient = returnAuthorizationClient;
            _litiumConnectErpClient = litiumConnectErpClient;
            _logger = logger;
        }

        public OrderOverview Create(string orderId)
        {
            return CreateAsync(orderId, CancellationToken.None).GetAwaiter().GetResult();
        }

        public async Task<OrderOverview> CreateAsync(string orderId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                throw new ArgumentException("Order id is required.", nameof(orderId));
            }

            var lookupResult = await _salesOrderClient
                .Litium_Sales_SalesOrders_LookupSystemIdAsync([orderId], cancellationToken)
                .ConfigureAwait(false);

            if (!lookupResult.TryGetValue(orderId, out var orderSystemId))
            {
                throw new KeyNotFoundException($"No sales order found for order id '{orderId}' in Lookup cache.");
            }

            var salesOrder = await _salesOrderClient
                .Litium_Sales_SalesOrders_GetBySystemIdAsync(orderSystemId, cancellationToken)
                .ConfigureAwait(false);

            salesOrder.OrderState = await _salesOrderClient
                .Litium_Sales_SalesOrders_GetStateAsync(orderSystemId, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("Building order overview for orderId {OrderId} (systemId: {OrderSystemId}).", orderId, orderSystemId);

            var tags = await _salesOrderClient
                .Litium_Sales_SalesOrders_GetAllTagsAsync(orderSystemId, cancellationToken)
                .ConfigureAwait(false);

            var connectSalesOrder = await _litiumConnectErpClient.GetOrderAsync(orderId, null, null, cancellationToken);

            var shipments = await GetShipmentsAsync(orderSystemId, connectSalesOrder, cancellationToken).ConfigureAwait(false);
            var paymentOverviews = await GetPaymentOverviewsAsync(salesOrder, cancellationToken).ConfigureAwait(false);
            var returnAuthorizations = await GetReturnAuthorizationsAsync(orderSystemId, cancellationToken).ConfigureAwait(false);
            var salesReturnOrders = await GetSalesReturnOrdersAsync(returnAuthorizations, cancellationToken).ConfigureAwait(false);

            _logger.LogDebug(
                "Order overview built for orderId {OrderId}: shipments={ShipmentCount}, paymentOverviews={PaymentOverviewCount}, returnAuthorizations={ReturnAuthorizationCount}, salesReturnOrders={SalesReturnOrderCount}, tags={TagCount}.",
                orderId,
                shipments.Count,
                paymentOverviews.Count,
                returnAuthorizations.Count,
                salesReturnOrders.Count,
                tags?.Count ?? 0);

            return new OrderOverview
            {
                SalesOrder = salesOrder,
                Shipments = shipments,
                PaymentOverviews = paymentOverviews,
                ReturnAuthorizations = returnAuthorizations,
                SalesReturnOrders = salesReturnOrders,
                Tags = new HashSet<string>(tags ?? [], StringComparer.OrdinalIgnoreCase)
            };
        }

        private async Task<IReadOnlyList<Shipment>> GetShipmentsAsync(
            Guid salesOrderSystemId,
            global::Litium.Samples.OrderInspection.LitiumApis.Generated.Order connectSalesOrder,
            CancellationToken cancellationToken)
        {
            var shipments = new List<Shipment>();
            foreach(var connectShipment in connectSalesOrder.Shipments ?? []) 
            {
                var lookupResult = await _shipmentClient
                    .Litium_Sales_Shipments_LookupSystemIdAsync([connectShipment.Id], cancellationToken)
                    .ConfigureAwait(false);

                    if (!lookupResult.TryGetValue(connectShipment.Id, out var shipmentSystemId))
                    {
                        throw new KeyNotFoundException($"No shipment found for shipment id '{connectShipment.Id}' in Lookup cache.");
                    }

                    var shipment = await _shipmentClient
                    .Litium_Sales_Shipments_GetBySystemIdAsync(shipmentSystemId, cancellationToken)
                    .ConfigureAwait(false);

                if (shipment != null)
                {
                    shipments.Add(shipment);
                }
            }

            foreach(var shipment in shipments) 
            {
                shipment.ShipmentState = await _shipmentClient
                    .Litium_Sales_Shipments_GetStateAsync(shipment.SystemId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return shipments;
        }

        private async Task<IReadOnlyList<PaymentOverview>> GetPaymentOverviewsAsync(SalesOrder salesOrder, CancellationToken cancellationToken)
        {
            var paymentOverviews = new List<PaymentOverview>();
            var links = salesOrder.OrderPaymentLinks ?? [];
            var seen = new HashSet<Guid>();

            foreach (var link in links)
            {
                if (!seen.Add(link.PaymentSystemId))
                {
                    continue;
                }

                var payment = await _paymentClient
                    .Litium_Sales_Payments_GetBySystemIdAsync(link.PaymentSystemId, cancellationToken)
                    .ConfigureAwait(false);

                var transactions = await _paymentClient
                    .Sales_Payments_GetPaymentTransactionsAsync(link.PaymentSystemId, cancellationToken)
                    .ConfigureAwait(false);

                paymentOverviews.Add(new PaymentOverview
                {
                    Payment = payment,
                    Transactions = transactions?.ToList() ?? []
                });
            }

            return paymentOverviews;
        }

        private async Task<IReadOnlyList<ReturnAuthorization>> GetReturnAuthorizationsAsync(Guid salesOrderSystemId, CancellationToken cancellationToken)
        {
            var returnAuthorizations = new List<ReturnAuthorization>();
            //TODO: implement.           

            return returnAuthorizations;
        }

        private async Task<IReadOnlyList<SalesReturnOrder>> GetSalesReturnOrdersAsync(
            IReadOnlyCollection<ReturnAuthorization> returnAuthorizations,
            CancellationToken cancellationToken)
        {
            var returnAuthorizationSystemIds = returnAuthorizations
                .Select(x => x.SystemId)
                .ToHashSet();

            if (returnAuthorizationSystemIds.Count == 0)
            {
                return [];
            }

            var salesReturnOrders = new List<SalesReturnOrder>();
            var salesReturnOrderSystemIds = new HashSet<Guid>();
            
            foreach (var salesReturnOrderSystemId in salesReturnOrderSystemIds)
            {
                var salesReturnOrder = await _salesReturnOrderClient
                    .Litium_Sales_SalesReturnOrders_GetBySystemIdAsync(salesReturnOrderSystemId, cancellationToken)
                    .ConfigureAwait(false);

                salesReturnOrders.Add(salesReturnOrder);
            }

            return salesReturnOrders;
        }        
    }
}
