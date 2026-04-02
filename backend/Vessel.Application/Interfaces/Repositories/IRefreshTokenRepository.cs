using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken token);
    Task UpdateAsync(RefreshToken token);
    Task DeleteByUserIdAsync(Guid userId);
}
