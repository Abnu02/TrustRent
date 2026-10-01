using TrustRent.Domain.Entities;

namespace TrustRent.Application.Properties.Interfaces;

public interface IPropertyRepository
{
    Task AddAsync(Property property, CancellationToken cancellationToken);
    Task<List<Property>> GetByLandlordAsync(Guid landlordId, CancellationToken cancellationToken);
    Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken);
    Task UpdateAsync(Property property, CancellationToken cancellationToken);
}