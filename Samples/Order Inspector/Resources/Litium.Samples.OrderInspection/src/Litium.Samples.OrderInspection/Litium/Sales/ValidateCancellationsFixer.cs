using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class ValidateCancellationsFixer
    {
       private readonly OrderValidator _orderValidator;

       public ValidateCancellationsFixer(OrderValidator orderValidator)
       {
           _orderValidator = orderValidator;
       }

        public Dictionary<string, OrderValidationCheck> Fix(OrderOverview orderOverview)
        {
            var result = new Dictionary<string, OrderValidationCheck>();

            var cancellationShipments = orderOverview.Shipments.Where(s => s.ShipmentType == ShipmentType.Cancellation).ToList();
            var cancelledShipmentValue = Math.Round(cancellationShipments.SelectMany(x => x.Rows).Sum(x => x.TotalIncludingVat), 2);
            
            var totalCancelledAndRefunded = Math.Round(orderOverview.PaymentOverviews.Sum(p => p.TotalCancelledAmount) + orderOverview.PaymentOverviews.Sum(p => p.TotalRefundedAmount), 2);
            var allCancellationsProcessed = cancelledShipmentValue == totalCancelledAndRefunded;
            if (!allCancellationsProcessed)
            {
                //set any cancel and refund transaction status to Success.
                foreach (var payment in orderOverview.PaymentOverviews)
                {
                    foreach (var transaction in payment.Transactions.Where(t => (t.TransactionType == TransactionType.Cancel || t.TransactionType == TransactionType.Refund) && t.TransactionResult != TransactionResult.Success))
                    {
                        transaction.TransactionResult = TransactionResult.Success;
                        //TODO: Try update the transaction in Litium.
                    }
                }
            }
            else
            {
                result.Add("ValidateCancellations", new OrderValidationCheck
                {
                    Status = true,
                    Description = $"All cancellations are processed. Total cancelled amount: {cancelledShipmentValue}, total cancelled and refunded amount: {totalCancelledAndRefunded}"
                });
            }

            
            return result;
        }
    }
}
