using TrustRent.Domain.Entities;

namespace TrustRent.Application.Common;

public interface ITrustRentDataStore
{
    ICollection<Property> Properties { get; }
    ICollection<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
