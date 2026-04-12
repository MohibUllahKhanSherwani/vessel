using Moq;
using FluentAssertions;
using Vessel.Application.DTOs.Bookings;
using Vessel.Application.Interfaces.Caching;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Application.Services;
using Vessel.Core.Entities;
using Vessel.Core.Enums;

namespace Vessel.Tests.Bookings;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepoMock;
    private readonly Mock<IProviderRateRepository> _rateRepoMock;
    private readonly Mock<IAreaRepository> _areaRepoMock;
    private readonly Mock<IProviderRepository> _providerRepoMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _bookingRepoMock = new Mock<IBookingRepository>();
        _rateRepoMock = new Mock<IProviderRateRepository>();
        _areaRepoMock = new Mock<IAreaRepository>();
        _providerRepoMock = new Mock<IProviderRepository>();
        _cacheMock = new Mock<ICacheService>();

        _service = new BookingService(
            _bookingRepoMock.Object,
            _rateRepoMock.Object,
            _areaRepoMock.Object,
            _providerRepoMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_WithSameKeyAndPayload_ReturnsOriginalBooking()
    {
        // Arrange
        var consumerId = Guid.NewGuid();
        var key = "test-key";
        var dto = new CreateBookingDto 
        { 
            ProviderId = Guid.NewGuid(), 
            AreaId = Guid.NewGuid(), 
            VolumeInGallons = 100 
        };
        
        var existingBooking = new Booking
        {
            Id = Guid.NewGuid(),
            ConsumerId = consumerId,
            IdempotencyKey = key,
            ProviderId = dto.ProviderId,
            AreaId = dto.AreaId,
            VolumeInGallons = (int)dto.VolumeInGallons
        };

        _cacheMock.Setup(m => m.GetAsync<BookingResponseDto>(It.IsAny<string>()))
            .ReturnsAsync((BookingResponseDto?)null);
            
        _bookingRepoMock.Setup(m => m.GetByIdempotencyKeyAsync(consumerId, key))
            .ReturnsAsync(existingBooking);

        // Act
        var result = await _service.CreateBookingAsync(consumerId, dto, key);

        // Assert
        result.Id.Should().Be(existingBooking.Id);
        _bookingRepoMock.Verify(m => m.AddAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CreateBookingAsync_WithSameKeyDifferentPayload_ThrowsInvalidOperationException()
    {
        // Arrange
        var consumerId = Guid.NewGuid();
        var key = "test-key";
        var dto = new CreateBookingDto 
        { 
            ProviderId = Guid.NewGuid(), 
            AreaId = Guid.NewGuid(), 
            VolumeInGallons = 200 // Different volume
        };
        
        var existingBooking = new Booking
        {
            Id = Guid.NewGuid(),
            ConsumerId = consumerId,
            IdempotencyKey = key,
            ProviderId = dto.ProviderId,
            AreaId = dto.AreaId,
            VolumeInGallons = 100 // Original volume
        };

        _cacheMock.Setup(m => m.GetAsync<BookingResponseDto>(It.IsAny<string>()))
            .ReturnsAsync((BookingResponseDto?)null);
            
        _bookingRepoMock.Setup(m => m.GetByIdempotencyKeyAsync(consumerId, key))
            .ReturnsAsync(existingBooking);

        // Act
        var act = () => _service.CreateBookingAsync(consumerId, dto, key);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*A different booking request was already processed with this key.*");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenProviderDoesNotOwnBooking_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var otherProviderId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        
        var booking = new Booking
        {
            Id = bookingId,
            ProviderId = otherProviderId,
            Status = BookingStatus.Pending
        };

        _bookingRepoMock.Setup(m => m.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act
        var act = () => _service.UpdateStatusAsync(providerId, bookingId, BookingStatus.Confirmed);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_WithInvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        
        var booking = new Booking
        {
            Id = bookingId,
            ProviderId = providerId,
            Status = BookingStatus.Cancelled // Already cancelled
        };

        _bookingRepoMock.Setup(m => m.GetByIdAsync(bookingId))
            .ReturnsAsync(booking);

        // Act
        var act = () => _service.UpdateStatusAsync(providerId, bookingId, BookingStatus.Confirmed);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid status transition*");
    }
}
