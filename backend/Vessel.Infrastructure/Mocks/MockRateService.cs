using Vessel.Application.DTOs.Rates;
using Vessel.Application.Interfaces.Rates;

namespace Vessel.Infrastructure.Mocks;

public class MockRateService : IRateService
{
    public Task<List<RateDto>> GetRatesByAreaAsync(Guid areaId)
    {
        return Task.FromResult(new List<RateDto>
        {
            new RateDto 
            { 
                ProviderId = Guid.NewGuid(), 
                CompanyName = "Mock Provider", 
                AreaId = areaId, 
                AreaName = "Mock Area", 
                City = "Mock City", 
                PricePerGallon = 2.50m, 
                EffectiveFrom = DateTime.UtcNow.AddDays(-1) 
            }
        });
    }

    public Task<List<RateHistoryDto>> GetRateHistoryAsync(Guid areaId, Guid providerId)
    {
        return Task.FromResult(new List<RateHistoryDto>
        {
            new RateHistoryDto { PricePerGallon = 2.45m, EffectiveFrom = DateTime.UtcNow.AddDays(-10), EffectiveTo = DateTime.UtcNow.AddDays(-1) },
            new RateHistoryDto { PricePerGallon = 2.50m, EffectiveFrom = DateTime.UtcNow.AddDays(-1), EffectiveTo = null }
        });
    }

    public Task<RateDto> CreateRateAsync(Guid providerId, CreateRateDto request)
    {
        return Task.FromResult(new RateDto
        {
            ProviderId = providerId,
            CompanyName = "Current Provider",
            AreaId = request.AreaId,
            AreaName = "New Area",
            City = "New City",
            PricePerGallon = request.PricePerGallon,
            EffectiveFrom = DateTime.UtcNow
        });
    }
}
