using System.Collections.Concurrent;
using System.Text.Json;
using Vessel.Application.DTOs.Bookings;
using Vessel.Application.Interfaces.Bookings;
using Vessel.Core.Enums;

namespace Vessel.Infrastructure.Mocks;

public class MockBookingService : IBookingService
{
    private static readonly ConcurrentDictionary<Guid, BookingResponseDto> _bookings = new();
    private static readonly ConcurrentDictionary<(Guid ConsumerId, string Key), (string Payload, Guid BookingId)> _idempotencyKeys = new();

    public Task<BookingResponseDto> CreateBookingAsync(Guid consumerId, CreateBookingDto dto, string idempotencyKey)
    {
        var payload = JsonSerializer.Serialize(dto);
        var key = (consumerId, idempotencyKey);

        if (_idempotencyKeys.TryGetValue(key, out var existing))
        {
            if (existing.Payload != payload)
            {
                throw new InvalidOperationException("Idempotency key mismatch. A different request was already processed with this key.");
            }

            return Task.FromResult(_bookings[existing.BookingId]);
        }

        var bookingId = Guid.NewGuid();
        var response = new BookingResponseDto
        {
            Id = bookingId,
            ConsumerId = consumerId,
            ProviderId = dto.ProviderId,
            AreaId = dto.AreaId,
            VolumeInGallons = dto.VolumeInGallons,
            PricePerGallonSnapshot = 3.50m, // Mock price
            TotalPrice = dto.VolumeInGallons * 3.50m,
            DeliveryAddress = dto.DeliveryAddress,
            Notes = dto.Notes,
            Status = BookingStatus.Pending,
            ScheduledFor = dto.ScheduledFor,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _bookings[bookingId] = response;
        _idempotencyKeys[key] = (payload, bookingId);

        return Task.FromResult(response);
    }

    public Task<IEnumerable<BookingResponseDto>> GetConsumerBookingsAsync(Guid consumerId)
    {
        return Task.FromResult(_bookings.Values.Where(b => b.ConsumerId == consumerId).OrderByDescending(b => b.CreatedAt).AsEnumerable());
    }

    public Task<IEnumerable<BookingResponseDto>> GetProviderRequestsAsync(Guid providerId)
    {
        return Task.FromResult(_bookings.Values.Where(b => b.ProviderId == providerId).OrderByDescending(b => b.CreatedAt).AsEnumerable());
    }

    public Task<BookingResponseDto> UpdateStatusAsync(Guid providerId, Guid bookingId, BookingStatus status)
    {
        if (!_bookings.TryGetValue(bookingId, out var booking))
        {
            throw new KeyNotFoundException("Booking not found.");
        }

        if (booking.ProviderId != providerId)
        {
            throw new UnauthorizedAccessException("You can only update bookings assigned to you.");
        }

        // Simple validation of transitions as per plan
        // Pending -> Confirmed, Pending -> Cancelled, Confirmed -> Cancelled
        bool isValid = (booking.Status == BookingStatus.Pending && (status == BookingStatus.Confirmed || status == BookingStatus.Cancelled)) ||
                       (booking.Status == BookingStatus.Confirmed && status == BookingStatus.Cancelled);

        if (!isValid && booking.Status != status)
        {
            throw new InvalidOperationException($"Invalid status transition from {booking.Status} to {status}.");
        }

        booking.Status = status;
        booking.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult(booking);
    }
}
