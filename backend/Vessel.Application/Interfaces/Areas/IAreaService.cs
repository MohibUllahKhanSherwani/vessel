using Vessel.Application.DTOs.Areas;

namespace Vessel.Application.Interfaces.Areas;

public interface IAreaService
{
    Task<List<AreaDto>> GetAllAreasAsync();
}
