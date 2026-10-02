using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging.Abstractions;
using TrustRent.Application.Properties.DTOs;
using TrustRent.Application.Properties.Interfaces;
using TrustRent.Application.Properties.Services;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;

namespace TrustRent.UnitTests;

public class PropertyServiceTests
{
    [Fact]
    public async Task CreatePropertyAsync_SetsPendingAndUnverifiedDefaults()
    {
        var landlordId = Guid.NewGuid();
        var repository = new FakePropertyRepository();
        var service = CreateService(repository);

        var result = await service.CreatePropertyAsync(
            landlordId,
            ValidCreateRequest(),
            CancellationToken.None);

        var savedProperty = Assert.Single(repository.Properties);
        Assert.Equal(landlordId, savedProperty.LandlordId);
        Assert.Equal(PropertyStatus.Pending, savedProperty.Status);
        Assert.False(savedProperty.IsVerified);
        Assert.Equal(savedProperty.Id, result.Id);
    }

    [Fact]
    public void CreatePropertyRequest_InvalidValuesFailValidation()
    {
        var request = new CreatePropertyRequest
        {
            Title = string.Empty,
            Description = new string('x', 2001),
            PropertyType = null,
            Rent = -1,
            Deposit = -1,
            Location = string.Empty,
            Bedrooms = -1,
            Bathrooms = -1
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
    }

    [Fact]
    public void GetMyPropertiesQuery_RejectsUnsafePageSizeRentRangeAndSortField()
    {
        var query = new GetMyPropertiesQuery
        {
            PageSize = 51,
            SortBy = "landlordId"
        };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            query,
            new ValidationContext(query),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Equal(2, validationResults.Count);
    }

    [Fact]
    public void GetMyPropertiesQuery_RejectsMinRentGreaterThanMaxRent()
    {
        var query = new GetMyPropertiesQuery { MinRent = 50000, MaxRent = 10000 };
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            query,
            new ValidationContext(query),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Single(validationResults);
    }

    [Fact]
    public async Task UpdatePropertyAsync_UpdatesOwnedPropertyWithoutChangingModerationFields()
    {
        var landlordId = Guid.NewGuid();
        var property = ExistingProperty(landlordId);
        property.Status = PropertyStatus.Approved;
        property.IsVerified = true;
        var repository = new FakePropertyRepository(property);
        var service = CreateService(repository);

        var result = await service.UpdatePropertyAsync(
            landlordId,
            property.Id,
            ValidUpdateRequest("Updated apartment"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Updated apartment", property.Title);
        Assert.Equal(landlordId, property.LandlordId);
        Assert.Equal(PropertyStatus.Approved, property.Status);
        Assert.True(property.IsVerified);
        Assert.Equal(1, repository.UpdateLoads);
    }

    [Fact]
    public async Task UpdatePropertyAsync_ReturnsNullForAnotherLandlordsProperty()
    {
        var ownerId = Guid.NewGuid();
        var property = ExistingProperty(ownerId);
        var repository = new FakePropertyRepository(property);
        var service = CreateService(repository);

        var result = await service.UpdatePropertyAsync(
            Guid.NewGuid(),
            property.Id,
            ValidUpdateRequest("Attempted takeover"),
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal("Existing apartment", property.Title);
        Assert.Equal(ownerId, property.LandlordId);
        Assert.Equal(1, repository.UpdateLoads);
    }

    private static PropertyService CreateService(FakePropertyRepository repository)
    {
        return new PropertyService(repository, NullLogger<PropertyService>.Instance);
    }

    private static CreatePropertyRequest ValidCreateRequest()
    {
        return new CreatePropertyRequest
        {
            Title = "Modern apartment",
            Description = "A bright rental property.",
            PropertyType = PropertyType.Apartment,
            Rent = 25000,
            Deposit = 50000,
            Location = "Bole, Addis Ababa",
            Bedrooms = 2,
            Bathrooms = 1
        };
    }

    private static UpdatePropertyRequest ValidUpdateRequest(string title)
    {
        return new UpdatePropertyRequest
        {
            Title = title,
            Description = "Updated description.",
            PropertyType = PropertyType.Apartment,
            Rent = 27000,
            Deposit = 54000,
            Location = "Bole, Addis Ababa",
            Bedrooms = 2,
            Bathrooms = 2
        };
    }

    private static Property ExistingProperty(Guid landlordId)
    {
        return new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = landlordId,
            Title = "Existing apartment",
            Description = "Existing description.",
            PropertyType = PropertyType.Apartment,
            Rent = 20000,
            Deposit = 40000,
            Location = "Bole",
            Bedrooms = 2,
            Bathrooms = 1
        };
    }

    private sealed class FakePropertyRepository(params Property[] properties) : IPropertyRepository
    {
        public List<Property> Properties { get; } = [.. properties];
        public int UpdateLoads { get; private set; }

        public Task AddAsync(Property property, CancellationToken cancellationToken)
        {
            Properties.Add(property);
            return Task.CompletedTask;
        }

        public Task<PagedResult<PropertyDto>> GetByLandlordAsync(
            Guid landlordId,
            GetMyPropertiesQuery query,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<Property?> GetByIdAsync(Guid propertyId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Property update must use the tracked lookup.");
        }

        public Task<Property?> GetByIdForUpdateAsync(Guid propertyId, CancellationToken cancellationToken)
        {
            UpdateLoads++;
            return Task.FromResult(Properties.SingleOrDefault(property => property.Id == propertyId));
        }

        public Task UpdateAsync(Property property, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}