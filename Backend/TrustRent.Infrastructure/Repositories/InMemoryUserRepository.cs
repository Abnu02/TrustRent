using TrustRent.Domain.Entities;
using TrustRent.Domain.Repositories;

namespace TrustRent.Infrastructure.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private static readonly List<User> Users =
    [
        new User
        {
            Id = Guid.Parse("8c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f"),
            FullName = "Abreham Bekele",
            Email = "abreham@example.com",
            PhoneNumber = "0912345678",
            Role = "Landlord",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        },
        new User
        {
            Id = Guid.Parse("11112222-3333-4444-5555-666677778888"),
            FullName = "Sara Hailu",
            Email = "sara@example.com",
            PhoneNumber = "0923456789",
            Role = "Tenant",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        }
    ];

    public Task<User?> GetByIdAsync(Guid id)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<IEnumerable<User>> GetPendingLandlordsAsync()
    {
        var pending = Users.Where(u => u.Role == "Landlord" && !u.IsVerified);
        return Task.FromResult<IEnumerable<User>>(pending.ToList());
    }

    public Task<User> AddAsync(User user)
    {
        Users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user)
    {
        var existing = Users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            Users.Remove(existing);
            Users.Add(user);
        }
        return Task.CompletedTask;
    }
}
