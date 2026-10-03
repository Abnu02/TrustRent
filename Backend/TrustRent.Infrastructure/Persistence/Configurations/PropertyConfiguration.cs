using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustRent.Domain.Entities;

namespace TrustRent.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration
    : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.PropertyType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Rent)
            .HasPrecision(18, 2);

        builder.Property(x => x.Deposit)
            .HasPrecision(18, 2);

        builder.Property(x => x.Location)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.LandlordId);

        builder.HasIndex(x => new
        {
            x.Status,
            x.IsVerified
        });
    }
}