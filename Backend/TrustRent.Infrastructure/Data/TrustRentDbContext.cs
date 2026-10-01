using Microsoft.EntityFrameworkCore;
using TrustRent.Domain.Entities;

namespace TrustRent.Infrastructure.Data;

public class TrustRentDbContext(DbContextOptions<TrustRentDbContext> options) : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var property = modelBuilder.Entity<Property>();
        property.ToTable("Properties");
        property.HasKey(entity => entity.Id);
        property.Property(entity => entity.Title).IsRequired().HasMaxLength(200);
        property.Property(entity => entity.Description).IsRequired().HasMaxLength(2000);
        property.Property(entity => entity.Location).IsRequired().HasMaxLength(200);
        property.Property(entity => entity.Rent).HasPrecision(12, 2);
        property.Property(entity => entity.Deposit).HasPrecision(12, 2);
        property.Property(entity => entity.PropertyType).HasConversion<int>().IsRequired();
        property.Property(entity => entity.Status).HasConversion<int>().IsRequired();
        property.HasIndex(entity => entity.LandlordId);
        property.HasIndex(entity => entity.Status);
    }
}