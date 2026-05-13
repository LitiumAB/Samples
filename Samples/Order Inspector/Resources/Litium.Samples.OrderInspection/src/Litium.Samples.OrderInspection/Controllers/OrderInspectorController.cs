using Litium.Samples.OrderInspection.Litium.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderInspectorController(OrderOverviewFactory orderOverviewFactory, OrderValidator orderValidator, OrderFixer orderFixer, OrderFinder orderFinder) : ControllerBase
{
    private readonly OrderOverviewFactory _orderOverviewFactory = orderOverviewFactory;
    private readonly OrderValidator _orderValidator = orderValidator;
    private readonly OrderFixer _orderFixer = orderFixer;
    private readonly OrderFinder _orderFinder = orderFinder;

    [HttpGet("ValidateOrder/{orderId}")]
    public async Task<IActionResult> ValidateOrder(string orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return BadRequest(new { error = "orderId is required." });
        }

        try
        {
            var orderOverview = await _orderOverviewFactory.CreateAsync(orderId, cancellationToken);
            var validationResult = _orderValidator.Validate(orderOverview);
            return Ok(validationResult);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"No sales order found for order id '{orderId}'." });
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to validate order", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpGet("FixOrder/{orderId}")]
    public async Task<IActionResult> FixOrder(string orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return BadRequest(new { error = "orderId is required." });
        }

        try
        {
            var fixResult = await _orderFixer.FixOrderAsync(orderId, cancellationToken);
            return Ok(fixResult);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"No sales order found for order id '{orderId}'." });
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to validate order", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Finds sales orders by tags.
    /// </summary>
    /// <param name="tags">Tags is a comma separated list of tags.</param>
    /// <param name="matchAll">If true, all tags must match. If false (default), any tag match is enough.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>List of order IDs that match the requested tags.</returns>
    [HttpGet("FindOrdersByTag")]
    public async Task<IActionResult> FindOrdersByTag(
        string tags,
        bool matchAll = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return BadRequest(new { error = "tags is required." });
        }

        try
        {
            var orders = await _orderFinder.FindOrdersByTagAsync(tags, matchAll, cancellationToken);
            var orderIds = orders.Select(x => x.Id).ToList();
            return Ok(orderIds);
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to find orders by tag", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }


}
