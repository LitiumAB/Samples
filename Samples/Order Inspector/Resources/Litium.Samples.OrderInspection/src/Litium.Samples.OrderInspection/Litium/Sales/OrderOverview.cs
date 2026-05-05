using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class OrderOverview
    {
        public SalesOrder SalesOrder { get; set; } = new();

        public IEnumerable<Shipment> Shipments { get; set; } = [];

        public IEnumerable<PaymentOverview> PaymentOverviews { get; set; } = [];

        public IEnumerable<SalesReturnOrder> SalesReturnOrders { get; set; } = [];

        public IEnumerable<ReturnAuthorization> ReturnAuthorizations { get; set; } = [];

        public ISet<string> Tags { get; set; } = new HashSet<string>();
    }
}
