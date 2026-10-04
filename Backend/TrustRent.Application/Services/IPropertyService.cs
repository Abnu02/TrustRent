using TrustRent.Application.DTOs.Properties;

namespace TrustRent.Application.Services;

public interface IPropertyService
{
    Task<PropertyListResponse> CreateAsync(
        Guid landlordId,
        CreatePropertyRequest request,
        CancellationToken cancellationToken);

    Task<List<PropertyListResponse>> GetMyPropertiesAsync(
        Guid landlordId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Guid landlordId,
        Guid propertyId,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken);

    Task ArchiveAsync(
        Guid landlordId,
        Guid propertyId,
        CancellationToken cancellationToken);

    Task<List<PropertyListResponse>> GetPublicAsync(
        string? location,
        decimal? minRent,
        decimal? maxRent,
        string? propertyType,
        int? bedrooms,
        int? bathrooms,
        CancellationToken cancellationToken);

    Task<PropertyDetailsResponse?> GetDetailsAsync(
        Guid propertyId,
        CancellationToken cancellationToken);

    Task<string> AddImageAsync(
        Guid landlordId,
        Guid propertyId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);
}