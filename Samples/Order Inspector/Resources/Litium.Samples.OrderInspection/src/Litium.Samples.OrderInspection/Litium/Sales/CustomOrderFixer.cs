namespace Litium.Samples.OrderInspection.Litium.Sales;
using global::Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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

    public Task<List<string>> RetryCaptureAsync(System.DateTimeOffset startDate, System.DateTimeOffset endDate, string orderTag, CancellationToken cancellationToken = default)
    {
        return RetryCaptureForPaymentOrdersAsync(startDate, endDate, orderTag, cancellationToken);
    }

    public async Task<List<string>> RetryCaptureAsync(string orderIdsList, CancellationToken cancellationToken = default)
    {
        var result = new List<string>();

        if (string.IsNullOrWhiteSpace(orderIdsList))
        {
            result.Add("No order IDs were provided.");
            return result;
        }

        var orderIds = orderIdsList
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (orderIds.Length == 0)
        {
            result.Add("No valid order IDs were found after parsing.");
            return result;
        }

        var total = orderIds.Length;
        var processed = 0;
        foreach (var orderId in orderIds)
        {
            processed++;
            var orderOverview = await _orderOverviewFactory
                .CreateAsync(orderId, cancellationToken)
                .ConfigureAwait(false);

            var hasUnknownCapture = orderOverview.PaymentOverviews
                .SelectMany(p => p.Transactions)
                .Any(t => t.TransactionType == TransactionType.Capture && t.TransactionResult == TransactionResult.Unknown);

            var fulfillmentShipments = orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Fulfillment)
                .ToList();

            var fulfillmentTotal = Math.Round(fulfillmentShipments
                .SelectMany(s => s.Rows)
                .Sum(r => r.TotalIncludingVat), 2);

            var orderTotal = Math.Round(orderOverview.SalesOrder.GrandTotal, 2);
            var orderTotalIncludedInFulfillmentShipments = fulfillmentTotal >= orderTotal;
            var allFulfillmentShipmentsShipped = fulfillmentShipments.Count != 0
                && fulfillmentShipments.All(s => string.Equals(s.ShipmentState, "Shipped", StringComparison.OrdinalIgnoreCase));

            if (string.Equals(orderOverview.SalesOrder.OrderState, "Processing", StringComparison.OrdinalIgnoreCase)
                && hasUnknownCapture
                && orderTotalIncludedInFulfillmentShipments
                && allFulfillmentShipmentsShipped)
            {               
                if (!orderOverview.Tags.Contains("xRetryCaptureInQliro"))
                {
                    if (!orderOverview.Tags.Contains("xCancelledInQliro"))
                    {
                        if (!orderOverview.Tags.Contains("xCapturedInQliro"))
                        {
                            try
                            {
                                await _salesOrderClient
                                    .Litium_Sales_SalesOrders_AddTagAsync(orderOverview.SalesOrder.SystemId, "xRetryCaptureInQliro", cancellationToken)
                                    .ConfigureAwait(false);

                                _logger.LogInformation("Fixing order {processed} of {total}: {matchingOrderId} with FixOrderAsync", orderId, processed, total);
                                var fixResult = await _orderFixer
                                    .FixOrderAsync(orderId, cancellationToken)
                                    .ConfigureAwait(false);

                                result.Add($"Processed order {orderId}: tagged with xRetryCaptureInQliro and invoked FixOrderAsync.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to process order {orderId}: {message}", orderId, ex.Message);
                                result.Add($"Failed to process order {orderId}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    private async Task<List<string>> RetryCaptureForPaymentOrdersAsync(System.DateTimeOffset fromDate, System.DateTimeOffset toDate, string orderTag, CancellationToken cancellationToken)
    {
        var orders = await _orderFinder
            .FindOrdersByDateRangeTagsAsync(orderTag, fromDate, toDate, matchAll: true, cancellationToken)
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

            var fulfillmentShipments = orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Fulfillment)
                .ToList();

            var fulfillmentTotal = Math.Round(fulfillmentShipments
                .SelectMany(s => s.Rows)
                .Sum(r => r.TotalIncludingVat), 2);

            var orderTotal = Math.Round(orderOverview.SalesOrder.GrandTotal, 2);
            var orderTotalIncludedInFulfillmentShipments = fulfillmentTotal >= orderTotal;
            var allFulfillmentShipmentsShipped = fulfillmentShipments.Count != 0
                && fulfillmentShipments.All(s => string.Equals(s.ShipmentState, "Shipped", StringComparison.OrdinalIgnoreCase));

            if (string.Equals(orderOverview.SalesOrder.OrderState, "Processing", StringComparison.OrdinalIgnoreCase)
                && hasUnknownCapture
                && orderTotalIncludedInFulfillmentShipments
                && allFulfillmentShipmentsShipped)
            {
                matchingOrderIds.Add(orderOverview.SalesOrder.Id);
            }
        }

        var result = new List<string>
        {
            $"Found {orders.Count} orders with tag '{orderTag}' between {fromDate:yyyy-MM-dd HH:mm} and {toDate:yyyy-MM-dd HH:mm}.",
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
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                result.Add($"Failed to process order {matchingOrderId}: {ex.Message}");
            }
        }

        return result;
    }
}
