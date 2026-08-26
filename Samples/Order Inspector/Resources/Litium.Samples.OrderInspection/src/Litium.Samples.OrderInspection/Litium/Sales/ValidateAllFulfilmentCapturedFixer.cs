using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class ValidateAllFulfilmentCapturedFixer
    {
        private readonly ISales_transactionClient _salesTransactionClient;
        private readonly ISales_paymentClient _salesPaymentClient;
        private readonly ISales_shipmentClient _salesShipmentClient;
        private readonly LitiumApis.Generated.ILitiumConnectErpClient _litiumConnectErpClient;

        public ValidateAllFulfilmentCapturedFixer(
            ISales_transactionClient salesTransactionClient,
            ISales_paymentClient salesPaymentClient,
            ISales_shipmentClient salesShipmentClient,
            LitiumApis.Generated.ILitiumConnectErpClient litiumConnectErpClient)
        {
            _salesTransactionClient = salesTransactionClient;
            _salesPaymentClient = salesPaymentClient;
            _salesShipmentClient = salesShipmentClient;
            _litiumConnectErpClient = litiumConnectErpClient;
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

        internal async Task<IEnumerable<string>> RetryCapture(OrderOverview orderOverview, CancellationToken cancellationToken)
        {
            var result = new List<string>();

            orderOverview = await SetShipmentsToShipped(orderOverview, result, cancellationToken);

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



            var unknownStateCaptures = orderOverview.PaymentOverviews
                                                            .SelectMany(p => p.Transactions)
                                                            .Where(t => t.TransactionType == TransactionType.Capture && t.TransactionResult == TransactionResult.Unknown)
                                                            .OrderBy(t => t.SystemId)
                                                            .ToList();

            //TODO: refetch the 

            var unknownCaptureAmount = Math.Round(unknownStateCaptures.Sum(x => x.TotalIncludingVat), 2);
            if (unknownCaptureAmount == Math.Round(orderOverview.SalesOrder.GrandTotal, 2))
            {
                if (unknownCaptureAmount == fullfillmentShipmentValue)
                {
                    //await UpdatePaymentToParialCaptureAsync(orderOverview, result, cancellationToken);

                    try
                    {
                        result.Add($"Attempting to fix by finalizing to retry capture: unknownCaptureAmount is {unknownCaptureAmount}");
                        await _litiumConnectErpClient.FinalizeOrderAsync(orderOverview.SalesOrder.Id, "2.4", "2.4", cancellationToken);
                    }
                    catch(Exception ex)
                    {
                        result.Add($"Failed finlaization {ex.Message}");
                    }
                }
                else
                {
                    result.Add($"Cannot fix because unknownCaptureAmount of {unknownCaptureAmount} is not equal to fullfillmentShipmentValue of {fullfillmentShipmentValue}");
                }
            }
            else
            {
                result.Add($"Cannot fix because unknownCaptureAmount of {unknownCaptureAmount} is not equal to grand total of {Math.Round(orderOverview.SalesOrder.GrandTotal, 2)}");
            }

            return result;
        }

        private async System.Threading.Tasks.Task UpdatePaymentToParialCaptureAsync(OrderOverview orderOverview, List<string> result, CancellationToken cancellationToken)
        {
            //put payment into support partial capture mode.
            var paymentSystemId = orderOverview.PaymentOverviews.FirstOrDefault()?.Payment.SystemId ?? System.Guid.Empty;
            var payment = await _salesPaymentClient
                .Litium_Sales_Payments_GetBySystemIdAsync(paymentSystemId, cancellationToken);
            if (payment is null)
            {
                result.Add($"Could not load payment {paymentSystemId}. Capture retry may not be triggered if partial capture support is disabled.");
            }
            else
            {
                if (payment.SupportedActions != null && payment.SupportedActions.PartialCapture == false)
                {
                    payment.SupportedActions.PartialCapture = true;
                    await _salesPaymentClient.Litium_Sales_Payments_UpdateBySystemIdAsync(
                        payment.SystemId,
                        payment,
                        cancellationToken);
                    result.Add($"Payment {payment.Id} : {payment.SystemId} updated with SupportedActions.PartialCapture=true.");
                }
            }
        }

        private async System.Threading.Tasks.Task<OrderOverview> SetShipmentsToShipped(OrderOverview orderOverview, List<string> result, CancellationToken cancellationToken)
        {
            var processingShipments = orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Fulfillment && s.ShipmentState == "Processing")
                .ToList();

            foreach (var shipment in processingShipments)
            {
                try
                {
                    result.Add($"Attempting to update shipment {shipment.Id} to ReadyToShip state.");
                    await _salesShipmentClient.Litium_Sales_Shipments_SetStateAsync(
                        shipment.SystemId,
                        State4.ReadyToShip,
                        cancellationToken);
                    result.Add($"Shipment {shipment.Id} updated to ReadyToShip state.");

                    await WaitForStateChangeAsync(shipment, "ReadyToShip", cancellationToken);
                }
                catch (Exception ex)
                {
                    result.Add($"Failed to update shipment {shipment.Id} to ReadyToShip state. {ex.Message}");
                }
            }

            var readyToShipShipments = orderOverview.Shipments
               .Where(s => s.ShipmentType == ShipmentType.Fulfillment && s.ShipmentState == "ReadyToShip")
               .ToList();

            foreach (var shipment in readyToShipShipments)
            {
                try
                {       
                    result.Add($"Attempting to update shipment {shipment.Id} to Shipped state.");
                    await _salesShipmentClient.Litium_Sales_Shipments_SetStateAsync(
                        shipment.SystemId,
                        State4.Shipped,
                        cancellationToken);
                    result.Add($"Shipment {shipment.Id} updated to Shipped state.");

                    await WaitForStateChangeAsync(shipment, "Shipped", cancellationToken);
                }
                catch (Exception ex)
                {
                    result.Add($"Failed to update shipment {shipment.Id} to ReadyToShip state. {ex.Message}");
                }
            }
            return orderOverview;
        }

        private async System.Threading.Tasks.Task WaitForStateChangeAsync(Shipment shipment, string expectedState, CancellationToken cancellationToken)
        {
            for (int i = 0; i < 3; i++)
            {
                var shipmentCopy = await _salesShipmentClient.Litium_Sales_Shipments_GetBySystemIdAsync(shipment.SystemId);
                if (shipmentCopy.ShipmentState == expectedState)
                {
                    shipment.ShipmentState = expectedState;
                    break;
                }

                // wait only between retries
                if (i < 2)
                {
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
            }
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
