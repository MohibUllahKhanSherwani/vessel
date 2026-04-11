using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Vessel.API.Hubs;
using Vessel.Application.DTOs.Rates;
using Vessel.Application.Interfaces.Rates;

namespace Vessel.API.Controllers;

/// <summary>
/// Controller for managing and retrieving fuel rates.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("Rates")]
public class RatesController : ControllerBase
{
    private readonly IRateService _rateService;
    private readonly IHubContext<RateAlertHub> _hubContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="RatesController"/> class.
    /// </summary>
    /// <param name="rateService">The rate service.</param>
    /// <param name="hubContext">The hub context for signalr notifications.</param>
    public RatesController(IRateService rateService, IHubContext<RateAlertHub> hubContext)
    {
        _rateService = rateService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gets active rates for a specific area.
    /// </summary>
    /// <param name="areaId">The ID of the area.</param>
    /// <returns>A list of rates.</returns>
    [HttpGet("{areaId:guid}")]
    [ProducesResponseType(typeof(List<RateDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByArea(Guid areaId)
    {
        var rates = await _rateService.GetRatesByAreaAsync(areaId);
        if (!rates.Any())
        {
            return NotFound();
        }
        return Ok(rates);
    }

    /// <summary>
    /// Gets the historically effective rates for a specific area and provider.
    /// </summary>
    /// <param name="areaId">The area ID.</param>
    /// <param name="providerId">The provider ID.</param>
    /// <returns>A list of historical rates.</returns>
    [HttpGet("history/{areaId:guid}/{providerId:guid}")]
    [ProducesResponseType(typeof(List<RateHistoryDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetHistory(Guid areaId, Guid providerId)
    {
        var history = await _rateService.GetRateHistoryAsync(areaId, providerId);
        if (!history.Any())
        {
            return NotFound();
        }
        return Ok(history);
    }

    /// <summary>
    /// Creates a new rate for the authenticated provider in a specific area.
    /// </summary>
    /// <param name="request">The rate creation request payload.</param>
    /// <returns>The newly created rate.</returns>
    [HttpPost]
    [Authorize(Policy = "ProviderOnly")]
    [ProducesResponseType(typeof(RateDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateRateDto request)
    {
        // Extract the provider ID from the user's claims. 
        // Note: For now, we assume the user's subject or a specific provider identifier claim is available.
        // During real implementation, the exact claim mapping for ProviderId or UserId to ProviderId will be resolved.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized();
        }
        
        // Mock simplification: We assume the UserId is functionally equivalent for the mock setup.
        // In the real system, you would resolve ProviderId from UserId via a db lookup or custom claim.
        var providerId = userId; 

        var createdRate = await _rateService.CreateRateAsync(providerId, request);
        await _hubContext.Clients.All.SendAsync("RateChanged", createdRate);
        return CreatedAtAction(nameof(GetByArea), new { areaId = createdRate.AreaId }, createdRate);
    }
}
