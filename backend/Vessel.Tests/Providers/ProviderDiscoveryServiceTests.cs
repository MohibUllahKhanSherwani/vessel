using FluentAssertions;
using Moq;
using Vessel.Application.DTOs.Providers;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Application.Services;
using Vessel.Core.Entities;
using Xunit;

namespace Vessel.Tests.Providers;

public class ProviderDiscoveryServiceTests
{
    private readonly Mock<IAreaRepository> _areaRepositoryMock;
    private readonly Mock<IProviderRateRepository> _providerRateRepositoryMock;
    private readonly ProviderDiscoveryService _service;

    public ProviderDiscoveryServiceTests()
    {
        _areaRepositoryMock = new Mock<IAreaRepository>();
        _providerRateRepositoryMock = new Mock<IProviderRateRepository>();
        _service = new ProviderDiscoveryService(_areaRepositoryMock.Object, _providerRateRepositoryMock.Object);
    }

    [Fact]
    public async Task SearchProvidersAsync_ShouldExcludeProvidersOutsideRadius()
    {
        // Arrange
        var query = new SearchProvidersQueryDto { Lat = 0, Lon = 0, RadiusKm = 100 };
        
        var area1 = new Area { Id = Guid.NewGuid(), Latitude = 0.5, Longitude = 0.5, Name = "Nearby" }; // Approx 78km
        var area2 = new Area { Id = Guid.NewGuid(), Latitude = 2.0, Longitude = 2.0, Name = "Far" };    // Approx 314km
        
        _areaRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Area> { area1, area2 });
        
        _providerRateRepositoryMock.Setup(repo => repo.SearchActiveRatesAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<ProviderSearchResultDto> 
            { 
                new ProviderSearchResultDto { AreaId = area1.Id, ProviderId = Guid.NewGuid(), CompanyName = "Nearby Provider" }
            });

        // Act
        var results = await _service.SearchProvidersAsync(query);

        // Assert
        results.Should().HaveCount(1);
        results.First().CompanyName.Should().Be("Nearby Provider");
        _providerRateRepositoryMock.Verify(repo => repo.SearchActiveRatesAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(area1.Id) && !ids.Contains(area2.Id))), Times.Once);
    }

    [Fact]
    public async Task SearchProvidersAsync_ShouldSortByDistanceThenPrice()
    {
        // Arrange
        var query = new SearchProvidersQueryDto { Lat = 0, Lon = 0, RadiusKm = 500 };
        
        var area1 = new Area { Id = Guid.NewGuid(), Latitude = 1.0, Longitude = 1.0, Name = "Area 1" }; // Distance X
        var area2 = new Area { Id = Guid.NewGuid(), Latitude = 0.5, Longitude = 0.5, Name = "Area 2" }; // Distance Y (Y < X)
        
        _areaRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Area> { area1, area2 });
        
        _providerRateRepositoryMock.Setup(repo => repo.SearchActiveRatesAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<ProviderSearchResultDto> 
            { 
                new ProviderSearchResultDto { AreaId = area1.Id, CurrentPricePerGallon = 3.00m, CompanyName = "A1_P1" },
                new ProviderSearchResultDto { AreaId = area2.Id, CurrentPricePerGallon = 4.00m, CompanyName = "A2_P1" },
                new ProviderSearchResultDto { AreaId = area2.Id, CurrentPricePerGallon = 3.50m, CompanyName = "A2_P2" }
            });

        // Act
        var results = await _service.SearchProvidersAsync(query);

        // Assert
        // Expected order:
        // 1. A2_P2 (Distance Y, Price 3.50)
        // 2. A2_P1 (Distance Y, Price 4.00)
        // 3. A1_P1 (Distance X, Price 3.00)
        
        results.Should().HaveCount(3);
        results[0].CompanyName.Should().Be("A2_P2");
        results[1].CompanyName.Should().Be("A2_P1");
        results[2].CompanyName.Should().Be("A1_P1");
    }
}
