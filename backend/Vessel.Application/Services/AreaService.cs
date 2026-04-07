using Vessel.Application.DTOs.Areas;
using Vessel.Application.Interfaces.Areas;
using Vessel.Application.Interfaces.Repositories;

namespace Vessel.Application.Services;

public class AreaService : IAreaService
{
    private readonly IAreaRepository _areaRepository;

    public AreaService(IAreaRepository areaRepository)
    {
        _areaRepository = areaRepository;
    }

    public async Task<List<AreaDto>> GetAllAreasAsync()
    {
        var areas = await _areaRepository.GetAllAsync();
        return areas.Select(a => new AreaDto
        {
            Id = a.Id,
            City = a.City ?? string.Empty,
            Name = a.Name ?? string.Empty,
            Latitude = a.Latitude,
            Longitude = a.Longitude
        }).ToList();
    }
}
