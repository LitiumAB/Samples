namespace Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;

/// <summary>
/// Partial extension of the generated SalesOrder class to include order state information.
/// </summary>
public partial class SalesOrder
{
    /// <summary>
    /// Gets or sets the current order state (e.g., Init, Confirmed, PendingProcessing, Processing, Completed).
    /// This is fetched from the /Litium/api/admin/sales/salesOrders/{systemId}/stateTransition endpoint.
    /// </summary>
    public string? OrderState { get; set; }
}
