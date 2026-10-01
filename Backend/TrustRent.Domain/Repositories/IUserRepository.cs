using TrustRent.Domain.Entities;

namespace TrustRent.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetPendingLandlordsAsync();
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
}
