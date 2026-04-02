using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Repositories;

public interface IPriceAlertRepository
{
    Task<PriceAlert?> GetByIdAsync(Guid id);
    Task<IEnumerable<PriceAlert>> GetByConsumerIdAsync(Guid consumerId);
    Task<IEnumerable<PriceAlert>> GetActiveAlertsByAreaAsync(Guid areaId);
    Task AddAsync(PriceAlert alert);
    Task UpdateAsync(PriceAlert alert);
    Task DeleteAsync(Guid id);
}
