using Vessel.Application.DTOs.Providers;
using Vessel.Application.Interfaces.Providers;
using Vessel.Application.Interfaces.Repositories;

namespace Vessel.Application.Services;

public class ProviderDiscoveryService : IProviderDiscoveryService
{
    private readonly IAreaRepository _areaRepository;
    private readonly IProviderRateRepository _providerRateRepository;

    public ProviderDiscoveryService(IAreaRepository areaRepository, IProviderRateRepository providerRateRepository)
    {
        _areaRepository = areaRepository;
        _providerRateRepository = providerRateRepository;
    }

    public async Task<List<ProviderSearchResultDto>> SearchProvidersAsync(SearchProvidersQueryDto query)
    {
        var allAreas = await _areaRepository.GetAllAsync();
        
        var nearbyAreas = allAreas
            .Select(area => new { Area = area, Distance = CalculateDistance(query.Lat, query.Lon, area.Latitude, area.Longitude) })
            .Where(x => x.Distance <= query.RadiusKm)
            .ToList();

        if (!nearbyAreas.Any())
        {
            return new List<ProviderSearchResultDto>();
        }

        var areaIds = nearbyAreas.Select(x => x.Area.Id).ToList();
        var discoveryResults = (await _providerRateRepository.SearchActiveRatesAsync(areaIds)).ToList();

        // Map distances back to discovery results
        foreach (var result in discoveryResults)
        {
            var nearbyArea = nearbyAreas.First(x => x.Area.Id == result.AreaId);
            result.DistanceKm = Math.Round(nearbyArea.Distance, 2);
        }

        // Sort by distance first, then price
        return discoveryResults
            .OrderBy(r => r.DistanceKm)
            .ThenBy(r => r.CurrentPricePerGallon)
            .ToList();
    }
    // Custom Haversine formula to calculate distance between two points on a sphere
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}
