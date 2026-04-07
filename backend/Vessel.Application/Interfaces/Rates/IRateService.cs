using Vessel.Application.DTOs.Rates;

namespace Vessel.Application.Interfaces.Rates;

public interface IRateService
{
    Task<List<RateDto>> GetRatesByAreaAsync(Guid areaId);
    Task<List<RateHistoryDto>> GetRateHistoryAsync(Guid areaId, Guid providerId);
    Task<RateDto> CreateRateAsync(Guid providerId, CreateRateDto request);
}
