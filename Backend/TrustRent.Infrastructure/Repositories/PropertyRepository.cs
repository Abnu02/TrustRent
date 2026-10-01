using Microsoft.EntityFrameworkCore;
using TrustRent.Domain.Entities;
using TrustRent.Infrastructure.Data;

namespace TrustRent.Infrastructure.Repositories;

public class PropertyRepository(TrustRentDbContext dbContext)
{
    public async Task AddAsync(Property property)
    {
        await dbContext.Properties.AddAsync(property);
        await dbContext.SaveChangesAsync();
    }

    public Task<List<Property>> GetByLandlordAsync(Guid landlordId)
    {
        return dbContext.Properties
            .Where(property => property.LandlordId == landlordId)
            .ToListAsync();
    }

    public Task<Property?> GetByIdAsync(Guid propertyId)
    {
        return dbContext.Properties
            .FirstOrDefaultAsync(property => property.Id == propertyId);
    }

    public async Task UpdateAsync(Property property)
    {
        dbContext.Properties.Update(property);
        await dbContext.SaveChangesAsync();
    }
}