using Litium.Samples.OrderInspection.Configuration;
using Litium.Samples.OrderInspection.LitiumApis.Generated;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

/// <summary>
/// Endpoints for Litium Connect ERP operations.
/// </summary>
[ApiController]
[Route("[controller]")]
public class LitiumConnectErpController(ILitiumConnectErpClient litiumConnectErpClient) : ControllerBase
{
    private readonly ILitiumConnectErpClient _litiumConnectErpClient = litiumConnectErpClient;

    /// <summary>
    /// Gets an ERP order from Litium Connect by order id.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The ERP order payload from Litium Connect.</returns>
    [HttpGet("GetOrder/{orderId}")]
    public async Task<IActionResult> GetOrder(string orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return BadRequest(new { error = "orderId is required." });
        }

        try
        {
            var result = await _litiumConnectErpClient.GetOrderAsync(orderId, null, null, cancellationToken);
            return Ok(result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message, detail = ex.Response });
        }
    }
}