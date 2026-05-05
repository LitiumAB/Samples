using System.Linq;
using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class PaymentOverview
    {
        public Payment Payment { get; set; } = new();

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        /// <summary>
        /// Gets the total amount of all authorized transactions.
        /// </summary>
        /// <value>The total authorized amount.</value>
        public double TotalAuthorizedAmount => Transactions.Where(x => x.TransactionType == TransactionType.Authorize && x.TransactionResult == TransactionResult.Success).Sum(x => x.TotalIncludingVat);

        /// <summary>
        /// Gets the total amount of all captured transactions.
        /// </summary>
        /// <value>The total captured amount.</value>
        public double TotalCapturedAmount => Transactions.Where(x => x.TransactionType == TransactionType.Capture && x.TransactionResult == TransactionResult.Success).Sum(x => x.TotalIncludingVat);

        /// <summary>
        /// Gets the total amount of all refunded transactions.
        /// </summary>
        /// <value>The total refunded amount.</value>
        public double TotalRefundedAmount => Transactions.Where(x => x.TransactionType == TransactionType.Refund && x.TransactionResult == TransactionResult.Success).Sum(x => x.TotalIncludingVat);

        /// <summary>
        /// Gets the total amount of all canceled transactions.
        /// </summary>
        /// <value>The total cancelled amount.</value>
        public double TotalCancelledAmount => Transactions.Where(x => x.TransactionType == TransactionType.Cancel && x.TransactionResult == TransactionResult.Success).Sum(x => x.TotalIncludingVat);

        /// <summary>
        /// Gets the total amount of all authorize transactions which are pending.
        /// </summary>
        /// <value>The total pending captured amount.</value>
        public double TotalPendingAuthorize => Transactions.Where(x => x.TransactionType == TransactionType.Authorize && x.TransactionResult == TransactionResult.Pending).Sum(x => x.TotalIncludingVat);

        /// <summary>
        /// Gets the total amount of all captured transactions which are pending.
        /// </summary>
        /// <value>The total pending captured amount.</value>
        public double TotalPendingCapture => Transactions.Where(x => x.TransactionType == TransactionType.Capture && x.TransactionResult == TransactionResult.Pending).Sum(x => x.TotalIncludingVat);

        /// <summary>
        /// Gets the total amount of all refunded transactions which are pending.
        /// </summary>
        /// <value>The total pending refunded amount.</value>
        public double TotalPendingRefund => Transactions.Where(x => x.TransactionType == TransactionType.Refund && x.TransactionResult == TransactionResult.Pending).Sum(x => x.TotalIncludingVat);

        /// <summary>
        /// Gets the total amount of all canceled transactions which are pending.
        /// </summary>
        /// <value>The total pending cancelled amount.</value>
        public double TotalPendingCancel => Transactions.Where(x => x.TransactionType == TransactionType.Cancel && x.TransactionResult == TransactionResult.Pending).Sum(x => x.TotalIncludingVat);
    }
}
