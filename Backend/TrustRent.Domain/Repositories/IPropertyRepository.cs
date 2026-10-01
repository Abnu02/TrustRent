using TrustRent.Domain.Entities;

namespace TrustRent.Domain.Repositories;

public interface IPropertyRepository
{
    Task<Property?> GetByIdAsync(Guid id);
    Task<IEnumerable<Property>> GetByLandlordIdAsync(Guid landlordId);
    Task<(IEnumerable<Property> Items, int TotalCount)> GetByLandlordIdPagedAsync(
        Guid landlordId, 
        int page, 
        int pageSize, 
        string? statusFilter, 
        string? searchTerm, 
        string? sortBy, 
        bool sortDescending
    );
    Task<IEnumerable<Property>> GetAllApprovedAsync();
    Task<IEnumerable<Property>> GetPendingAsync();
    Task<Property> AddAsync(Property property);
    Task UpdateAsync(Property property);
}
