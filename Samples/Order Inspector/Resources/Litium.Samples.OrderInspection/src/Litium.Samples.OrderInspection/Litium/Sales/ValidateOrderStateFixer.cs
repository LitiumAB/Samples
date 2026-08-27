using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class ValidateOrderStateFixer
    {
        private readonly ISales_sales_orderClient _salesOrderClient;

        public ValidateOrderStateFixer(ISales_sales_orderClient salesOrderClient)
        {
            _salesOrderClient = salesOrderClient;
        }

        public async Task<List<string>> Fix(OrderOverview orderOverview, CancellationToken cancellationToken = default)
        {
            var result = new List<string>();

            if (orderOverview.SalesOrder.OrderState != "Processing")
            {
                result.Add($"Order state is {orderOverview.SalesOrder.OrderState}. No order state fix applied.");
                return result;
            }

            result.Add("All non-order-state validations pass, attempting to update order state to Completed.");
            await _salesOrderClient.Litium_Sales_SalesOrders_SetStateAsync(orderOverview.SalesOrder.SystemId, State2.Completed, cancellationToken);
            result.Add("Order state update to Completed requested.");

            return result;
        }
    }
}
