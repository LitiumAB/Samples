using System;
using Litium.Connect.Erp;

namespace Litium.SampleApps.Erp.WebHooks
{
    internal class OrderConfirmed
    {
        public Guid SystemId { get; set; }
        public Order Item { get; set; }
    }
}
