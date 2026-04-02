using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<Booking?> GetByIdempotencyKeyAsync(Guid consumerId, string key);
    Task<IEnumerable<Booking>> GetByConsumerIdAsync(Guid consumerId);
    Task<IEnumerable<Booking>> GetByProviderIdAsync(Guid providerId);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
}
