using System.Text.Json;
using Litium.Samples.OrderInspection.Configuration;
using Litium.Samples.OrderInspection.Services;
using Microsoft.AspNetCore.Mvc;

namespace Litium.Samples.OrderInspection.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderInspectorController(
    ILitiumOrderInspectorClient litiumOrderInspectorClient,
    ILitiumAuthenticationService litiumAuthenticationService) : ControllerBase
{
    private readonly ILitiumOrderInspectorClient _litiumOrderInspectorClient = litiumOrderInspectorClient;
    private readonly ILitiumAuthenticationService _litiumAuthenticationService = litiumAuthenticationService;

    [HttpGet("ValidateOrder/{orderId}")]
    public async Task<IActionResult> ValidateOrder(string orderId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return BadRequest(new { error = "orderId is required." });
        }

        try
        {
            var accessToken = await _litiumAuthenticationService.AuthenticateAsync(cancellationToken);
            var endpoint = $"{EndPointRoute.AdminApi}/orders/{Uri.EscapeDataString(orderId)}";
            var response = await _litiumOrderInspectorClient.GetOrderAsync(endpoint, accessToken, cancellationToken);
            if (string.Equals(response.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                var parsed = JsonSerializer.Deserialize<JsonElement>(response.Body);
                return StatusCode(response.StatusCode, parsed);
            }

            return new ContentResult
            {
                StatusCode = response.StatusCode,
                ContentType = response.ContentType ?? "application/json",
                Content = response.Body
            };
        }
        catch (Exception ex)
        {
            return Problem(title: "Failed to validate order", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
