using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public class OrderFixer
    {
        private readonly OrderOverviewFactory _orderOverviewFactory;
        private readonly OrderValidator _orderValidator;
        private readonly ValidateCancellationsFixer _validateCancellationsFixer;
        private readonly ValidateAllFulfilmentCapturedFixer _validateAllFulfilmentCapturedFixer;
        private readonly ISales_sales_orderClient _salesOrderClient;

        public OrderFixer(
            OrderOverviewFactory orderOverviewFactory,
            OrderValidator orderValidator,
            ValidateCancellationsFixer validateCancellationsFixer,
            ValidateAllFulfilmentCapturedFixer validateAllFulfilmentCapturedFixer,
            ISales_sales_orderClient salesOrderClient)
        {
            _orderOverviewFactory = orderOverviewFactory;
            _orderValidator = orderValidator;
            _validateCancellationsFixer = validateCancellationsFixer;
            _validateAllFulfilmentCapturedFixer = validateAllFulfilmentCapturedFixer;
            _salesOrderClient = salesOrderClient;
        }

        public async Task<List<string>> FixOrderAsync(string orderId, CancellationToken cancellationToken = default)
        {
            var result = new List<string>();
            var orderOverview = await _orderOverviewFactory.CreateAsync(orderId, cancellationToken);
            var validationResult = _orderValidator.Validate(orderOverview);
            if(validationResult.IsValid == true)
            {
                result.Add($"Order {orderId} is already valid.");
                //check whether order has a shipment

                if (orderOverview.SalesOrder.OrderState == "Processing")
                {
                    result.Add("Order is in Processing state, attempting to put it into Completed state.");
                    await SetOrderToCompleted(result, orderOverview, cancellationToken);    
                }
                return result;
            }

            foreach (var check in validationResult.ValidationChecks)
            {
                result.Add($"{check.Key}: {(check.Value.Success ? "Passed" : "Failed")} - {check.Value.Description}");
            }

            if (validationResult.ValidationChecks.TryGetValue(OrderValidationCheckKeys.ValidateCancellations, out var cancellationsCheck) && !cancellationsCheck.Success)
            {
                if(!orderOverview.Tags.Contains("xCancelledInQliro"))
                {
                    result.Add("Cancellation validation checks fails, but order cannot be fixed because order is not tagged with xCancelledInQliro");
                }
                else
                {
                    var fixResult = await _validateCancellationsFixer.Fix(orderOverview);
                    result.AddRange(fixResult);
                }
            }

            if (validationResult.ValidationChecks.TryGetValue(OrderValidationCheckKeys.AllFulfillmentCaptured, out var allFulfillmentCapturedCheck) && !allFulfillmentCapturedCheck.Success)
            {
                var fixResult = await _validateAllFulfilmentCapturedFixer.Fix(orderOverview, cancellationToken);
                result.AddRange(fixResult);
            }

            var orderOverviewAfterFix = await _orderOverviewFactory.CreateAsync(orderId, cancellationToken);
            var validationAfterFix = _orderValidator.Validate(orderOverviewAfterFix);
            if(validationAfterFix.IsValid)
            {
                result.Add($"All validations pass. Order is in {orderOverviewAfterFix.SalesOrder.OrderState}");
                await SetOrderToCompleted(result, orderOverviewAfterFix, cancellationToken);
            }
            else
            {
                result.Add($"Order is still not valid after fixes. Current state: {orderOverviewAfterFix.SalesOrder.OrderState}");
                foreach (var check in validationAfterFix.ValidationChecks)
                {
                    result.Add($"{check.Key}: {(check.Value.Success ? "Passed" : "Failed")} - {check.Value.Description}");
                }
            }

            return result;
        }

        private async System.Threading.Tasks.Task SetOrderToCompleted(List<string> result, OrderOverview orderOverview, CancellationToken cancellationToken)
        {
            if (orderOverview.SalesOrder.OrderState == "Processing")
            {
                result.Add("Attempting to put order into Completed state.");
                await _salesOrderClient.Litium_Sales_SalesOrders_SetStateAsync(orderOverview.SalesOrder.SystemId, State2.Completed, cancellationToken);
                result.Add("Order state updated to Completed.");
            }
        }
    }
}
