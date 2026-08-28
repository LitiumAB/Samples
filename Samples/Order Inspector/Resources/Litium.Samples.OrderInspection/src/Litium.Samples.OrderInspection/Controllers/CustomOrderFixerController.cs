using Litium.Samples.OrderInspection.Litium.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

[ApiController]
[Route("[controller]")]
public class CustomOrderFixerController(CustomOrderFixer customOrderFixer) : ControllerBase
{
    private readonly CustomOrderFixer _customOrderFixer = customOrderFixer;

    [HttpPut("RetryCaptureAsync")]
    public async Task<IActionResult> RetryCaptureAsync(DateTimeOffset startDate, DateTimeOffset endDate, string? orderTag, CancellationToken cancellationToken)
    {
        if (startDate > endDate)
        {
            return BadRequest(new { error = "startDate must be less than or equal to endDate." });
        }

        try
        {
            var result = await _customOrderFixer.RetryCaptureAsync(startDate, endDate, orderTag, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to retry capture", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpPut("RetryCaptureOrdersAsync")]
    public async Task<IActionResult> RetryCaptureOrdersAsync(string commaSeperatedOrderIds, CancellationToken cancellationToken)
    { 
        try
        {
            var result = await _customOrderFixer.RetryCaptureAsync(commaSeperatedOrderIds, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to retry capture", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
