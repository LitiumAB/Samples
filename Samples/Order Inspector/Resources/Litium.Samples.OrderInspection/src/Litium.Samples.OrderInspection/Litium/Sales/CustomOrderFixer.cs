namespace Litium.Samples.OrderInspection.Litium.Sales;

using global::Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;
using Microsoft.Extensions.Logging;

public class CustomOrderFixer(
    OrderFinder orderFinder,
    OrderOverviewFactory orderOverviewFactory,
    ISales_sales_orderClient salesOrderClient,
    OrderFixer orderFixer,
    ILogger<CustomOrderFixer> logger)
{
    private readonly OrderFinder _orderFinder = orderFinder;
    private readonly OrderOverviewFactory _orderOverviewFactory = orderOverviewFactory;
    private readonly ISales_sales_orderClient _salesOrderClient = salesOrderClient;
    private readonly OrderFixer _orderFixer = orderFixer;
    private readonly ILogger<CustomOrderFixer> _logger = logger;

    public Task<List<string>> RetryCaptureAsync(string orderId, CancellationToken cancellationToken = default)
    {
        return RetryCaptureForPaymentOrdersAsync(cancellationToken);
    }

    private async Task<List<string>> RetryCaptureForPaymentOrdersAsync(CancellationToken cancellationToken)
    {
        var fromDate = new System.DateTimeOffset(2026, 8, 1, 13, 35, 0, System.TimeSpan.Zero);
        var toDate = new System.DateTimeOffset(2026, 8, 24, 13, 35, 0, System.TimeSpan.Zero);

        var orders = await _orderFinder
            .FindOrdersByDateRangeTagsAsync("PaymentOrder", fromDate, toDate, matchAll: true, cancellationToken)
            .ConfigureAwait(false);

        var matchingOrderIds = new List<string>();

        foreach (var order in orders)
        {
            var orderOverview = await _orderOverviewFactory
                .CreateAsync(order.Id, cancellationToken)
                .ConfigureAwait(false);

            var hasUnknownCapture = orderOverview.PaymentOverviews
                .SelectMany(p => p.Transactions)
                .Any(t => t.TransactionType == TransactionType.Capture && t.TransactionResult == TransactionResult.Unknown);

            var fulfillmentTotal = Math.Round(orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Fulfillment)
                .SelectMany(s => s.Rows)
                .Sum(r => r.TotalIncludingVat), 2);

            var orderTotal = Math.Round(orderOverview.SalesOrder.GrandTotal, 2);
            var orderTotalIncludedInFulfillmentShipments = fulfillmentTotal >= orderTotal;

            if (string.Equals(orderOverview.SalesOrder.OrderState, "Processing", StringComparison.OrdinalIgnoreCase)
                && hasUnknownCapture
                && orderTotalIncludedInFulfillmentShipments)
            {
                matchingOrderIds.Add(orderOverview.SalesOrder.Id);
            }
        }

        var result = new List<string>
        {
            $"Found {orders.Count} orders with tag 'PaymentOrder' between {fromDate:yyyy-MM-dd HH:mm} and {toDate:yyyy-MM-dd HH:mm}.",
            $"Orders in state 'Processing' with a capture transaction result 'Unknown': {matchingOrderIds.Count}."
        };
        _logger.LogInformation("Found {matchingOrderIdsCount} orders with state 'Processing' and a capture transaction result 'Unknown'.", matchingOrderIds.Count);
        var total = matchingOrderIds.Count;
        var processed = 0;
        foreach (var matchingOrderId in matchingOrderIds)
        {
            processed++;
            try
            {
                var matchingOrderOverview = await _orderOverviewFactory
                    .CreateAsync(matchingOrderId, cancellationToken)
                    .ConfigureAwait(false);

                if (!matchingOrderOverview.Tags.Contains("xRetryCaptureInQliro"))
                {
                    if (!matchingOrderOverview.Tags.Contains("xCancelledInQliro"))
                    {
                        if (!matchingOrderOverview.Tags.Contains("xCapturedInQliro"))
                        {
                            await _salesOrderClient
                                .Litium_Sales_SalesOrders_AddTagAsync(matchingOrderOverview.SalesOrder.SystemId, "xRetryCaptureInQliro", cancellationToken)
                                .ConfigureAwait(false);

                            _logger.LogInformation("Fixing order {processed} of {total}: {matchingOrderId} with FixOrderAsync", matchingOrderId, processed, total);
                            var fixResult = await _orderFixer
                                .FixOrderAsync(matchingOrderId, cancellationToken)
                                .ConfigureAwait(false);

                            result.Add($"Processed order {matchingOrderId}: tagged with xRetryCaptureInQliro and invoked FixOrderAsync.");
                            result.AddRange(fixResult.Select(x => $"{matchingOrderId}: {x}"));
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                result.Add($"Failed to process order {matchingOrderId}: {ex.Message}");
            }
        }

        result.AddRange(matchingOrderIds.Select(id => $"Matched order: {id}"));

        return result;
    }
}
