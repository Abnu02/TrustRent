using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using TrustRent.Infrastructure.Identity;
using TrustRent.Domain.Properties;

namespace TrustRent.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<PropertyListing> PropertyListings => Set<PropertyListing>();
    public DbSet<PropertyReviewEvent> PropertyReviewEvents => Set<PropertyReviewEvent>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<PropertyListing>(entity =>
        {
            entity.HasKey(property => property.Id);
            entity.Property(property => property.Address).HasMaxLength(200).IsRequired();
            entity.Property(property => property.City).HasMaxLength(100).IsRequired();
            entity.Property(property => property.State).HasMaxLength(2).IsRequired();
            entity.Property(property => property.PostalCode).HasMaxLength(20).IsRequired();
            entity.Property(property => property.Description).HasMaxLength(4000);
            entity.Property(property => property.DeedFileNumber).HasMaxLength(100).IsRequired();
            entity.Property(property => property.RecordedOwner).HasMaxLength(200).IsRequired();
            entity.Property(property => property.ParcelId).HasMaxLength(100).IsRequired();
            entity.Property(property => property.UtilityStatus).HasMaxLength(200);
            entity.Property(property => property.MonthlyRent).HasPrecision(12, 2);
            entity.Property(property => property.Bathrooms).HasPrecision(4, 1);
            entity.Property(property => property.ReviewStatus).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(property => property.ReviewStatus);
            entity.HasIndex(property => property.SubmittedAt);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(property => property.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<PropertyReviewEvent>(entity =>
        {
            entity.HasKey(reviewEvent => reviewEvent.Id);
            entity.Property(reviewEvent => reviewEvent.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(reviewEvent => reviewEvent.Note).HasMaxLength(2000);
            entity.HasIndex(reviewEvent => reviewEvent.OccurredAt);
            entity.HasOne<PropertyListing>()
                .WithMany()
                .HasForeignKey(reviewEvent => reviewEvent.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(reviewEvent => reviewEvent.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
