namespace Litium.Samples.OrderInspection.Litium.Sales
{
    public static class OrderValidationCheckKeys
    {
        public const string Order = "Order";
        public const string Payment = "Payment";
        public const string OrderState = "orderState";
        public const string AllShipmentsInFinalState = "allShipmentsInFinalState";
        public const string HasAllItemsShippedOrCancelled = "hasAllItemsShippedOrCancelled";
        public const string AllFulfillmentCaptured = "allFulfillmentCaptured";
        public const string ValidateCancellations = "ValidateCancellations";
        public const string ShipmentTotalMatchesOrderTotal = "shipmentTotalMatchesOrderTotal";
        public const string SalesTaxesReconciled = "salesTaxesReconciled";
        public const string ShipmentTransactionConsistency = "shipmentTransactionConsistency";
        public const string CancellationShipmentStates = "cancellationShipmentStates";
    }
}
