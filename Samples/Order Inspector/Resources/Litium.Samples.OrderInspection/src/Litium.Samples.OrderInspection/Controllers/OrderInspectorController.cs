using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderInspectorController : ControllerBase
{
    [HttpGet("ValidateOrder/{orderId}")]
    public IActionResult ValidateOrder(string orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return BadRequest(new { error = "orderId is required." });
        }

        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
