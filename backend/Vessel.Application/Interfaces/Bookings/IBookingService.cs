using Vessel.Application.DTOs.Bookings;
using Vessel.Core.Enums;

namespace Vessel.Application.Interfaces.Bookings;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(Guid consumerId, CreateBookingDto dto, string idempotencyKey);
    Task<IEnumerable<BookingResponseDto>> GetConsumerBookingsAsync(Guid consumerId);
    Task<IEnumerable<BookingResponseDto>> GetProviderRequestsAsync(Guid providerId);
    Task<BookingResponseDto> UpdateStatusAsync(Guid providerId, Guid bookingId, BookingStatus status);
}
