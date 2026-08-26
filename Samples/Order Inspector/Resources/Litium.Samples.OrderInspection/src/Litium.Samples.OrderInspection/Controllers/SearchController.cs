using Litium.Samples.OrderInspection.Litium.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController(OrderSearchService orderSearchService) : ControllerBase
{
    private readonly OrderSearchService _orderSearchService = orderSearchService;

    /// <summary>
    /// Searches sales orders using date, order-status, shipping-status, and order-tag filters.
    /// </summary>
    /// <param name="startDate">Inclusive order-date range start in YYYY-MM-DD HH:mm:ss format with timezone, for example 2026-06-28 22:02:00Z.</param>
    /// <param name="endDate">Inclusive order-date range end in YYYY-MM-DD HH:mm:ss format with timezone, for example 2026-08-24 07:03:13Z.</param>
    /// <param name="orderStatus">Order status, for example <c>processing</c>.</param>
    /// <param name="shippingStatus">Shipping status, for example <c>shipped</c>.</param>
    /// <param name="orderTag">Order tag, for example <c>PaymentOrder</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet("Orders")]
    public async Task<IActionResult> Orders(
        [FromQuery] System.DateTimeOffset? startDate,
        [FromQuery] System.DateTimeOffset? endDate,
        [FromQuery] string? orderStatus,
        [FromQuery] string? shippingStatus,
        [FromQuery] string? orderTag,
        CancellationToken cancellationToken)
    {
        if (startDate is null || endDate is null)
        {
            return BadRequest(new { error = "startDate and endDate are required." });
        }

        try
        {
            var orderIds = await _orderSearchService.SearchAsync(
                startDate.Value,
                endDate.Value,
                orderStatus ?? string.Empty,
                shippingStatus ?? string.Empty,
                orderTag ?? string.Empty,
                cancellationToken);
            return Ok(new { Total = orderIds.Count, OrderIds = orderIds });
        }
        catch (OrderSearchFormatException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Problem(title: "Failed to search orders", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
