using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Vessel.Application.DTOs.Rates;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Application.Services;
using Vessel.Core.Entities;
using Xunit;

namespace Vessel.Tests.Rates;

public class RateServiceTests
{
    private readonly Mock<IProviderRateRepository> _mockRateRepository;
    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<IAreaRepository> _mockAreaRepository;
    private readonly RateService _sut;

    public RateServiceTests()
    {
        _mockRateRepository = new Mock<IProviderRateRepository>();
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockAreaRepository = new Mock<IAreaRepository>();

        _sut = new RateService(
            _mockRateRepository.Object,
            _mockProviderRepository.Object,
            _mockAreaRepository.Object);
    }

    [Fact]
    public async Task CreateRateAsync_PostingNewRate_ShouldExpireOldActiveRate()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var request = new CreateRateDto { AreaId = areaId, PricePerGallon = 2.50m };

        var oldRate = new ProviderRate
        {
            Id = Guid.NewGuid(),
            ProviderId = providerId,
            AreaId = areaId,
            PricePerGallon = 2.45m,
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _mockAreaRepository.Setup(x => x.GetByIdAsync(areaId))
            .ReturnsAsync(new Area { Id = areaId, Name = "Test" });
            
        _mockProviderRepository.Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync(new Provider { Id = providerId, CompanyName = "Test Co" });

        _mockRateRepository.Setup(x => x.GetActiveRateAsync(providerId, areaId))
            .ReturnsAsync(oldRate);

        ProviderRate? capturedOldRate = null;
        ProviderRate? capturedNewRate = null;

        _mockRateRepository.Setup(x => x.ReplaceRateAsync(It.IsAny<ProviderRate?>(), It.IsAny<ProviderRate>()))
            .Callback<ProviderRate?, ProviderRate>((o, n) => 
            { 
                capturedOldRate = o; 
                capturedNewRate = n; 
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateRateAsync(providerId, request);

        // Assert
        capturedOldRate.Should().NotBeNull();
        capturedOldRate!.EffectiveTo.Should().NotBeNull(); // Old rate was expired
        capturedNewRate.Should().NotBeNull();
        capturedNewRate!.PricePerGallon.Should().Be(2.50m);
    }

    [Fact]
    public async Task GetRateHistoryAsync_ReturnsOlderRatesAndLatestRatesCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        
        var historyData = new List<ProviderRate>
        {
            new ProviderRate { PricePerGallon = 2.60m, EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-5), EffectiveTo = DateTimeOffset.UtcNow.AddDays(-2) },
            new ProviderRate { PricePerGallon = 2.50m, EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-2), EffectiveTo = null }
        };

        _mockRateRepository.Setup(x => x.GetHistoryAsync(providerId, areaId))
            .ReturnsAsync(historyData);

        // Act
        var result = await _sut.GetRateHistoryAsync(areaId, providerId);

        // Assert
        result.Should().HaveCount(2);
        result.First().EffectiveTo.Should().NotBeNull();
        result.Last().EffectiveTo.Should().BeNull();
    }

    [Fact]
    public async Task GetRatesByAreaAsync_PublicRateQueriesReturnOnlyActiveRates()
    {
        // Arrange
        var areaId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        
        var mockArea = new Area { Id = areaId, Name = "Test Area" };
        var mockProvider = new Provider { Id = providerId, CompanyName = "Test Co" };
        
        var activeRates = new List<ProviderRate>
        {
            new ProviderRate { ProviderId = providerId, AreaId = areaId, PricePerGallon = 2.50m, EffectiveTo = null }
        };

        _mockRateRepository.Setup(x => x.GetActiveRatesByAreaAsync(areaId))
            .ReturnsAsync(activeRates);

        _mockAreaRepository.Setup(x => x.GetByIdAsync(areaId))
            .ReturnsAsync(mockArea);

        _mockProviderRepository.Setup(x => x.GetByIdAsync(providerId))
            .ReturnsAsync(mockProvider);

        // Act
        var result = await _sut.GetRatesByAreaAsync(areaId);

        // Assert
        result.Should().HaveCount(1);
        result.First().PricePerGallon.Should().Be(2.50m);
    }
}
