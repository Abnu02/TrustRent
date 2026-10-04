using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;

namespace TrustRent.Application.Abstractions.Persistence;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<Property>> GetByLandlordIdAsync(
        Guid landlordId,
        CancellationToken cancellationToken);

    Task<List<Property>> GetPendingAsync(
        CancellationToken cancellationToken);

    Task<List<Property>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<List<Property>> GetPublicAsync(
        string? location,
        decimal? minRent,
        decimal? maxRent,
        PropertyType? propertyType,
        int? bedrooms,
        int? bathrooms,
        CancellationToken cancellationToken);

    Task AddAsync(
        Property property,
        CancellationToken cancellationToken);

    void Update(Property property);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}