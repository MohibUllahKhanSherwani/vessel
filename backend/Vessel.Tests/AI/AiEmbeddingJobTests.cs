using Moq;
using Vessel.Application.Interfaces.AI;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Application.Services.AI;
using Xunit;
using FluentAssertions;

namespace Vessel.Tests.AI;

public class AiEmbeddingJobTests
{
    private readonly Mock<IRateEmbeddingRepository> _embeddingRepoMock;
    private readonly Mock<IEmbeddingGeneratorService> _generatorMock;
    private readonly Mock<IProviderRateRepository> _rateRepoMock;
    private readonly Mock<IProviderRepository> _providerRepoMock;
    private readonly Mock<IAreaRepository> _areaRepoMock;
    private readonly Mock<IPriceAlertRepository> _alertRepoMock;
    private readonly AiEmbeddingJob _job;

    public AiEmbeddingJobTests()
    {
        _embeddingRepoMock = new Mock<IRateEmbeddingRepository>();
        _generatorMock = new Mock<IEmbeddingGeneratorService>();
        _rateRepoMock = new Mock<IProviderRateRepository>();
        _providerRepoMock = new Mock<IProviderRepository>();
        _areaRepoMock = new Mock<IAreaRepository>();
        _alertRepoMock = new Mock<IPriceAlertRepository>();

        _job = new AiEmbeddingJob(
            _embeddingRepoMock.Object,
            _generatorMock.Object,
            _rateRepoMock.Object,
            _providerRepoMock.Object,
            _areaRepoMock.Object,
            _alertRepoMock.Object);
    }

    [Fact]
    public async Task ProcessRateChangeAsync_ShouldCreateEmbeddingWithCorrectText()
    {
        // Arrange
        var rateId = Guid.NewGuid();
        var rate = new ProviderRate 
        { 
            Id = rateId, 
            ProviderId = Guid.NewGuid(), 
            AreaId = Guid.NewGuid(), 
            PricePerGallon = 2.5m, 
            EffectiveFrom = DateTimeOffset.UtcNow 
        };
        var provider = new Provider { CompanyName = "Test Provider" };
        var area = new Area { Name = "Test Area", City = "Test City" };

        _rateRepoMock.Setup(x => x.GetByIdAsync(rateId)).ReturnsAsync(rate);
        _providerRepoMock.Setup(x => x.GetByIdAsync(rate.ProviderId)).ReturnsAsync(provider);
        _areaRepoMock.Setup(x => x.GetByIdAsync(rate.AreaId)).ReturnsAsync(area);
        _generatorMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>())).ReturnsAsync(new float[768]);

        // Act
        await _job.ProcessRateChangeAsync(rateId);

        // Assert
        _embeddingRepoMock.Verify(x => x.AddAsync(It.Is<RateEmbedding>(e => 
            e.ContentText.Contains("Test Provider") && 
            e.ContentText.Contains("Test Area") && 
            e.ContentText.Contains("2.5"))), Times.Once);
    }

    [Fact]
    public async Task ProcessAlertTriggerAsync_ShouldCreateEmbeddingWithCorrectText()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var rateId = Guid.NewGuid();
        var alert = new PriceAlert 
        { 
            Id = alertId, 
            AreaId = Guid.NewGuid(), 
            ThresholdTotalPrice = 500, 
            TargetVolumeInGallons = 200 
        };
        var rate = new ProviderRate { Id = rateId, PricePerGallon = 2.4m };
        var area = new Area { Name = "Test Area", City = "Test City" };

        _alertRepoMock.Setup(x => x.GetByIdAsync(alertId)).ReturnsAsync(alert);
        _rateRepoMock.Setup(x => x.GetByIdAsync(rateId)).ReturnsAsync(rate);
        _areaRepoMock.Setup(x => x.GetByIdAsync(alert.AreaId)).ReturnsAsync(area);
        _generatorMock.Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>())).ReturnsAsync(new float[768]);

        // Act
        await _job.ProcessAlertTriggerAsync(alertId, rateId);

        // Assert
        _embeddingRepoMock.Verify(x => x.AddAsync(It.Is<RateEmbedding>(e => 
            e.ContentText.Contains("Price alert triggered") && 
            e.ContentText.Contains("500") && 
            e.ContentText.Contains("2.4"))), Times.Once);
    }
}
