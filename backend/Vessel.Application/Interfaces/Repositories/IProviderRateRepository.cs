using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Repositories;

public interface IProviderRateRepository
{
    Task<ProviderRate?> GetActiveRateAsync(Guid providerId, Guid areaId);
    Task<IEnumerable<ProviderRate>> GetActiveRatesByAreaAsync(Guid areaId);
    Task<IEnumerable<ProviderRate>> GetHistoryAsync(Guid providerId, Guid areaId);
    Task AddAsync(ProviderRate rate);
    Task UpdateAsync(ProviderRate rate);
    Task ReplaceRateAsync(ProviderRate? oldRate, ProviderRate newRate);
}
