using Vessel.Core.Entities;

namespace Vessel.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
    
}