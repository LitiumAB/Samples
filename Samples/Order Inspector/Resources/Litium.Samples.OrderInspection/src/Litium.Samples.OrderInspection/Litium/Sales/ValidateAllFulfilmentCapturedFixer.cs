using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class ValidateAllFulfilmentCapturedFixer
    {
        private readonly ISales_transactionClient _salesTransactionClient;

        public ValidateAllFulfilmentCapturedFixer(ISales_transactionClient salesTransactionClient)
        {
            _salesTransactionClient = salesTransactionClient;
        }

        public async Task<List<string>> Fix(OrderOverview orderOverview, CancellationToken cancellationToken = default)
        {
            var result = new List<string>();

            var fullfillmentShipmentValue = Math.Round(orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Fulfillment && s.ShipmentState == "Shipped")
                .SelectMany(x => x.Rows)
                .Sum(x => x.TotalIncludingVat), 2);
            var totalCaptured = Math.Round(orderOverview.PaymentOverviews.Sum(p => p.TotalCapturedAmount), 2);

            if (totalCaptured >= fullfillmentShipmentValue)
            {
                result.Add($"All fulfillment amounts {fullfillmentShipmentValue} are already captured {totalCaptured}.");
                return result;
            }

            result.Add($"Attempting to fix: Fulfillment shipment value {fullfillmentShipmentValue} is greater than captured amount {totalCaptured}.");

            var nonSuccessCaptureTransactions = orderOverview.PaymentOverviews
                .SelectMany(p => p.Transactions)
                .Where(t => t.TransactionType == TransactionType.Capture && t.TransactionResult != TransactionResult.Success)
                .OrderBy(t => t.SystemId)
                .ToList();

            var outstandingAmount = Math.Round(fullfillmentShipmentValue - totalCaptured, 2);

            if (nonSuccessCaptureTransactions.Count == 0)
            {
                result.Add("No capture transactions available., attempt to create a capture transaction, and set its state to Success.");

                return result;
            }
            else
            {
                foreach (var transaction in nonSuccessCaptureTransactions)
                {
                    if (outstandingAmount <= 0)
                    {
                        break;
                    }

                    var fullTransaction = await _salesTransactionClient.Litium_Sales_Transactions_GetBySystemIdAsync(transaction.SystemId, cancellationToken);
                    if (fullTransaction == null)
                    {
                        continue;
                    }

                    fullTransaction.TransactionResult = TransactionResult.Success;
                    result.Add($"Start updating capture transaction {fullTransaction.Id} : {fullTransaction.SystemId} to success. Transaction value: {fullTransaction.TotalIncludingVat}");
                    await _salesTransactionClient.Litium_Sales_Transactions_UpdateBySystemIdAsync(fullTransaction.SystemId, fullTransaction, cancellationToken);
                    result.Add($"Updated capture transaction {fullTransaction.Id} : {fullTransaction.SystemId} to success. Transaction value: {fullTransaction.TotalIncludingVat}");

                    outstandingAmount = Math.Round(outstandingAmount - fullTransaction.TotalIncludingVat, 2);
                }
            }

            if (outstandingAmount > 0)
            {
                result.Add($"Could not fully reconcile captured amount. Remaining amount to capture: {outstandingAmount}");
            }
            else
            {
                result.Add("Capture transactions were updated to reconcile fulfillment captured amount.");
            }

            return result;
        }
    }
}
