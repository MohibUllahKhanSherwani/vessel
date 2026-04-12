using Vessel.Application.DTOs.Bookings;
using Vessel.Application.Interfaces.Bookings;
using Vessel.Application.Interfaces.Caching;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Core.Enums;

namespace Vessel.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IProviderRateRepository _rateRepository;
    private readonly IAreaRepository _areaRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly ICacheService _cache;

    public BookingService(
        IBookingRepository bookingRepository,
        IProviderRateRepository rateRepository,
        IAreaRepository areaRepository,
        IProviderRepository providerRepository,
        ICacheService cache)
    {
        _bookingRepository = bookingRepository;
        _rateRepository = rateRepository;
        _areaRepository = areaRepository;
        _providerRepository = providerRepository;
        _cache = cache;
    }

    public async Task<BookingResponseDto> CreateBookingAsync(Guid consumerId, CreateBookingDto dto, string idempotencyKey)
    {
        // 1. Check Cache for cached response first
        var cacheKey = $"booking_idempotency:{consumerId}:{idempotencyKey}";
        var cachedResponse = await _cache.GetAsync<BookingResponseDto>(cacheKey);
        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        // 2. Check Database for existing booking
        var existingBooking = await _bookingRepository.GetByIdempotencyKeyAsync(consumerId, idempotencyKey);
        if (existingBooking != null)
        {
            var response = MapToResponse(existingBooking);
            await _cache.SetAsync(cacheKey, response, TimeSpan.FromHours(24));
            return response;
        }

        // 3. Validation
        await ValidateBookingRequestAsync(dto);

        // 4. Get Active Rate
        var activeRate = await _rateRepository.GetActiveRateAsync(dto.ProviderId, dto.AreaId)
            ?? throw new InvalidOperationException("Provider does not have an active rate for this area.");

        // 5. Calculate Pricing
        var priceSnapshot = activeRate.PricePerGallon;
        var totalPrice = dto.VolumeInGallons * priceSnapshot;

        // 6. Create Booking Entity
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            ConsumerId = consumerId,
            ProviderId = dto.ProviderId,
            AreaId = dto.AreaId,
            VolumeInGallons = (int)dto.VolumeInGallons, // Entity uses int, DTO uses decimal. Plan implied volume could be decimal but entity says int. Let's cast or check entity.
            PricePerGallonSnapshot = priceSnapshot,
            TotalPrice = totalPrice,
            DeliveryAddress = dto.DeliveryAddress,
            Notes = dto.Notes,
            Status = BookingStatus.Pending,
            ScheduledFor = dto.ScheduledFor.ToUniversalTime(),
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // 7. Save to DB
        await _bookingRepository.AddAsync(booking);

        // 8. Cache in Redis (24 hours as per plan)
        var result = MapToResponse(booking);
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(24));

        return result;
    }

    public async Task<IEnumerable<BookingResponseDto>> GetConsumerBookingsAsync(Guid consumerId)
    {
        var bookings = await _bookingRepository.GetByConsumerIdAsync(consumerId);
        return bookings.Select(MapToResponse);
    }

    public async Task<IEnumerable<BookingResponseDto>> GetProviderRequestsAsync(Guid providerId)
    {
        var bookings = await _bookingRepository.GetByProviderIdAsync(providerId);
        return bookings.Select(MapToResponse);
    }

    public async Task<BookingResponseDto> UpdateStatusAsync(Guid providerId, Guid bookingId, BookingStatus status)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.ProviderId != providerId)
        {
            throw new UnauthorizedAccessException("You can only update bookings assigned to you.");
        }

        // Transitions: Pending -> Confirmed, Pending -> Cancelled, Confirmed -> Cancelled
        bool isValid = (booking.Status == BookingStatus.Pending && (status == BookingStatus.Confirmed || status == BookingStatus.Cancelled)) ||
                       (booking.Status == BookingStatus.Confirmed && status == BookingStatus.Cancelled);

        if (!isValid && booking.Status != status)
        {
            throw new InvalidOperationException($"Invalid status transition from {booking.Status} to {status}.");
        }

        booking.Status = status;
        booking.UpdatedAt = DateTimeOffset.UtcNow;

        await _bookingRepository.UpdateAsync(booking);

        return MapToResponse(booking);
    }

    private async Task ValidateBookingRequestAsync(CreateBookingDto dto)
    {
        if (dto.VolumeInGallons <= 0)
            throw new InvalidOperationException("Volume must be positive.");

        if (dto.ScheduledFor < DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Scheduled date cannot be in the past.");

        var areaExists = await _areaRepository.GetByIdAsync(dto.AreaId) != null;
        if (!areaExists) throw new InvalidOperationException("Area does not exist.");

        var providerExists = await _providerRepository.GetByIdAsync(dto.ProviderId) != null;
        if (!providerExists) throw new InvalidOperationException("Provider does not exist.");
    }

    private BookingResponseDto MapToResponse(Booking b)
    {
        return new BookingResponseDto
        {
            Id = b.Id,
            ConsumerId = b.ConsumerId,
            ProviderId = b.ProviderId,
            AreaId = b.AreaId,
            VolumeInGallons = b.VolumeInGallons,
            PricePerGallonSnapshot = b.PricePerGallonSnapshot,
            TotalPrice = b.TotalPrice,
            DeliveryAddress = b.DeliveryAddress,
            Notes = b.Notes,
            Status = b.Status,
            ScheduledFor = b.ScheduledFor,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };
    }
}
