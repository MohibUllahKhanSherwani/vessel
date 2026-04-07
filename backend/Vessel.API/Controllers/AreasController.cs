using Microsoft.AspNetCore.Mvc;
using Vessel.Application.DTOs.Areas;
using Vessel.Application.Interfaces.Areas;

namespace Vessel.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Tags("Areas")]
public class AreasController : ControllerBase
{
    private readonly IAreaService _areaService;

    public AreasController(IAreaService areaService)
    {
        _areaService = areaService;
    }

    /// <summary>
    /// Gets all active areas.
    /// </summary>
    /// <returns>A list of areas.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<AreaDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var areas = await _areaService.GetAllAreasAsync();
        return Ok(areas);
    }
}
