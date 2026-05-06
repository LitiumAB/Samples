using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class ValidateCancellationsFixer
    {
       private readonly ISales_transactionClient _salesTransactionClient;
       public ValidateCancellationsFixer(ISales_transactionClient salesTransactionClient)
       {
           _salesTransactionClient = salesTransactionClient;
       }

        public async Task<List<string>> Fix(OrderOverview orderOverview, CancellationToken cancellationToken = default)
        {
            var result = new List<string>();

            var cancellationShipments = orderOverview.Shipments.Where(s => s.ShipmentType == ShipmentType.Cancellation).ToList();
            var cancelledShipmentValue = Math.Round(cancellationShipments.SelectMany(x => x.Rows).Sum(x => x.TotalIncludingVat), 2);
            
            var totalCancelledAndRefunded = Math.Round(orderOverview.PaymentOverviews.Sum(p => p.TotalCancelledAmount) + orderOverview.PaymentOverviews.Sum(p => p.TotalRefundedAmount), 2);
            var allCancellationsProcessed = cancelledShipmentValue == totalCancelledAndRefunded;
            if (!allCancellationsProcessed)
            {
                result.Add($"Attempting to fix: Cancelled shipment value {cancelledShipmentValue} does not match the total cancelled or refunded value {totalCancelledAndRefunded}");
                var nonSuccessCancelTransactions = orderOverview.PaymentOverviews
                    .SelectMany(p => p.Transactions)
                    .Where(t => (t.TransactionType == TransactionType.Cancel || t.TransactionType == TransactionType.Refund) && t.TransactionResult != TransactionResult.Success)
                    .ToList();

                var totalNonSuccessCancelAndRefund = Math.Round(nonSuccessCancelTransactions.Sum(t => t.TotalIncludingVat), 2);
                if(cancelledShipmentValue == totalNonSuccessCancelAndRefund)
                {
                    result.Add($"All cancellations and/or refunds are marked as non-success, but the total amount of cancelled shipments ({cancelledShipmentValue}) matches the total amount of non-success cancel and refund transactions ({totalNonSuccessCancelAndRefund}). Attempting to fix by marking all cancel and refund transactions as success.");
                    foreach (var transaction in nonSuccessCancelTransactions)
                    {
                        //re-fetch the full transaction and update it.
                        var fullTransaction = await _salesTransactionClient.Litium_Sales_Transactions_GetBySystemIdAsync(transaction.SystemId, cancellationToken);
                        if (fullTransaction != null)
                        {
                            fullTransaction.TransactionResult = TransactionResult.Success;
                            result.Add($"Start Updating transaction {fullTransaction.Id} : {fullTransaction.SystemId} of type {fullTransaction.TransactionType} to success. Transaction value: {fullTransaction.TotalIncludingVat}");
                            await _salesTransactionClient.Litium_Sales_Transactions_UpdateBySystemIdAsync(fullTransaction.SystemId, fullTransaction, cancellationToken);
                            result.Add($"Updated transaction  {fullTransaction.Id} : {fullTransaction.SystemId} of type {fullTransaction.TransactionType} to success. Transaction value: {fullTransaction.TotalIncludingVat}");
                        }                       
                    }
                }
            }
            else
            {
                result.Add( $"All cancellations are processed. Total cancelled amount: {cancelledShipmentValue}, total cancelled and refunded amount: {totalCancelledAndRefunded}");
            }
            
            return result;
        }
    }
}
