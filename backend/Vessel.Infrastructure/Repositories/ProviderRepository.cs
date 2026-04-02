using Microsoft.EntityFrameworkCore;
using Vessel.Application.Interfaces.Repositories;
using Vessel.Core.Entities;
using Vessel.Infrastructure.Data;

namespace Vessel.Infrastructure.Repositories;

public class ProviderRepository : IProviderRepository
{
    private readonly ApplicationDbContext _context;

    public ProviderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Provider?> GetByIdAsync(Guid id)
    {
        return await _context.Providers.FindAsync(id);
    }

    public async Task<Provider?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task AddAsync(Provider provider)
    {
        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Provider provider)
    {
        _context.Providers.Update(provider);
        await _context.SaveChangesAsync();
    }
}
