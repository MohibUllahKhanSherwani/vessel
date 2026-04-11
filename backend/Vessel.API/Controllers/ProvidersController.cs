using Microsoft.AspNetCore.Mvc;
using Vessel.Application.DTOs.Providers;
using Vessel.Application.Interfaces.Providers;

namespace Vessel.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Providers")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderDiscoveryService _providerDiscoveryService;

    public ProvidersController(IProviderDiscoveryService providerDiscoveryService)
    {
        _providerDiscoveryService = providerDiscoveryService;
    }

    /// <summary>
    /// Searches for fuel providers near the given coordinates.
    /// </summary>
    /// <param name="query">The search criteria including latitude, longitude, and radius.</param>
    /// <returns>A list of providers matching the search criteria.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ProviderSearchResultDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Search([FromQuery] SearchProvidersQueryDto query)
    {
        var results = await _providerDiscoveryService.SearchProvidersAsync(query);
        return Ok(results);
    }
}
