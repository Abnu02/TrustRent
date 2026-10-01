using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Domain.Entities;

namespace TrustRent.Application.Interfaces;

public interface ILandlordPropertyService
{
    Task<User> RegisterLandlordAsync(RegisterLandlordRequest request);
    Task<PropertyCreatedResponse> CreatePropertyAsync(Guid landlordId, CreatePropertyRequest request);
    Task<IEnumerable<MyPropertyResponse>> GetMyPropertiesAsync(Guid landlordId);
    Task<PagedResponse<MyPropertyResponse>> GetMyPropertiesPagedAsync(
        Guid landlordId, 
        PagedRequest pagedRequest, 
        string? statusFilter, 
        string? searchTerm, 
        string? sortBy, 
        bool sortDescending
    );
    Task<LandlordStatsDto> GetLandlordStatsAsync(Guid landlordId);
    Task<PropertyDetailResponse?> GetPropertyByIdAsync(Guid id);
    Task<PropertyDetailResponse?> UpdatePropertyAsync(Guid landlordId, Guid propertyId, UpdatePropertyRequest request);
}
