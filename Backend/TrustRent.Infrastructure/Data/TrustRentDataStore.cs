using System.Collections.Concurrent;
using TrustRent.Application.Common;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;

namespace TrustRent.Infrastructure.Data;

public class TrustRentDataStore : ITrustRentDataStore
{
    public static readonly Guid SeedLandlordId = Guid.Parse("8c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f");

    private readonly List<Property> _properties =
    [
        new Property
        {
            Id = Guid.Parse("7c3a1111-2222-3333-4444-555566667777"),
            LandlordId = SeedLandlordId,
            Title = "Modern 2 Bedroom Apartment",
            Description = "Clean and spacious apartment near Bole Edna Mall.",
            PropertyType = "Apartment",
            Rent = 25000m,
            Deposit = 50000m,
            Location = "Bole, Addis Ababa",
            Bedrooms = 2,
            Bathrooms = 2,
            Status = PropertyStatus.Pending,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        },
        new Property
        {
            Id = Guid.Parse("92ab1111-2222-3333-4444-555566667777"),
            LandlordId = SeedLandlordId,
            Title = "Family House with Garden",
            Description = "Spacious standalone family house with water reservoir.",
            PropertyType = "Villa",
            Rent = 40000m,
            Deposit = 80000m,
            Location = "Sarbet, Addis Ababa",
            Bedrooms = 3,
            Bathrooms = 2,
            Status = PropertyStatus.Approved,
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow.AddDays(-2),
            VerifiedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        },
        new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = SeedLandlordId,
            Title = "Luxury Condominium Floor 7",
            Description = "Elevator access, 24/7 generator backup, panoramic view.",
            PropertyType = "Condominium",
            Rent = 32000m,
            Deposit = 64000m,
            Location = "CMC, Addis Ababa",
            Bedrooms = 3,
            Bathrooms = 2,
            Status = PropertyStatus.Pending,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        },
        new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = SeedLandlordId,
            Title = "Cozy Studio near Kazanchis",
            Description = "Furnished studio walking distance to UNECA and hotels.",
            PropertyType = "Studio",
            Rent = 18000m,
            Deposit = 36000m,
            Location = "Kazanchis, Addis Ababa",
            Bedrooms = 1,
            Bathrooms = 1,
            Status = PropertyStatus.Approved,
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow.AddDays(-5),
            VerifiedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-6)
        },
        new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = SeedLandlordId,
            Title = "Spacious Villa in Old Airport",
            Description = "Large compound, diplomatic neighborhood, 4 master bedrooms.",
            PropertyType = "Villa",
            Rent = 75000m,
            Deposit = 150000m,
            Location = "Old Airport, Addis Ababa",
            Bedrooms = 4,
            Bathrooms = 3,
            Status = PropertyStatus.Approved,
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow.AddDays(-7),
            VerifiedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        },
        new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = SeedLandlordId,
            Title = "Commercial Ground Floor Space",
            Description = "Frontline roadside property suitable for branch or clinic.",
            PropertyType = "Commercial",
            Rent = 60000m,
            Deposit = 120000m,
            Location = "Megenagna, Addis Ababa",
            Bedrooms = 2,
            Bathrooms = 2,
            Status = PropertyStatus.Rejected,
            IsVerified = false,
            RejectionReason = "Title deed copy blurred and illegible; kindly re-upload scanned original.",
            CreatedAt = DateTime.UtcNow.AddDays(-4)
        }
    ];

    private readonly List<User> _users =
    [
        new User
        {
            Id = SeedLandlordId,
            FullName = "Abreham Bekele",
            Email = "abreham@example.com",
            PhoneNumber = "0912345678",
            Role = "Landlord",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        },
        new User
        {
            Id = Guid.Parse("11112222-3333-4444-5555-666677778888"),
            FullName = "Sara Hailu",
            Email = "sara@example.com",
            PhoneNumber = "0923456789",
            Role = "Tenant",
            IsVerified = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        }
    ];

    public ICollection<Property> Properties => _properties;
    public ICollection<User> Users => _users;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}
