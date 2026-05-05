using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class PaymentOverview
    {
        public Payment Payment { get; set; } = new();

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
