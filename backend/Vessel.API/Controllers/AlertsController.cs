using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vessel.Application.DTOs.Alerts;
using Vessel.Application.Interfaces.Alerts;

namespace Vessel.API.Controllers;

/// <summary>
/// Manage price alerts for consumers.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("Price Alerts")]
[Authorize(Policy = "ConsumerOnly")]
public class AlertsController : ControllerBase
{
    private readonly IPriceAlertService _alertService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlertsController"/> class.
    /// </summary>
    /// <param name="alertService">The price alert service.</param>
    public AlertsController(IPriceAlertService alertService)
    {
        _alertService = alertService;
    }

    /// <summary>
    /// Creates a new price alert for the authenticated consumer.
    /// </summary>
    /// <param name="dto">The alert details.</param>
    /// <returns>The created alert.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PriceAlertDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<PriceAlertDto>> Create([FromBody] CreatePriceAlertDto dto)
    {
        var userId = GetUserId();
        try
        {
            var alert = await _alertService.CreateAlertAsync(userId, dto);
            return CreatedAtAction(nameof(GetMyAlerts), new { }, alert);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets all active price alerts for the authenticated consumer.
    /// </summary>
    /// <returns>A list of price alerts.</returns>
    [HttpGet("my-alerts")]
    [ProducesResponseType(typeof(IEnumerable<PriceAlertDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<IEnumerable<PriceAlertDto>>> GetMyAlerts()
    {
        var userId = GetUserId();
        var alerts = await _alertService.GetConsumerAlertsAsync(userId);
        return Ok(alerts);
    }

    /// <summary>
    /// Updates an existing price alert.
    /// </summary>
    /// <param name="id">The unique identifier of the alert.</param>
    /// <param name="dto">The updated alert details.</param>
    /// <returns>No content if successful.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriceAlertDto dto)
    {
        var userId = GetUserId();
        try
        {
            await _alertService.UpdateAlertAsync(userId, id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Deletes a price alert.
    /// </summary>
    /// <param name="id">The unique identifier of the alert to delete.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        try
        {
            await _alertService.DeleteAlertAsync(userId, id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User not found in token.");
        }
        return userId;
    }
}
