using Microsoft.EntityFrameworkCore;
using Vessel.Core.Entities;
using Vessel.Core.Enums;
using Vessel.Infrastructure.Data;
using Vessel.Infrastructure.Services.Analytics;
using FluentAssertions;

namespace Vessel.Tests.Analytics;

public class AdminAnalyticsServiceTests
{
    private ApplicationDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetTopProvidersAsync_ReturnsTopProvidersByBookingCount()
    {
        // Arrange
        using var context = GetContext();
        var provider1Id = Guid.NewGuid();
        var provider2Id = Guid.NewGuid();
        
        context.Providers.AddRange(
            new Provider { Id = provider1Id, CompanyName = "Provider 1" },
            new Provider { Id = provider2Id, CompanyName = "Provider 2" }
        );

        context.Bookings.AddRange(
            new Booking { Id = Guid.NewGuid(), ProviderId = provider1Id, Status = BookingStatus.Confirmed, VolumeInGallons = 100, CreatedAt = DateTimeOffset.UtcNow },
            new Booking { Id = Guid.NewGuid(), ProviderId = provider1Id, Status = BookingStatus.Confirmed, VolumeInGallons = 150, CreatedAt = DateTimeOffset.UtcNow },
            new Booking { Id = Guid.NewGuid(), ProviderId = provider2Id, Status = BookingStatus.Confirmed, VolumeInGallons = 300, CreatedAt = DateTimeOffset.UtcNow },
            new Booking { Id = Guid.NewGuid(), ProviderId = provider2Id, Status = BookingStatus.Pending, VolumeInGallons = 500, CreatedAt = DateTimeOffset.UtcNow }
        );
        
        await context.SaveChangesAsync();

        var service = new AdminAnalyticsService(context);

        // Act
        var result = (await service.GetTopProvidersAsync(5)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].ProviderId.Should().Be(provider1Id);
        result[0].ConfirmedBookingCount.Should().Be(2);
        result[1].ProviderId.Should().Be(provider2Id);
        result[1].ConfirmedBookingCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAveragePricesByCityAsync_ReturnsCorrectAverages()
    {
        // Arrange
        using var context = GetContext();
        var nyArea = new Area { Id = Guid.NewGuid(), City = "New York" };
        var bostonArea = new Area { Id = Guid.NewGuid(), City = "Boston" };
        context.Areas.AddRange(nyArea, bostonArea);

        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();

        context.ProviderRates.AddRange(
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = p1, AreaId = nyArea.Id, PricePerGallon = 3.50m, EffectiveTo = null },
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = p2, AreaId = nyArea.Id, PricePerGallon = 3.70m, EffectiveTo = null },
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = p1, AreaId = bostonArea.Id, PricePerGallon = 4.00m, EffectiveTo = null },
            new ProviderRate { Id = Guid.NewGuid(), ProviderId = p1, AreaId = nyArea.Id, PricePerGallon = 2.00m, EffectiveTo = DateTimeOffset.UtcNow }
        );

        await context.SaveChangesAsync();

        var service = new AdminAnalyticsService(context);

        // Act
        var result = (await service.GetAveragePricesByCityAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.First(r => r.City == "New York").AveragePricePerGallon.Should().Be(3.60m);
        result.First(r => r.City == "Boston").AveragePricePerGallon.Should().Be(4.00m);
    }

    [Fact]
    public async Task GetVolumeTrendsAsync_ReturnsCorrectGroupedCounts()
    {
        // Arrange
        using var context = GetContext();
        
        // Use very distinct dates to avoid any range issues
        var day1 = new DateTimeOffset(2026, 4, 1, 10, 0, 0, TimeSpan.Zero);
        var day2 = new DateTimeOffset(2026, 4, 2, 11, 0, 0, TimeSpan.Zero);

        context.Bookings.AddRange(
            new Booking { Id = Guid.NewGuid(), Status = BookingStatus.Confirmed, VolumeInGallons = 100, CreatedAt = day1 },
            new Booking { Id = Guid.NewGuid(), Status = BookingStatus.Confirmed, VolumeInGallons = 200, CreatedAt = day1 },
            new Booking { Id = Guid.NewGuid(), Status = BookingStatus.Confirmed, VolumeInGallons = 50, CreatedAt = day2 }
        );

        await context.SaveChangesAsync();

        var service = new AdminAnalyticsService(context);

        // Act
        // Set days high enough to include April 1st
        var result = (await service.GetVolumeTrendsAsync(100)).ToList();

        // Assert
        result.Should().Contain(t => t.Date.Date == day1.Date && t.ConfirmedBookingCount == 2);
        result.Should().Contain(t => t.Date.Date == day2.Date && t.ConfirmedBookingCount == 1);
    }

    [Fact]
    public async Task GetPriceTrendsAsync_ReturnsCorrectTrends()
    {
        // Arrange
        using var context = GetContext();
        var areaId = Guid.NewGuid();
        var day1 = new DateTimeOffset(2026, 4, 1, 10, 0, 0, TimeSpan.Zero);
        var day2 = new DateTimeOffset(2026, 4, 2, 11, 0, 0, TimeSpan.Zero);

        context.Bookings.AddRange(
            new Booking { Id = Guid.NewGuid(), AreaId = areaId, PricePerGallonSnapshot = 3.00m, CreatedAt = day1, Status = BookingStatus.Confirmed },
            new Booking { Id = Guid.NewGuid(), AreaId = areaId, PricePerGallonSnapshot = 4.00m, CreatedAt = day2, Status = BookingStatus.Confirmed }
        );

        await context.SaveChangesAsync();

        var service = new AdminAnalyticsService(context);

        // Act
        var result = (await service.GetPriceTrendsAsync(areaId, 100)).ToList();

        // Assert
        result.Should().Contain(r => r.Date.Date == day1.Date && r.AveragePricePerGallon == 3.00m);
        result.Should().Contain(r => r.Date.Date == day2.Date && r.AveragePricePerGallon == 4.00m);
    }
}
