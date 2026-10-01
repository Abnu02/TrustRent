using TrustRent.Domain.Entities;
using TrustRent.Application.Properties.DTOs;

namespace TrustRent.Application.Properties.Interfaces;

public interface IPropertyRepository
{
    Task AddAsync(Property property, CancellationToken cancellationToken);
    Task<PagedResult<PropertyDto>> GetByLandlordAsync(
        Guid landlordId,
        GetMyPropertiesQuery query,
        CancellationToken cancellationToken);
    Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken);
    Task<Property?> GetByIdForUpdateAsync(Guid propertyId, CancellationToken cancellationToken);
    Task UpdateAsync(Property property, CancellationToken cancellationToken);
}