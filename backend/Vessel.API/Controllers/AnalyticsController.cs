using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vessel.Application.DTOs.Analytics;
using Vessel.Application.Interfaces.Analytics;

namespace Vessel.API.Controllers;

/// <summary>
/// Provides analytical data for system administrators.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Tags("Analytics")]
[Authorize(Policy = "AdminOnly")]
public class AnalyticsController : ControllerBase
{
    private readonly IAdminAnalyticsService _analyticsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsController"/> class.
    /// </summary>
    /// <param name="analyticsService">The administrative analytics service.</param>
    public AnalyticsController(IAdminAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Gets the top providers based on confirmed booking volume.
    /// </summary>
    /// <param name="count">The number of providers to retrieve.</param>
    /// <returns>A list of top providers.</returns>
    [HttpGet("top-providers")]
    [ProducesResponseType(typeof(IEnumerable<TopProviderDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<TopProviderDto>>> GetTopProviders([FromQuery] int count = 5)
    {
        var result = await _analyticsService.GetTopProvidersAsync(count);
        return Ok(result);
    }

    /// <summary>
    /// Gets average price per gallon grouped by city.
    /// </summary>
    /// <returns>A list of average prices by city.</returns>
    [HttpGet("average-prices")]
    [ProducesResponseType(typeof(IEnumerable<AveragePriceDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<AveragePriceDto>>> GetAveragePrices()
    {
        var result = await _analyticsService.GetAveragePricesByCityAsync();
        return Ok(result);
    }

    /// <summary>
    /// Gets volume trends over time.
    /// </summary>
    /// <param name="days">The number of days to look back.</param>
    /// <returns>A list of daily volumes.</returns>
    [HttpGet("volume-trends")]
    [ProducesResponseType(typeof(IEnumerable<VolumeTrendDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<VolumeTrendDto>>> GetVolumeTrends([FromQuery] int days = 30)
    {
        var result = await _analyticsService.GetVolumeTrendsAsync(days);
        return Ok(result);
    }

    /// <summary>
    /// Gets price trends over time.
    /// </summary>
    /// <param name="days">The number of days to look back.</param>
    /// <returns>A list of daily average prices.</returns>
    [HttpGet("price-trends")]
    [ProducesResponseType(typeof(IEnumerable<PriceTrendDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<PriceTrendDto>>> GetPriceTrends([FromQuery] int days = 30)
    {
        var result = await _analyticsService.GetPriceTrendsAsync(days);
        return Ok(result);
    }
}
