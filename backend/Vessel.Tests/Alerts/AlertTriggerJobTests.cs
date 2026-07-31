using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Vessel.API.BackgroundJobs;
using Vessel.API.Hubs;
using Vessel.Core.Entities;
using Vessel.Core.Enums;
using Vessel.Infrastructure.Data;
using Xunit;
using FluentAssertions;

namespace Vessel.Tests.Alerts;

public class AlertTriggerJobTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
    private readonly Mock<IHubContext<RateAlertHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockClientProxy;

    public AlertTriggerJobTests()
    {
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockHubContext = new Mock<IHubContext<RateAlertHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<ISingleClientProxy>();

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);
    }

    [Fact]
    public async Task RunAsync_BelowThresholdAlert_ShouldTriggerWhenPriceIsLower()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbOptions);
        var areaId = Guid.NewGuid();
        var consumerId = Guid.NewGuid();
        var rateId = Guid.NewGuid();

        context.PriceAlerts.Add(new PriceAlert
        {
            Id = Guid.NewGuid(),
            AreaId = areaId,
            ConsumerId = consumerId,
            ThresholdTotalPrice = 1000m,
            TargetVolumeInGallons = 100,
            Direction = AlertDirection.BelowOrEqual,
            IsActive = true
        });

        // Current price per gallon = 9.00 -> Total = 900 (Below 1000)
        context.ProviderRates.Add(new ProviderRate
        {
            Id = rateId,
            AreaId = areaId,
            PricePerGallon = 9.00m,
            EffectiveTo = null
        });

        await context.SaveChangesAsync();

        var job = new AlertTriggerJob(context, _mockHubContext.Object);

        // Act
        await job.RunAsync();

        // Assert
        _mockClients.Verify(x => x.User(consumerId.ToString()), Times.Once);
        _mockClientProxy.Verify(x => x.SendCoreAsync("AlertTriggered", It.IsAny<object[]>(), default), Times.Once);
        
        var alert = await context.PriceAlerts.FirstAsync();
        alert.LastTriggeredRateId.Should().Be(rateId);
    }

    [Fact]
    public async Task RunAsync_AboveThresholdAlert_ShouldTriggerWhenPriceIsHigher()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbOptions);
        var areaId = Guid.NewGuid();
        var consumerId = Guid.NewGuid();

        context.PriceAlerts.Add(new PriceAlert
        {
            Id = Guid.NewGuid(),
            AreaId = areaId,
            ConsumerId = consumerId,
            ThresholdTotalPrice = 1000m,
            TargetVolumeInGallons = 100,
            Direction = AlertDirection.AboveOrEqual,
            IsActive = true
        });

        // Current price = 11.00 -> Total = 1100 (Above 1000)
        context.ProviderRates.Add(new ProviderRate
        {
            Id = Guid.NewGuid(),
            AreaId = areaId,
            PricePerGallon = 11.00m,
            EffectiveTo = null
        });

        await context.SaveChangesAsync();

        var job = new AlertTriggerJob(context, _mockHubContext.Object);

        // Act
        await job.RunAsync();

        // Assert
        _mockClients.Verify(x => x.User(consumerId.ToString()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_SameRate_ShouldNotTriggerDuplicateNotification()
    {
        // Arrange
        using var context = new ApplicationDbContext(_dbOptions);
        var areaId = Guid.NewGuid();
        var consumerId = Guid.NewGuid();
        var rateId = Guid.NewGuid();

        context.PriceAlerts.Add(new PriceAlert
        {
            Id = Guid.NewGuid(),
            AreaId = areaId,
            ConsumerId = consumerId,
            ThresholdTotalPrice = 1000m,
            TargetVolumeInGallons = 100,
            Direction = AlertDirection.BelowOrEqual,
            IsActive = true,
            LastTriggeredRateId = rateId // Already triggered for this rate
        });

        context.ProviderRates.Add(new ProviderRate
        {
            Id = rateId,
            AreaId = areaId,
            PricePerGallon = 5.00m,
            EffectiveTo = null
        });

        await context.SaveChangesAsync();

        var job = new AlertTriggerJob(context, _mockHubContext.Object);

        // Act
        await job.RunAsync();

        // Assert
        _mockClients.Verify(x => x.User(It.IsAny<string>()), Times.Never);
    }
}
