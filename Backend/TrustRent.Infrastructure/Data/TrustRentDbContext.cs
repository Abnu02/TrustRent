using Microsoft.EntityFrameworkCore;
using TrustRent.Domain.Entities;

namespace TrustRent.Infrastructure.Data;

public class TrustRentDbContext(DbContextOptions<TrustRentDbContext> options) : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();
}