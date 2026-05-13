using Guid = System.Guid;
using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class OrderValidator
    {
        public OrderValidationResult Validate(OrderOverview orderOverview)
        {
            var orderId = orderOverview?.SalesOrder?.Id.ToString() ?? "Unknown";
            var validationChecks = new Dictionary<string, OrderValidationCheck>();
            if (orderOverview == null)
            {
                validationChecks.Add(OrderValidationCheckKeys.Order, new OrderValidationCheck { Success = false, Description = $"Order {orderId} not found" });
                return new OrderValidationResult { IsValid = false, ValidationChecks = validationChecks };
            }

            validationChecks.Add(OrderValidationCheckKeys.Order, new OrderValidationCheck { Success = true, Description = $"Order found. Grand Total = {orderOverview.SalesOrder.GrandTotal}" });

            var payment = orderOverview.PaymentOverviews.FirstOrDefault();
            if (payment == null)
            {
                validationChecks.Add(OrderValidationCheckKeys.Payment, new OrderValidationCheck { Success = false, Description = $"Payment for order {orderId} not found" });
            }
            else
            {
                validationChecks.Add(OrderValidationCheckKeys.Payment, new OrderValidationCheck { Success = true, Description = "Payment found" });
            }

            var orderState = orderOverview.SalesOrder.OrderState;
            validationChecks.Add(OrderValidationCheckKeys.OrderState, new OrderValidationCheck { Success = true, Description = $"Current order state: {orderOverview.SalesOrder.OrderState}" });

            if (orderState == "Processing" || orderState == "Completed")
            {
                var processingValidationChecks = ProcessingToCompletedValidation(orderOverview);
                foreach (var check in processingValidationChecks)
                {
                    validationChecks.TryAdd(check.Key, check.Value);
                }
            }

            ValidateShipmentTransactionConsistency(orderOverview, validationChecks);

            ValidateCancellationShipmentStates(orderOverview, validationChecks);

            var isValid = validationChecks.Values.All(v => v.Success);
            return new OrderValidationResult { IsValid = isValid, ValidationChecks = validationChecks };
        }

        private static void ValidateShipmentTransactionConsistency(OrderOverview orderOverview, Dictionary<string, OrderValidationCheck> checks)
        {
            var captureTransactions = orderOverview.PaymentOverviews
                .SelectMany(p => p.Transactions)
                .Where(t => t.TransactionType == TransactionType.Capture && (t.TransactionResult == TransactionResult.Unknown || t.TransactionResult == TransactionResult.Success || t.TransactionResult == TransactionResult.Pending))
                .ToList();
            var cancelTransactions = orderOverview.PaymentOverviews
                .SelectMany(p => p.Transactions)
                .Where(t => t.TransactionType == TransactionType.Cancel && (t.TransactionResult == TransactionResult.Unknown || t.TransactionResult == TransactionResult.Success || t.TransactionResult == TransactionResult.Pending))
                .ToList();

            var captureTransactionAmount = Math.Round(captureTransactions.Sum(t => t.TotalIncludingVat), 2);
            var cancelTransactionAmount = Math.Round(cancelTransactions.Sum(t => t.TotalIncludingVat), 2);
            var fulfillmentShipmentAmount = Math.Round(orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Fulfillment)
                .SelectMany(s => s.Rows)
                .Sum(r => r.TotalIncludingVat), 2);
            var cancellationShipmentAmount = Math.Round(orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Cancellation)
                .SelectMany(s => s.Rows)
                .Sum(r => r.TotalIncludingVat), 2);

            var isValid = cancelTransactionAmount == cancellationShipmentAmount && captureTransactionAmount == fulfillmentShipmentAmount;
            var description = isValid
                ? "Shipment types are consistent with payment transactions."
                : $"Validation failed: cancel transactions total {cancelTransactionAmount}, cancellation shipments total {cancellationShipmentAmount}, capture transactions total {captureTransactionAmount}, and fulfillment shipments total {fulfillmentShipmentAmount}.";

            checks.Add(OrderValidationCheckKeys.ShipmentTransactionConsistency, new OrderValidationCheck
            {
                Success = isValid,
                Description = description
            });
        }

        private void ValidateCancellationShipmentStates(OrderOverview orderOverview, Dictionary<string, OrderValidationCheck> checks)
        {
            var cancellationShipments = orderOverview.Shipments.Where(s => s.ShipmentType == ShipmentType.Cancellation).ToList();
            if (!cancellationShipments.Any())
            {
                checks.Add(OrderValidationCheckKeys.CancellationShipmentStates, new OrderValidationCheck
                {
                    Success = true,
                    Description = "No cancellation shipments found"
                });
                return;
            }

            var invalidCancellationShipments = cancellationShipments.Where(s => s.ShipmentState != "Cancelled").ToList();
            var allCancellationShipmentsHaveCorrectState = !invalidCancellationShipments.Any();

            var description = allCancellationShipmentsHaveCorrectState
                ? "All cancellation shipments are in Cancelled state"
                : $"Cancellation shipments should be in Cancelled state. Invalid shipments: {string.Join("; ", invalidCancellationShipments.Select(s => $"Shipment {s.Id} is in {s.ShipmentState} state"))}";

            checks.Add(OrderValidationCheckKeys.CancellationShipmentStates, new OrderValidationCheck
            {
                Success = true, //do not consider as a failure.
                Description = description
            });
        }

        private Dictionary<string, OrderValidationCheck> ProcessingToCompletedValidation(OrderOverview orderOverview)
        {
            var checks = new Dictionary<string, OrderValidationCheck>();

            var allShipmentsInFinalState = true;
            var shipmentStateErrors = new List<string>();

            foreach (var shipment in orderOverview.Shipments)
            {
                var shipmentState = shipment.ShipmentState;
                switch (shipmentState)
                {
                    case "Shipped":
                    case "Cancelled":
                        break;
                    default:
                        allShipmentsInFinalState = false;
                        shipmentStateErrors.Add($"Shipment {shipment.Id} (ShipmentType: {shipment.ShipmentType}) is in {shipmentState}. It need to reach Shipped or Cancelled state for the order state to be Completed.");
                        break;
                }
            }

            checks.Add(OrderValidationCheckKeys.AllShipmentsInFinalState, new OrderValidationCheck
            {
                Success = allShipmentsInFinalState,
                Description = allShipmentsInFinalState ? "All shipments are in final state" : string.Join("; ", shipmentStateErrors)
            });

            var hasAllItemsShippedOrCancelled = HasAllItemsShippedOrCancelled(orderOverview);
            checks.Add(OrderValidationCheckKeys.HasAllItemsShippedOrCancelled, new OrderValidationCheck
            {
                Success = hasAllItemsShippedOrCancelled,
                Description = hasAllItemsShippedOrCancelled ? "All items are shipped or cancelled" : "All items in the order should be either shipped or cancelled."
            });

            var fullfillmentShipmentValue = Math.Round(orderOverview.Shipments.Where(s => s.ShipmentType == ShipmentType.Fulfillment).SelectMany(x => x.Rows).Sum(x => x.TotalIncludingVat), 2);
            var totalCaptured = Math.Round(orderOverview.PaymentOverviews.Sum(p => p.TotalCapturedAmount), 2);
            var allFulfillmentCaptured = totalCaptured >= fullfillmentShipmentValue;
            checks.Add(OrderValidationCheckKeys.AllFulfillmentCaptured, new OrderValidationCheck
            {
                Success = allFulfillmentCaptured,
                Description = allFulfillmentCaptured ? $"All fulfillment amounts  {fullfillmentShipmentValue} is captured {totalCaptured}" : $"Total amount in all fulfillment shipments is {fullfillmentShipmentValue}. But only {totalCaptured} is captured."
            });

            ValidateCancellations(orderOverview, checks);

            var shipmentAllRows = orderOverview.Shipments.Where(s => s.ShipmentType == ShipmentType.Fulfillment || s.ShipmentType == ShipmentType.Cancellation).SelectMany(x => x.Rows);
            ValidateSalesTax(orderOverview, checks, shipmentAllRows);

            var shipmentAllRowsTotal = Math.Round(shipmentAllRows.Sum(x => x.TotalIncludingVat), 2);
            var shipmentTotalMatchWithOrderTotal = Math.Round(orderOverview.SalesOrder.GrandTotal, 2) == shipmentAllRowsTotal;
            checks.Add(OrderValidationCheckKeys.ShipmentTotalMatchesOrderTotal, new OrderValidationCheck
            {
                Success = shipmentTotalMatchWithOrderTotal,
                Description = shipmentTotalMatchWithOrderTotal ? $"Shipment total {shipmentAllRowsTotal} matches order total {Math.Round(orderOverview.SalesOrder.GrandTotal, 2)}" : $"Shipments total value , (total value of both fulfilled and cancelled shipments) {shipmentAllRowsTotal} does not match with order total value {Math.Round(orderOverview.SalesOrder.GrandTotal, 2)}."
            });

            return checks;
        }

        public void ValidateCancellations(OrderOverview orderOverview, Dictionary<string, OrderValidationCheck> checks)
        {
            var cancellationShipments = orderOverview.Shipments.Where(s => s.ShipmentType == ShipmentType.Cancellation).ToList();
            if (!cancellationShipments.Any())
            {
                if (orderOverview.Tags.Contains("xCancelledInQliro"))
                {
                    checks.Add(OrderValidationCheckKeys.ValidateCancellations, new OrderValidationCheck
                    {
                        Success = false,
                        Description = "Order is tagged with xCancelledInQliro but no cancellation shipments found."
                    });
                }
                else
                {
                    checks.Add(OrderValidationCheckKeys.ValidateCancellations, new OrderValidationCheck
                    {
                        Success = true,
                        Description = "No cancellation shipments found"
                    });
                }
                return;
            }

            var cancelledShipmentValue = Math.Round(cancellationShipments.SelectMany(x => x.Rows).Sum(x => x.TotalIncludingVat), 2);
            var totalCancelledAndRefunded = Math.Round(orderOverview.PaymentOverviews.Sum(p => p.TotalCancelledAmount) + orderOverview.PaymentOverviews.Sum(p => p.TotalRefundedAmount), 2);
            var allCancellationsProcessed = cancelledShipmentValue == totalCancelledAndRefunded;
            checks.Add(OrderValidationCheckKeys.ValidateCancellations, new OrderValidationCheck
            {
                Success = allCancellationsProcessed,
                Description = allCancellationsProcessed ? $"All cancellation amounts {cancelledShipmentValue} refunded or cancelled" : $"Total amount in all cancelled shipments is {cancelledShipmentValue}. But only {totalCancelledAndRefunded} is cancelled or refunded."
            });            
        }

        private static void ValidateSalesTax(OrderOverview orderOverview, Dictionary<string, OrderValidationCheck> checks, IEnumerable<ShipmentRow> shipmentAllRows)
        {
            var totalTaxInShipments = Math.Round(shipmentAllRows.Where(r => r.OrderRowType == ShipmentRowOrderRowType.Tax).Sum(x => x.TotalIncludingVat), 2);
            var orderTotalTax = Math.Round(orderOverview.SalesOrder.TotalTax, 2);

            bool taxesReconciled;
            string salesTaxDescription;

            if (orderTotalTax == 0)
            {
                taxesReconciled = true;
                salesTaxDescription = "No sales taxes applicable";
            }
            else
            {
                taxesReconciled = totalTaxInShipments == orderTotalTax;
                salesTaxDescription = taxesReconciled ? "All sales taxes reconciled" : "Total estimated sales tax != (total actual sales tax in shipments - any excess sales tax estimated).";
            }

            checks.Add(OrderValidationCheckKeys.SalesTaxesReconciled, new OrderValidationCheck
            {
                Success = taxesReconciled,
                Description = salesTaxDescription
            });
        }

        private bool HasAllItemsShippedOrCancelled(OrderOverview orderOverview)
        {
            var orderCountPerArticle = orderOverview.SalesOrder.Rows.Where(x => x.OrderRowType == SalesOrderRowOrderRowType.Product)
                .GroupBy(r => r.ArticleNumber)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Quantity));

            var shipmentCountPerArticle = orderOverview.Shipments.Where(IsShippedOrCancelled)
                .SelectMany(x => x.Rows.Where(r => r.OrderRowType == ShipmentRowOrderRowType.Product))
                .GroupBy(r => r.ArticleNumber)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.Quantity));

            return shipmentCountPerArticle.Sum(x => x.Value) == orderCountPerArticle.Sum(x => x.Value)
                   && orderCountPerArticle.All(x => shipmentCountPerArticle.TryGetValue(x.Key, out var value) && value == x.Value);

            static bool IsShippedOrCancelled(Shipment shipment)
            {               
                return shipment.ShipmentState == "Shipped" || shipment.ShipmentState == "Cancelled";
            }
        }
    }

    public class OrderValidationCheck
    {
        public bool Success { get; set; }
        public string Description { get; set; }
    }

    public class OrderValidationResult
    {
        public bool IsValid { get; set; }
        public Dictionary<string, OrderValidationCheck> ValidationChecks { get; set; } = new Dictionary<string, OrderValidationCheck>();
    }
}
