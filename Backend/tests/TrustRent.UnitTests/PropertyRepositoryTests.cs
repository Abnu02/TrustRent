using Microsoft.EntityFrameworkCore;
using TrustRent.Application.Properties.DTOs;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;
using TrustRent.Infrastructure.Data;
using TrustRent.Infrastructure.Repositories;

namespace TrustRent.UnitTests;

public class PropertyRepositoryTests
{
    [Fact]
    public async Task GetByLandlordAsync_AppliesFiltersBeforePaginationAndProjectsDtos()
    {
        var landlordId = Guid.NewGuid();
        await using var dbContext = CreateDbContext();
        await dbContext.Properties.AddRangeAsync(
            CreateProperty(landlordId, "Modern apartment one", "Bole", 15000, PropertyStatus.Pending),
            CreateProperty(landlordId, "Modern apartment two", "Sarbet", 25000, PropertyStatus.Pending),
            CreateProperty(landlordId, "Modern studio", "Bole", 18000, PropertyStatus.Approved, PropertyType.Studio),
            CreateProperty(Guid.NewGuid(), "Modern apartment three", "Bole", 20000, PropertyStatus.Pending));
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        var repository = new PropertyRepository(dbContext);

        var result = await repository.GetByLandlordAsync(
            landlordId,
            new GetMyPropertiesQuery
            {
                PageSize = 1,
                Status = PropertyStatus.Pending,
                PropertyType = PropertyType.Apartment,
                MinRent = 10000,
                MaxRent = 30000,
                Search = "MODERN",
                SortBy = "rent",
                SortDirection = "asc"
            },
            CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.Equal(15000, Assert.Single(result.Items).Rent);
        Assert.Empty(dbContext.ChangeTracker.Entries<Property>());
    }

    [Fact]
    public async Task GetByLandlordAsync_SortsAndReturnsPaginationMetadata()
    {
        var landlordId = Guid.NewGuid();
        await using var dbContext = CreateDbContext();
        await dbContext.Properties.AddRangeAsync(
            CreateProperty(landlordId, "Apartment one", "Bole", 10000, PropertyStatus.Pending),
            CreateProperty(landlordId, "Apartment two", "Sarbet", 20000, PropertyStatus.Pending),
            CreateProperty(landlordId, "Apartment three", "Kazanchis", 30000, PropertyStatus.Pending));
        await dbContext.SaveChangesAsync();
        var repository = new PropertyRepository(dbContext);

        var result = await repository.GetByLandlordAsync(
            landlordId,
            new GetMyPropertiesQuery { Page = 2, PageSize = 2, SortBy = "rent", SortDirection = "desc" },
            CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
        Assert.Equal(10000, Assert.Single(result.Items).Rent);
    }

    [Fact]
    public async Task GetByIdForUpdateAsync_ReturnsTrackedProperty()
    {
        var property = CreateProperty(Guid.NewGuid(), "Apartment", "Bole", 10000, PropertyStatus.Pending);
        await using var dbContext = CreateDbContext();
        await dbContext.Properties.AddAsync(property);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        var repository = new PropertyRepository(dbContext);

        var result = await repository.GetByIdForUpdateAsync(property.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(EntityState.Unchanged, dbContext.Entry(result).State);
    }

    [Fact]
    public void PropertyModel_ConfiguresPrecisionLengthsAndIndexes()
    {
        using var dbContext = CreateDbContext();
        var entityType = dbContext.Model.FindEntityType(typeof(Property));
        Assert.NotNull(entityType);

        var rent = entityType.FindProperty(nameof(Property.Rent));
        var title = entityType.FindProperty(nameof(Property.Title));
        Assert.Equal(12, rent!.GetPrecision());
        Assert.Equal(2, rent.GetScale());
        Assert.Equal(200, title!.GetMaxLength());
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(Property.LandlordId)]));
        Assert.Contains(entityType.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(Property.Status)]));
    }

    private static TrustRentDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TrustRentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TrustRentDbContext(options);
    }

    private static Property CreateProperty(
        Guid landlordId,
        string title,
        string location,
        decimal rent,
        PropertyStatus status,
        PropertyType propertyType = PropertyType.Apartment)
    {
        return new Property
        {
            LandlordId = landlordId,
            Title = title,
            Description = "Rental description.",
            PropertyType = propertyType,
            Rent = rent,
            Deposit = rent * 2,
            Location = location,
            Bedrooms = 2,
            Bathrooms = 1,
            Status = status
        };
    }
}