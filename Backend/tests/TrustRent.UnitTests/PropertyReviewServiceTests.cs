using Microsoft.EntityFrameworkCore;
using TrustRent.Application.Properties;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;
using TrustRent.Domain.Properties;
using TrustRent.Infrastructure.Data;
using TrustRent.Infrastructure.Identity;
using TrustRent.Infrastructure.Persistence;
using TrustRent.Infrastructure.Services;

namespace TrustRent.UnitTests;

public sealed class PropertyReviewServiceTests
{
    [Fact]
    public async Task GetAllAsync_IncludesPropertiesCreatedByLandlordPropertyApi()
    {
        await using var applicationDb = CreateApplicationDbContext();
        await using var propertyDb = CreatePropertyDbContext();
        var owner = CreateOwner();
        applicationDb.Users.Add(owner);
        propertyDb.Properties.Add(CreateLegacyProperty(owner.Id));
        await applicationDb.SaveChangesAsync();
        await propertyDb.SaveChangesAsync();
        var service = new PropertyReviewService(applicationDb, propertyDb);

        var result = await service.GetAllAsync(CancellationToken.None);

        var property = Assert.Single(result);
        Assert.Equal("Submitted home", property.Address);
        Assert.Equal("Addis Ababa", property.City);
        Assert.Equal(owner.FullName, property.OwnerName);
        Assert.Equal(PropertyReviewStatus.Pending, property.ReviewStatus);
        Assert.Equal(property.Id, (await service.GetByIdAsync(property.Id, CancellationToken.None))?.Id);
        Assert.Equal(1, (await service.GetSummaryAsync(CancellationToken.None)).Pending);
    }

    [Fact]
    public async Task ReviewAsync_PersistsDecisionForLandlordPropertyApiSubmission()
    {
        await using var applicationDb = CreateApplicationDbContext();
        await using var propertyDb = CreatePropertyDbContext();
        var owner = CreateOwner();
        var reviewer = CreateOwner();
        var property = CreateLegacyProperty(owner.Id);
        applicationDb.Users.AddRange(owner, reviewer);
        propertyDb.Properties.Add(property);
        await applicationDb.SaveChangesAsync();
        await propertyDb.SaveChangesAsync();
        var service = new PropertyReviewService(applicationDb, propertyDb);

        var result = await service.ReviewAsync(
            property.Id,
            reviewer.Id,
            new ReviewPropertyRequest
            {
                Status = PropertyReviewStatus.Approved,
                ReviewNote = "Ownership verified."
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(PropertyReviewStatus.Approved, result.ReviewStatus);
        Assert.Equal("Ownership verified.", result.ReviewNote);
        Assert.NotNull(result.ReviewedAt);
        var savedProperty = await propertyDb.Properties.SingleAsync(item => item.Id == property.Id);
        Assert.Equal(PropertyStatus.Approved, savedProperty.Status);
        Assert.True(savedProperty.IsVerified);
        Assert.Equal(reviewer.Id, savedProperty.ReviewedByUserId);
        var auditEvent = await propertyDb.PropertyReviewEvents.SingleAsync();
        Assert.Equal(property.Id, auditEvent.PropertyId);
        Assert.Equal(reviewer.Id, auditEvent.ActorUserId);
        Assert.Equal(PropertyStatus.Approved, auditEvent.Status);
        Assert.Contains(await service.GetAuditLogAsync(CancellationToken.None), entry =>
            entry.PropertyId == property.Id && entry.ActorName == reviewer.FullName);
    }

    private static ApplicationDbContext CreateApplicationDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static TrustRentDbContext CreatePropertyDbContext()
    {
        var options = new DbContextOptionsBuilder<TrustRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TrustRentDbContext(options);
    }

    private static ApplicationUser CreateOwner()
    {
        var email = "landlord@example.test";
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            FullName = "Test Landlord",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Property CreateLegacyProperty(Guid ownerId)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = ownerId,
            Title = "Submitted home",
            Description = "A test property.",
            PropertyType = PropertyType.Apartment,
            Rent = 12000,
            Deposit = 12000,
            Location = "Addis Ababa",
            Bedrooms = 2,
            Bathrooms = 1
        };
    }
}
