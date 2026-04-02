using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Repositories;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid id);
    Task<Provider?> GetByUserIdAsync(Guid userId);
    Task AddAsync(Provider provider);
    Task UpdateAsync(Provider provider);
}
