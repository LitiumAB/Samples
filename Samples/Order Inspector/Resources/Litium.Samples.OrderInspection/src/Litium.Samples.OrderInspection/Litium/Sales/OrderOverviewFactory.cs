using Guid = System.Guid;
using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    /// <summary>
    /// Creates order overview data by composing data from the Litium Admin Web API.
    /// </summary>
    public class OrderOverviewFactory
    {
        private const int PageSize = 100;

        private readonly ISales_sales_orderClient _salesOrderClient;
        private readonly ISales_shipmentClient _shipmentClient;
        private readonly ISales_paymentClient _paymentClient;
        private readonly ISales_sales_return_orderClient _salesReturnOrderClient;
        private readonly ISales_return_authorizationClient _returnAuthorizationClient;

        public OrderOverviewFactory(
            ISales_sales_orderClient salesOrderClient,
            ISales_shipmentClient shipmentClient,
            ISales_paymentClient paymentClient,
            ISales_sales_return_orderClient salesReturnOrderClient,
            ISales_return_authorizationClient returnAuthorizationClient)
        {
            _salesOrderClient = salesOrderClient;
            _shipmentClient = shipmentClient;
            _paymentClient = paymentClient;
            _salesReturnOrderClient = salesReturnOrderClient;
            _returnAuthorizationClient = returnAuthorizationClient;
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
                throw new KeyNotFoundException($"No sales order found for order id '{orderId}'.");
            }

            var salesOrder = await _salesOrderClient
                .Litium_Sales_SalesOrders_GetBySystemIdAsync(orderSystemId, cancellationToken)
                .ConfigureAwait(false);

            var tags = await _salesOrderClient
                .Litium_Sales_SalesOrders_GetAllTagsAsync(orderSystemId, cancellationToken)
                .ConfigureAwait(false);

            var shipments = await GetShipmentsAsync(orderSystemId, cancellationToken).ConfigureAwait(false);
            var paymentOverviews = await GetPaymentOverviewsAsync(salesOrder, cancellationToken).ConfigureAwait(false);
            var returnAuthorizations = await GetReturnAuthorizationsAsync(orderSystemId, cancellationToken).ConfigureAwait(false);
            var salesReturnOrders = await GetSalesReturnOrdersAsync(returnAuthorizations, cancellationToken).ConfigureAwait(false);

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

        private async Task<IReadOnlyList<Shipment>> GetShipmentsAsync(Guid salesOrderSystemId, CancellationToken cancellationToken)
        {
            var shipments = new List<Shipment>();
            var skip = 0;

            while (true)
            {
                var page = await _shipmentClient
                    .Litium_Sales_Shipments_SearchAsync(new SearchModel
                    {
                        Take = PageSize,
                        Skip = skip
                    }, cancellationToken)
                    .ConfigureAwait(false);

                var items = page?.Items?.ToList() ?? [];
                if (items.Count == 0)
                {
                    break;
                }

                shipments.AddRange(items.Where(x => x.OrderSystemId == salesOrderSystemId));
                skip += items.Count;

                if (skip >= page!.Total)
                {
                    break;
                }
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
            var skip = 0;

            while (true)
            {
                var page = await _returnAuthorizationClient
                    .Litium_Sales_ReturnAuthorizations_SearchAsync(new SearchModel
                    {
                        Take = PageSize,
                        Skip = skip
                    }, cancellationToken)
                    .ConfigureAwait(false);

                var items = page?.Items?.ToList() ?? [];
                if (items.Count == 0)
                {
                    break;
                }

                returnAuthorizations.AddRange(items.Where(x => x.SalesOrderSystemId == salesOrderSystemId));
                skip += items.Count;

                if (skip >= page!.Total)
                {
                    break;
                }
            }

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
            var skip = 0;

            while (true)
            {
                var page = await _salesReturnOrderClient
                    .Litium_Sales_SalesReturnOrders_SearchAsync(new SearchModel
                    {
                        Take = PageSize,
                        Skip = skip
                    }, cancellationToken)
                    .ConfigureAwait(false);

                var items = page?.Items?.ToList() ?? [];
                if (items.Count == 0)
                {
                    break;
                }

                salesReturnOrders.AddRange(items.Where(x => x.ReturnAuthorizationSystemId.HasValue && returnAuthorizationSystemIds.Contains(x.ReturnAuthorizationSystemId.Value)));
                skip += items.Count;

                if (skip >= page!.Total)
                {
                    break;
                }
            }

            return salesReturnOrders;
        }
    }
}
