using Litium.Samples.OrderInspection.Litium.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderInspectorController(OrderOverviewFactory orderOverviewFactory, OrderValidator orderValidator, OrderFixer orderFixer) : ControllerBase
{
    private readonly OrderOverviewFactory _orderOverviewFactory = orderOverviewFactory;
    private readonly OrderValidator _orderValidator = orderValidator;
    private readonly OrderFixer _orderFixer = orderFixer;

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


}
