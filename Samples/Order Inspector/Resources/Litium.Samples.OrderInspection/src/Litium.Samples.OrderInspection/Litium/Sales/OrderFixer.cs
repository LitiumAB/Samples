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
        private readonly ISales_shipmentClient _salesShipmentClient;

        public OrderFixer(
            OrderOverviewFactory orderOverviewFactory,
            OrderValidator orderValidator,
            ValidateCancellationsFixer validateCancellationsFixer,
            ValidateAllFulfilmentCapturedFixer validateAllFulfilmentCapturedFixer,
            ISales_sales_orderClient salesOrderClient,
            ISales_shipmentClient salesShipmentClient)
        {
            _orderOverviewFactory = orderOverviewFactory;
            _orderValidator = orderValidator;
            _validateCancellationsFixer = validateCancellationsFixer;
            _validateAllFulfilmentCapturedFixer = validateAllFulfilmentCapturedFixer;
            _salesOrderClient = salesOrderClient;
            _salesShipmentClient = salesShipmentClient;
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

            if (validationResult.ValidationChecks.TryGetValue(OrderValidationCheckKeys.ReadyToShipShipmentStates, out var readyToShipShipmentStatesCheck) && !readyToShipShipmentStatesCheck.Success)
            {
                if (!orderOverview.Tags.Contains("xShipped"))
                {
                    result.Add("ReadyToShip shipment validation check fails, but order cannot be fixed because order is not tagged with xShipped");
                }
                else
                {
                    var fixResult = await SetReadyToShipShipmentsToShippedAsync(orderOverview, cancellationToken);
                    result.AddRange(fixResult);
                }
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
                if (orderOverview.Tags.Contains("xCapturedInQliro"))
                {
                    var fixResult = await _validateAllFulfilmentCapturedFixer.Fix(orderOverview, cancellationToken);
                    result.AddRange(fixResult);
                }
                else if(orderOverview.Tags.Contains("xRetryCaptureInQliro"))
                {
                    var fixResult = await _validateAllFulfilmentCapturedFixer.RetryCapture(orderOverview, cancellationToken);
                    result.AddRange(fixResult);
                }
                else
                {
                    result.Add("Capture validation checks fails, but order cannot be fixed because order is not tagged with xCapturedInQliro");                   
                }
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

        private async Task<List<string>> SetReadyToShipShipmentsToShippedAsync(OrderOverview orderOverview, CancellationToken cancellationToken)
        {
            var result = new List<string>();
            var readyToShipFulfillmentShipments = orderOverview.Shipments
                .Where(s => s.ShipmentType == ShipmentType.Fulfillment && s.ShipmentState == "ReadyToShip")
                .ToList();

            if (readyToShipFulfillmentShipments.Count == 0)
            {
                result.Add("No fulfillment shipments in ReadyToShip state found.");
                return result;
            }

            foreach (var shipment in readyToShipFulfillmentShipments)
            {
                try
                {
                    result.Add($"Attempting to update shipment {shipment.Id} to Shipped state.");
                    await _salesShipmentClient.Litium_Sales_Shipments_SetStateAsync(shipment.SystemId, State4.Shipped, cancellationToken);
                    result.Add($"Shipment {shipment.Id} updated to Shipped state.");
                }
                catch (Exception ex)
                {
                    result.Add($"Failed to update shipment {shipment.Id} to Shipped state. {ex.Message}");
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
