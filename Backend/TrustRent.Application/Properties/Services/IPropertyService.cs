using PropertyCreateRequest = TrustRent.Application.Properties.DTOs.CreatePropertyRequest;
using TrustRent.Application.Properties.DTOs;

namespace TrustRent.Application.Properties.Services;

public interface IPropertyService
{
    Task<PropertyDto> CreatePropertyAsync(
        Guid landlordId,
        PropertyCreateRequest request,
        CancellationToken cancellationToken);

    Task<PagedResult<PropertyDto>> GetByLandlordAsync(
        Guid landlordId,
        GetMyPropertiesQuery query,
        CancellationToken cancellationToken);

    Task<PropertyDto?> GetByIdAsync(
        Guid landlordId,
        Guid propertyId,
        CancellationToken cancellationToken);

    Task<PropertyDto?> UpdatePropertyAsync(
        Guid landlordId,
        Guid propertyId,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken);
}