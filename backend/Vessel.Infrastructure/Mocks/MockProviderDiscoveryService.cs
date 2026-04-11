using Vessel.Application.DTOs.Providers;
using Vessel.Application.Interfaces.Providers;

namespace Vessel.Infrastructure.Mocks;

public class MockProviderDiscoveryService : IProviderDiscoveryService
{
    public Task<List<ProviderSearchResultDto>> SearchProvidersAsync(SearchProvidersQueryDto query)
    {
        // Mocking a handful of providers within approximately the requested radius
        var results = new List<ProviderSearchResultDto>
        {
            new ProviderSearchResultDto
            {
                ProviderId = Guid.NewGuid(),
                CompanyName = "Premium Petro",
                AreaId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AreaName = "F-8",
                City = "Islamabad",
                DistanceKm = 1.2,
                CurrentPricePerGallon = 3.45m
            },
            new ProviderSearchResultDto
            {
                ProviderId = Guid.NewGuid(),
                CompanyName = "Global Energy Solutions",
                AreaId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                AreaName = "F-8",
                City = "Islamabad",
                DistanceKm = 2.5,
                CurrentPricePerGallon = 3.50m
            },
            new ProviderSearchResultDto
            {
                ProviderId = Guid.NewGuid(),
                CompanyName = "Rapid Fuel",
                AreaId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                AreaName = "DHA Phase 5",
                City = "Lahore",
                DistanceKm = 4.8,
                CurrentPricePerGallon = 3.38m
            }
        };

        // Simple filter based on radius to simulate behavior
        var filteredResults = results.Where(r => r.DistanceKm <= query.RadiusKm).ToList();
        
        return Task.FromResult(filteredResults);
    }
}
