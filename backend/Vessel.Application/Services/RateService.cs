using Vessel.Application.DTOs.Rates;
using Vessel.Application.Interfaces.Rates;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;

namespace Vessel.Application.Services;

public class RateService : IRateService
{
    private readonly IProviderRateRepository _rateRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IAreaRepository _areaRepository;

    public RateService(IProviderRateRepository rateRepository, IProviderRepository providerRepository, IAreaRepository areaRepository)
    {
        _rateRepository = rateRepository;
        _providerRepository = providerRepository;
        _areaRepository = areaRepository;
    }

    public async Task<List<RateDto>> GetRatesByAreaAsync(Guid areaId)
    {
        var activeRates = await _rateRepository.GetActiveRatesByAreaAsync(areaId);
        var rateDtos = new List<RateDto>();
        
        var area = await _areaRepository.GetByIdAsync(areaId);
        if (area == null) return rateDtos;

        foreach (var rate in activeRates)
        {
            var provider = await _providerRepository.GetByIdAsync(rate.ProviderId);
            if (provider != null)
            {
                rateDtos.Add(new RateDto
                {
                    ProviderId = rate.ProviderId,
                    CompanyName = provider.CompanyName ?? string.Empty,
                    AreaId = area.Id,
                    AreaName = area.Name ?? string.Empty,
                    City = area.City ?? string.Empty,
                    PricePerGallon = rate.PricePerGallon,
                    EffectiveFrom = rate.EffectiveFrom
                });
            }
        }
        return rateDtos;
    }

    public async Task<List<RateHistoryDto>> GetRateHistoryAsync(Guid areaId, Guid providerId)
    {
        var history = await _rateRepository.GetHistoryAsync(providerId, areaId);
        return history.Select(h => new RateHistoryDto
        {
            PricePerGallon = h.PricePerGallon,
            EffectiveFrom = h.EffectiveFrom,
            EffectiveTo = h.EffectiveTo
        }).ToList();
    }

    public async Task<RateDto> CreateRateAsync(Guid providerId, CreateRateDto request)
    {
        var area = await _areaRepository.GetByIdAsync(request.AreaId);
        if (area == null) throw new ArgumentException("Invalid Area Id.");
        
        var provider = await _providerRepository.GetByIdAsync(providerId);
        if (provider == null) throw new ArgumentException("Invalid Provider Id.");

        var activeRate = await _rateRepository.GetActiveRateAsync(providerId, request.AreaId);
        var now = DateTimeOffset.UtcNow;

        if (activeRate != null)
        {
            activeRate.EffectiveTo = now;
        }

        var newRate = new ProviderRate
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            AreaId = request.AreaId,
            PricePerGallon = request.PricePerGallon,
            EffectiveFrom = now,
            CreatedAt = now
        };

        await _rateRepository.ReplaceRateAsync(activeRate, newRate);

        return new RateDto
        {
            ProviderId = providerId,
            CompanyName = provider.CompanyName ?? string.Empty,
            AreaId = area.Id,
            AreaName = area.Name ?? string.Empty,
            City = area.City ?? string.Empty,
            PricePerGallon = newRate.PricePerGallon,
            EffectiveFrom = newRate.EffectiveFrom
        };
    }
}
