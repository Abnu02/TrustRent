using Microsoft.EntityFrameworkCore;
using TrustRent.Application.Properties.DTOs;
using TrustRent.Application.Properties.Interfaces;
using TrustRent.Domain.Entities;
using TrustRent.Infrastructure.Data;

namespace TrustRent.Infrastructure.Repositories;

public class PropertyRepository(TrustRentDbContext dbContext) : IPropertyRepository
{
    public async Task AddAsync(Property property, CancellationToken cancellationToken)
    {
        await dbContext.Properties.AddAsync(property, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<PropertyDto>> GetByLandlordAsync(
        Guid landlordId,
        GetMyPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        var properties = dbContext.Properties
            .AsNoTracking()
            .Where(property => property.LandlordId == landlordId)
            .AsQueryable();

        if (query.Status.HasValue)
        {
            properties = properties.Where(property => property.Status == query.Status.Value);
        }

        if (query.PropertyType.HasValue)
        {
            properties = properties.Where(property => property.PropertyType == query.PropertyType.Value);
        }

        if (query.MinRent.HasValue)
        {
            properties = properties.Where(property => property.Rent >= query.MinRent.Value);
        }

        if (query.MaxRent.HasValue)
        {
            properties = properties.Where(property => property.Rent <= query.MaxRent.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.Trim().ToLowerInvariant();
            properties = properties.Where(property =>
                property.Title.ToLower().Contains(searchTerm) ||
                property.Location.ToLower().Contains(searchTerm));
        }

        var totalCount = await properties.CountAsync(cancellationToken);
        var orderedProperties = ApplySorting(properties, query.SortBy, query.SortDirection);
        var offset = checked((query.Page - 1) * query.PageSize);
        var items = await orderedProperties
            .Skip(offset)
            .Take(query.PageSize)
            .Select(property => new PropertyDto
            {
                Id = property.Id,
                Title = property.Title,
                Rent = property.Rent,
                Location = property.Location,
                Status = property.Status,
                IsVerified = property.IsVerified
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PropertyDto>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken)
    {
        return dbContext.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(property => property.Id == propertyId, cancellationToken);
    }

    public Task<Property?> GetByIdForUpdateAsync(Guid propertyId, CancellationToken cancellationToken)
    {
        return dbContext.Properties
            .FirstOrDefaultAsync(property => property.Id == propertyId, cancellationToken);
    }

    public async Task UpdateAsync(Property property, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(property).State == EntityState.Detached)
        {
            throw new InvalidOperationException("The property must be loaded for update before saving.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IOrderedQueryable<Property> ApplySorting(
        IQueryable<Property> properties,
        string sortBy,
        string sortDirection)
    {
        var descending = sortDirection == "desc";
        IOrderedQueryable<Property> orderedProperties = (sortBy, descending) switch
        {
            ("rent", false) => properties.OrderBy(property => property.Rent),
            ("rent", true) => properties.OrderByDescending(property => property.Rent),
            ("location", false) => properties.OrderBy(property => property.Location),
            ("location", true) => properties.OrderByDescending(property => property.Location),
            (_, true) => properties.OrderByDescending(property => property.Title),
            _ => properties.OrderBy(property => property.Title)
        };

        return orderedProperties.ThenBy(property => property.Id);
    }
}