using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;
using TrustRent.Domain.Repositories;

namespace TrustRent.Infrastructure.Repositories;

public class InMemoryPropertyRepository : IPropertyRepository
{
    private static readonly Guid SeedLandlordId = Guid.Parse("8c1d2e3f-4a5b-6c7d-8e9f-0a1b2c3d4e5f");

    private static readonly List<Property> Properties =
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
            CreatedAt = DateTime.UtcNow.AddHours(-5)
        },
        new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = SeedLandlordId,
            Title = "Cozy 1-Bedroom Studio",
            Description = "Furnished studio near international organizations.",
            PropertyType = "Apartment",
            Rent = 18000m,
            Deposit = 36000m,
            Location = "Kazanchis, Addis Ababa",
            Bedrooms = 1,
            Bathrooms = 1,
            Status = PropertyStatus.Approved,
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow.AddDays(-1),
            VerifiedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-4)
        },
        new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = SeedLandlordId,
            Title = "G+1 Spacious Executive Residence",
            Description = "Private compound, guard house, multiple parking spots.",
            PropertyType = "G+1",
            Rent = 65000m,
            Deposit = 130000m,
            Location = "Old Airport, Addis Ababa",
            Bedrooms = 4,
            Bathrooms = 3,
            Status = PropertyStatus.Approved,
            IsVerified = true,
            VerifiedAt = DateTime.UtcNow.AddDays(-5),
            VerifiedBy = "Admin",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        },
        new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = SeedLandlordId,
            Title = "Newly Finished 2-Bed Condominium",
            Description = "Tiled floor, water tank, peaceful neighborhood.",
            PropertyType = "Condominium",
            Rent = 22000m,
            Deposit = 44000m,
            Location = "Gerji, Addis Ababa",
            Bedrooms = 2,
            Bathrooms = 1,
            Status = PropertyStatus.Pending,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow.AddHours(-12)
        },
        new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = SeedLandlordId,
            Title = "Townhouse with Balcony",
            Description = "Near shopping center, high-speed fiber internet ready.",
            PropertyType = "Apartment",
            Rent = 28000m,
            Deposit = 56000m,
            Location = "Bole Medhanialem, Addis Ababa",
            Bedrooms = 2,
            Bathrooms = 2,
            Status = PropertyStatus.Pending,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow.AddHours(-20)
        }
    ];

    public Task<Property?> GetByIdAsync(Guid id)
    {
        var property = Properties.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(property);
    }

    public Task<IEnumerable<Property>> GetByLandlordIdAsync(Guid landlordId)
    {
        var properties = Properties.Where(p => p.LandlordId == landlordId);
        return Task.FromResult<IEnumerable<Property>>(properties.ToList());
    }

    public Task<(IEnumerable<Property> Items, int TotalCount)> GetByLandlordIdPagedAsync(
        Guid landlordId, 
        int page, 
        int pageSize, 
        string? statusFilter, 
        string? searchTerm, 
        string? sortBy, 
        bool sortDescending)
    {
        var query = Properties.Where(p => p.LandlordId == landlordId);

        // Filter by Status (Module 5 LINQ filtering)
        if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<PropertyStatus>(statusFilter, true, out var parsedStatus))
            {
                query = query.Where(p => p.Status == parsedStatus);
            }
        }

        // Search term (Title or Location)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(p => p.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                     p.Location.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = query.Count();

        // Sorting
        query = (sortBy?.ToLowerInvariant()) switch
        {
            "rent" => sortDescending ? query.OrderByDescending(p => p.Rent) : query.OrderBy(p => p.Rent),
            "title" => sortDescending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            _ => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        // Pagination with .Skip() and .Take() (Module 6)
        var pagedItems = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<(IEnumerable<Property> Items, int TotalCount)>((pagedItems, totalCount));
    }

    public Task<IEnumerable<Property>> GetAllApprovedAsync()
    {
        var approved = Properties.Where(p => p.Status == PropertyStatus.Approved && p.IsVerified);
        return Task.FromResult<IEnumerable<Property>>(approved.ToList());
    }

    public Task<IEnumerable<Property>> GetPendingAsync()
    {
        var pending = Properties.Where(p => p.Status == PropertyStatus.Pending);
        return Task.FromResult<IEnumerable<Property>>(pending.ToList());
    }

    public Task<Property> AddAsync(Property property)
    {
        Properties.Add(property);
        return Task.FromResult(property);
    }

    public Task UpdateAsync(Property property)
    {
        var index = Properties.FindIndex(p => p.Id == property.Id);
        if (index >= 0)
        {
            Properties[index] = property;
        }
        return Task.CompletedTask;
    }
}
