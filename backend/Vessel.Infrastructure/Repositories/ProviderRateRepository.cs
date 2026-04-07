using Microsoft.EntityFrameworkCore;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Infrastructure.Data;

namespace Vessel.Infrastructure.Repositories;

public class ProviderRateRepository : IProviderRateRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderRateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProviderRate?> GetActiveRateAsync(Guid providerId, Guid areaId)
    {
        return await _context.ProviderRates
            .FirstOrDefaultAsync(r => r.ProviderId == providerId && r.AreaId == areaId && r.EffectiveTo == null);
    }

    public async Task<IEnumerable<ProviderRate>> GetActiveRatesByAreaAsync(Guid areaId)
    {
        return await _context.ProviderRates
            .Where(r => r.AreaId == areaId && r.EffectiveTo == null)
            .OrderBy(r => r.PricePerGallon)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProviderRate>> GetHistoryAsync(Guid providerId, Guid areaId)
    {
        return await _context.ProviderRates
            .Where(r => r.ProviderId == providerId && r.AreaId == areaId)
            .OrderByDescending(r => r.EffectiveFrom)
            .ToListAsync();
    }

    public async Task AddAsync(ProviderRate rate)
    {
        await _context.ProviderRates.AddAsync(rate);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProviderRate rate)
    {
        _context.ProviderRates.Update(rate);
        await _context.SaveChangesAsync();
    }

    public async Task ReplaceRateAsync(ProviderRate? oldRate, ProviderRate newRate)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (oldRate != null)
            {
                _context.ProviderRates.Update(oldRate);
            }
            await _context.ProviderRates.AddAsync(newRate);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
