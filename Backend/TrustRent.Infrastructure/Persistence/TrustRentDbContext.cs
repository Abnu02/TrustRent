using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrustRent.Domain.Entities;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Infrastructure.Persistence;

public class TrustRentDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public TrustRentDbContext(
        DbContextOptions<TrustRentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(TrustRentDbContext).Assembly);
    }
}