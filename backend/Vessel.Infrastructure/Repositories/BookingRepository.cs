using Microsoft.EntityFrameworkCore;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Infrastructure.Data;

namespace Vessel.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings.FindAsync(id);
    }

    public async Task<Booking?> GetByIdempotencyKeyAsync(Guid consumerId, string key)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.ConsumerId == consumerId && b.IdempotencyKey == key);
    }

    public async Task<IEnumerable<Booking>> GetByConsumerIdAsync(Guid consumerId)
    {
        return await _context.Bookings
            .Where(b => b.ConsumerId == consumerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetByProviderIdAsync(Guid providerId)
    {
        return await _context.Bookings
            .Where(b => b.ProviderId == providerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }
}
