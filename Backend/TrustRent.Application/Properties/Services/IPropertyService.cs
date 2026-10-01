using TrustRent.Application.Properties.DTOs;

namespace TrustRent.Application.Properties.Services;

public interface IPropertyService
{
    Task<PropertyDto> CreatePropertyAsync(
        Guid landlordId,
        CreatePropertyRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PropertyDto>> GetByLandlordAsync(
        Guid landlordId,
        CancellationToken cancellationToken);

    Task<PropertyDto?> UpdatePropertyAsync(
        Guid landlordId,
        Guid propertyId,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken);
}