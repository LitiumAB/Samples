using Litium.Samples.OrderInspection.Litium.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomOrderFixerController(CustomOrderFixer customOrderFixer) : ControllerBase
{
    private readonly CustomOrderFixer _customOrderFixer = customOrderFixer;

    [HttpPut("RetryCaptureAsync/{orderId}")]
    public async Task<IActionResult> RetryCaptureAsync(string orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return BadRequest(new { error = "orderId is required." });
        }

        try
        {
            var result = await _customOrderFixer.RetryCaptureAsync(orderId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to retry capture", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
