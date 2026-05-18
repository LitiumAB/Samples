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
            var fulfillmentShipments = orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Fulfillment && s.ShipmentState == "Shipped")
                .ToList();

            var fullfillmentShipmentValue = Math.Round(fulfillmentShipments
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
                result.Add($"No non-success capture transactions found. totalCaptured = {totalCaptured}, but amount in fulfillment shipments = {fullfillmentShipmentValue}");

                var allCaptureTransactions = orderOverview.PaymentOverviews
                    .SelectMany(p => p.Transactions)
                    .Where(t => t.TransactionType == TransactionType.Capture)
                    .ToList();

                if (allCaptureTransactions.Count == 0)
                {
                    result.Add("No capture transactions found. Creating a capture transaction.");
                    var authorization = orderOverview.PaymentOverviews
                        .SelectMany(p => p.Transactions)
                        .FirstOrDefault(t => t.TransactionType == TransactionType.Authorize && t.TransactionResult == TransactionResult.Success);
                    var paymentOverview = orderOverview.PaymentOverviews.FirstOrDefault();

                    if (authorization != null && paymentOverview != null)
                    {
                        var captureTransaction = CreateTransaction(TransactionType.Capture, authorization, paymentOverview, fulfillmentShipments.SelectMany(s => s.Rows), allCaptureTransactions.Count);
                        captureTransaction.TransactionResult = TransactionResult.Success;

                        result.Add($"Start creating capture transaction {captureTransaction.Id} : {captureTransaction.SystemId} to success. Transaction value: {captureTransaction.TotalIncludingVat}");
                        await _salesTransactionClient.Litium_Sales_Transactions_CreateAsync(captureTransaction, cancellationToken);
                        result.Add($"Created capture transaction {captureTransaction.Id} : {captureTransaction.SystemId} to success. Transaction value: {captureTransaction.TotalIncludingVat}");

                        outstandingAmount = Math.Round(outstandingAmount - captureTransaction.Rows.Sum(x => x.TotalIncludingVat), 2);
                    }
                    else if (paymentOverview == null)
                    {
                        result.Add("No payment overview found. Unable to create a capture transaction.");
                    }
                    else
                    {
                        result.Add("No successful authorization transaction found. Unable to create a capture transaction.");
                    }
                }

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

        private Transaction CreateTransaction(TransactionType transactionType, Transaction originalTransaction, PaymentOverview paymentOverview, IEnumerable<ShipmentRow> shipmentRows, int index = 0)
        {
            var transaction = new Transaction
            {
                Id = CreateTransactionId(paymentOverview, index),
                SystemId = System.Guid.NewGuid(),
                TransactionType = transactionType,
                TransactionResult = TransactionResult.Unknown,
                PaymentSystemId = paymentOverview.Payment.SystemId,
                PaymentOption = paymentOverview.Payment.PaymentOption,
                MerchantAccountId = paymentOverview.Payment.MerchantAccountId,
                CurrencyCode = paymentOverview.Payment.CurrencyCode,
            };

            if (originalTransaction is not null)
            {
                transaction.RelatedTransactionSystemId = originalTransaction.SystemId;
                transaction.TransactionReference1 = originalTransaction.TransactionReference1;
                transaction.TransactionReference2 = originalTransaction.TransactionReference2;
                transaction.TransactionEnvironment = originalTransaction.TransactionEnvironment;
            }

            transaction.Rows = shipmentRows.Select(TransactionRowMapper.FromShipmentRow)
                .ToList()
                .SetRowNumber();

            return transaction;
        }

        private string CreateTransactionId(PaymentOverview paymentOverview, int index)
        {
            var paymentOverviewIndex = paymentOverview.Transactions.Count + 1 + index;
            string? id = null;
            while (id is null)
            {
                var nextId = $"{paymentOverview.Payment.Id}T{paymentOverviewIndex}";
                if (paymentOverview.Transactions.Any(t => t.Id == nextId))
                {
                    paymentOverviewIndex++;
                    continue;
                }

                id = nextId;
            }

            return id;
        }
    }
}
