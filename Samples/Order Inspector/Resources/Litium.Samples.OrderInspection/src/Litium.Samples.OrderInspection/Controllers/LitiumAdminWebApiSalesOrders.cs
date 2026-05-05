using Litium.Samples.OrderInspection.LitiumApis.Generated.Admin;
using Litium.Samples.OrderInspection.Litium.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

/// <summary>
/// Endpoints for Litium Admin Web API sales order operations.
/// </summary>
[ApiController]
[Route("[controller]")]
public class LitiumAdminWebApiSalesOrders(
    ISales_sales_orderClient salesSalesOrderClient,
    OrderOverviewFactory orderOverviewFactory) : ControllerBase
{
    private readonly ISales_sales_orderClient _salesSalesOrderClient = salesSalesOrderClient;
    private readonly OrderOverviewFactory _orderOverviewFactory = orderOverviewFactory;

    /// <summary>
    /// Gets a sales order from Litium Admin Web API by system id.
    /// </summary>
    /// <param name="systemId">The sales order system identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The sales order payload from Litium Admin Web API.</returns>
    [HttpGet("GetOrder/{systemId:guid}")]
    public async Task<IActionResult> GetOrder(System.Guid systemId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _salesSalesOrderClient.Litium_Sales_SalesOrders_GetBySystemIdAsync(systemId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to get sales order", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Looks up a sales order system id by its string order id using the Litium Admin Web API key lookup endpoint.
    /// </summary>
    /// <param name="orderId">The string order identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The Guid system id of the sales order.</returns>
    [HttpGet("KeyLookupSalesOrder/{orderId}")]
    public async Task<IActionResult> KeyLookupSalesOrder(string orderId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _salesSalesOrderClient.Litium_Sales_SalesOrders_LookupSystemIdAsync([orderId], cancellationToken);
            if (result.TryGetValue(orderId, out var systemId))
            {
                return Ok(systemId);
            }
            return NotFound($"No sales order found for order id '{orderId}'.");
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to look up sales order", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Builds an order overview close to Litium API OrderOverview for a given order id.
    /// </summary>
    /// <param name="orderId">The string order identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>Order overview with sales order, shipments, payments, returns, and tags.</returns>
    [HttpGet("GetOrderOverview/{orderId}")]
    public async Task<IActionResult> GetOrderOverview(string orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return BadRequest(new { error = "orderId is required." });
        }

        try
        {
            var overview = await _orderOverviewFactory.CreateAsync(orderId, cancellationToken);
            return Ok(overview);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"No sales order found for order id '{orderId}'." });
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to build order overview", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
