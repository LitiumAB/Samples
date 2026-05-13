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

                if (nonSuccessCancelTransactions.Count == 0)
                {
                    result.Add($"No non-success cancel or refund transactions found. totalCancelledAndRefunded = {totalCancelledAndRefunded}, but amount in cancellation shipments = {cancelledShipmentValue}" );
                    //only add the cancellation transaction, only if there are no other cancellation transactions.
                    var allCancelTransactions = orderOverview.PaymentOverviews
                    .SelectMany(p => p.Transactions)
                    .Where(t => (t.TransactionType == TransactionType.Cancel || t.TransactionType == TransactionType.Refund))
                    .ToList();
                    if (allCancelTransactions.Count == 0)
                    {
                        result.Add($"No cancel or refund transactions found. Creating a cancel transaction.");
                        var authorization = orderOverview.PaymentOverviews.SelectMany(p => p.Transactions).Where(t => (t.TransactionType == TransactionType.Authorize) && t.TransactionResult == TransactionResult.Success).FirstOrDefault();
                        if (authorization != null)
                        {
                            foreach(var shipment in cancellationShipments)
                            {
                                var cancelTransaction = CreateTransaction(TransactionType.Cancel, authorization, orderOverview.PaymentOverviews.First(), shipment.Rows, allCancelTransactions.Count);
                                //use admin api to create the transaction.
                                result.Add($"Start creating transaction {cancelTransaction.Id} : {cancelTransaction.SystemId} of type {cancelTransaction.TransactionType} to success. Transaction value: {cancelTransaction.TotalIncludingVat}");
                                await _salesTransactionClient.Litium_Sales_Transactions_CreateAsync(cancelTransaction, cancellationToken);
                                result.Add($"Created transaction  {cancelTransaction.Id} : {cancelTransaction.SystemId} of type {cancelTransaction.TransactionType} to success. Transaction value: {cancelTransaction.TotalIncludingVat}");
                            }
                        }
                        else
                        {
                            result.Add($"No successful authorization transaction found. Unable to create a cancel transaction.");
                        }
                    }
                    return result;
                }
                else
                {
                    var totalNonSuccessCancelAndRefund = Math.Round(nonSuccessCancelTransactions.Sum(t => t.TotalIncludingVat), 2);
                    if (cancelledShipmentValue == totalNonSuccessCancelAndRefund)
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
            }
            else
            {
                result.Add( $"All cancellations are processed. Total cancelled amount: {cancelledShipmentValue}, total cancelled and refunded amount: {totalCancelledAndRefunded}");
            }
            
            return result;
        }


        private Transaction CreateTransaction(TransactionType transactionType, Transaction originalTransaction, PaymentOverview paymentOverview, IEnumerable<ShipmentRow> shipmentRows, int index = 0)
        {
            var transaction = new Transaction
            {
                Id = CreateTransactionId(paymentOverview,index),
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
            transaction.Rows = shipmentRows.Select(CreateTransactionRow)
                .ToList()
                .SetRowNumber();

            return transaction;
        }

        private static TransactionRow CreateTransactionRow(ShipmentRow shipmentRow)
        {
            return new TransactionRow
            {
                OrderRowSystemId = shipmentRow.OrderRowSystemId,
                OriginOrderRowSystemId = shipmentRow.OriginOrderRowSystemId,
                RowType = GetRowType(shipmentRow.OrderRowType, shipmentRow.ProductType),
                ShipmentRowSystemId = shipmentRow.SystemId,
                Description = shipmentRow.Description,
                UnitPriceIncludingVat = shipmentRow.UnitPriceIncludingVat,
                UnitPriceExcludingVat = shipmentRow.UnitPriceExcludingVat,
                Quantity = shipmentRow.Quantity,
                VatRate = shipmentRow.VatRate,
                TotalIncludingVat = shipmentRow.TotalIncludingVat,
                TotalExcludingVat = shipmentRow.TotalExcludingVat,
                TotalVat = shipmentRow.TotalVat,
                AdditionalInfo = shipmentRow.AdditionalInfo?.ToDictionary(s => s.Key, s => s.Value),
                ArticleNumber = shipmentRow.ArticleNumber,
                VatDetails = shipmentRow.VatDetails?.Select(x => (VatDetail)((ICloneable)x).Clone()).ToList(),
                TaxDetails = shipmentRow.TaxDetails?.Select(x => (TaxDetail)((ICloneable)x).Clone()).ToList()
            };
        }

        private static TransactionRowRowType GetRowType(ShipmentRowOrderRowType orderRowType, ShipmentRowProductType productType)
        {
            return orderRowType switch
            {
                ShipmentRowOrderRowType.ShippingFee => TransactionRowRowType.ShippingFee,
                ShipmentRowOrderRowType.Tax => TransactionRowRowType.Tax,
                ShipmentRowOrderRowType.Fee => TransactionRowRowType.Fee,
                ShipmentRowOrderRowType.Discount => TransactionRowRowType.Discount,
                ShipmentRowOrderRowType.RoundingOffAdjustment => TransactionRowRowType.RoundingOffAdjustment,
                _ => productType switch
                {
                    ShipmentRowProductType.DigitalGoods => TransactionRowRowType.DigitalGoods,
                    ShipmentRowProductType.PhysicalGoods => TransactionRowRowType.PhysicalGoods,
                    ShipmentRowProductType.Service => TransactionRowRowType.Service,
                    _ => TransactionRowRowType.Unknown,
                },
            };
        }

        public string CreateTransactionId(PaymentOverview paymentOverview, int index)
        {
            var paymentOverviewIndex = paymentOverview.Transactions.Count + 1 + index   ;
            string id = string.Empty;
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

    public static class RowNumberCalculator
    {
        public static List<TransactionRow> SetRowNumber(this List<TransactionRow> items)
        {
            var padCount = CreatePadding(items.Count);
            var i = 1;

            foreach (var item in items)
            {
                item.Id = (i++).ToString().PadLeft(padCount, '0');
            }

            return items;
        }
        private static int CreatePadding(int count)
        {
            return count switch
            {
                int when count < 10 => 1,
                int when count < 100 => 2,
                int when count < 1000 => 3,
                int when count < 10000 => 4,
                int when count < 100000 => 5,
                int when count < 1000000 => 6,
                int when count < 10000000 => 7,
                int when count < 100000000 => 8,
                int when count < 1000000000 => 9,
                _ => 20,
            };
        }
    }
}
