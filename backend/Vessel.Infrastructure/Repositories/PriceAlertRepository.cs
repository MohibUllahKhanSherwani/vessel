using Microsoft.EntityFrameworkCore;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Infrastructure.Data;

namespace Vessel.Infrastructure.Repositories;

public class PriceAlertRepository : IPriceAlertRepository
{
    private readonly ApplicationDbContext _context;

    public PriceAlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PriceAlert?> GetByIdAsync(Guid id)
    {
        return await _context.PriceAlerts.FindAsync(id);
    }

    public async Task<IEnumerable<PriceAlert>> GetByConsumerIdAsync(Guid consumerId)
    {
        return await _context.PriceAlerts
            .Where(a => a.ConsumerId == consumerId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PriceAlert>> GetActiveAlertsByAreaAsync(Guid areaId)
    {
        return await _context.PriceAlerts
            .Where(a => a.AreaId == areaId && a.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(PriceAlert alert)
    {
        await _context.PriceAlerts.AddAsync(alert);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PriceAlert alert)
    {
        _context.PriceAlerts.Update(alert);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var alert = await _context.PriceAlerts.FindAsync(id);
        if (alert != null)
        {
            _context.PriceAlerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }
}
