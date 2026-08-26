using Litium.Samples.OrderInspection.Litium.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController(OrderSearchService orderSearchService) : ControllerBase
{
    private readonly OrderSearchService _orderSearchService = orderSearchService;

    /// <summary>
    /// Searches sales orders using the filters from a Litium backoffice order-grid URL.
    /// </summary>
    /// <param name="backofficeUrl">The complete backoffice order-grid URL.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    [HttpGet("Orders")]
    public async Task<IActionResult> Orders([FromQuery] string backofficeUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(backofficeUrl))
        {
            return BadRequest(new { error = "backofficeUrl is required." });
        }

        try
        {
            var orderIds = await _orderSearchService.SearchAsync(backofficeUrl, cancellationToken);
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
