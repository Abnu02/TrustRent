using Microsoft.EntityFrameworkCore;
using TrustRent.Application.Abstractions.Persistence;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;
using TrustRent.Infrastructure.Persistence;

namespace TrustRent.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly TrustRentDbContext _db;

    public PropertyRepository(TrustRentDbContext db)
    {
        _db = db;
    }

    public async Task<Property?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _db.Properties
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<List<Property>> GetByLandlordIdAsync(
        Guid landlordId,
        CancellationToken cancellationToken)
    {
        return await _db.Properties
            .AsNoTracking()
            .Where(x => x.LandlordId == landlordId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Property>> GetPendingAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Properties
            .AsNoTracking()
            .Where(x => x.Status == PropertyStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Property>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _db.Properties
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Property>> GetPublicAsync(
        string? location,
        decimal? minRent,
        decimal? maxRent,
        PropertyType? propertyType,
        int? bedrooms,
        int? bathrooms,
        CancellationToken cancellationToken)
    {
        var query = _db.Properties
            .AsNoTracking()
            .Where(x =>
                x.Status == PropertyStatus.Approved &&
                x.IsVerified);

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(x =>
                x.Location.ToLower().Contains(
                    location.ToLower()));
        }

        if (minRent.HasValue)
            query = query.Where(x => x.Rent >= minRent.Value);

        if (maxRent.HasValue)
            query = query.Where(x => x.Rent <= maxRent.Value);

        if (propertyType.HasValue)
            query = query.Where(x =>
                x.PropertyType == propertyType.Value);

        if (bedrooms.HasValue)
            query = query.Where(x =>
                x.Bedrooms == bedrooms.Value);

        if (bathrooms.HasValue)
            query = query.Where(x =>
                x.Bathrooms == bathrooms.Value);

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Property property,
        CancellationToken cancellationToken)
    {
        await _db.Properties.AddAsync(
            property,
            cancellationToken);
    }

    public void Update(Property property)
    {
        _db.Properties.Update(property);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }
}