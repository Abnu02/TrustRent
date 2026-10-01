using Microsoft.EntityFrameworkCore;
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

    public Task<List<Property>> GetByLandlordAsync(Guid landlordId, CancellationToken cancellationToken)
    {
        return dbContext.Properties
            .Where(property => property.LandlordId == landlordId)
            .ToListAsync(cancellationToken);
    }

    public Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken)
    {
        return dbContext.Properties
            .FirstOrDefaultAsync(property => property.Id == propertyId, cancellationToken);
    }

    public async Task UpdateAsync(Property property, CancellationToken cancellationToken)
    {
        dbContext.Properties.Update(property);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}