using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Repositories;

public interface IAreaRepository
{
    Task<IEnumerable<Area>> GetAllAsync();
    Task<Area?> GetByIdAsync(Guid id);
}