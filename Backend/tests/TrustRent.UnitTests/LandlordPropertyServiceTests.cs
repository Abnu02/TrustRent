using Moq;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Application.Exceptions;
using TrustRent.Application.Services;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;
using TrustRent.Domain.Repositories;

namespace TrustRent.UnitTests;

public class LandlordPropertyServiceTests
{
    private readonly Mock<IPropertyRepository> _propertyRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly LandlordPropertyService _service;

    public LandlordPropertyServiceTests()
    {
        _propertyRepoMock = new Mock<IPropertyRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _service = new LandlordPropertyService(_propertyRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task CreateProperty_ShouldInitializeAsPendingAndUnverified()
    {
        // Arrange
        var landlordId = Guid.NewGuid();
        var request = new CreatePropertyRequest(
            Title: "Modern 2 Bedroom Apartment",
            Description: "Clean and spacious near Bole.",
            PropertyType: "Apartment",
            Rent: 25000m,
            Deposit: 50000m,
            Location: "Bole, Addis Ababa",
            Bedrooms: 2,
            Bathrooms: 2
        );

        _propertyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Property>()))
            .ReturnsAsync((Property p) => p);

        // Act
        var result = await _service.CreatePropertyAsync(landlordId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pending", result.Status);
        Assert.False(result.IsVerified);
        Assert.Equal("Modern 2 Bedroom Apartment", result.Title);
        _propertyRepoMock.Verify(r => r.AddAsync(It.Is<Property>(p =>
            p.LandlordId == landlordId &&
            p.Status == PropertyStatus.Pending &&
            !p.IsVerified &&
            p.Rent == 25000m
        )), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public async Task CreateProperty_InvalidRent_ThrowsPropertyValidationException(decimal invalidRent)
    {
        // Arrange
        var landlordId = Guid.NewGuid();
        var request = new CreatePropertyRequest(
            Title: "Luxury Villa",
            Description: "Beautiful home",
            PropertyType: "Villa",
            Rent: invalidRent,
            Deposit: 50000m,
            Location: "Sarbet, Addis Ababa",
            Bedrooms: 4,
            Bathrooms: 3
        );

        // Act & Assert
        await Assert.ThrowsAsync<PropertyValidationException>(() => _service.CreatePropertyAsync(landlordId, request));
    }

    [Fact]
    public async Task UpdateProperty_UnauthorizedLandlord_ThrowsUnauthorizedPropertyAccessException()
    {
        // Arrange
        var realOwnerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        var existingProperty = new Property
        {
            Id = propertyId,
            LandlordId = realOwnerId,
            Title = "Real Owner Apartment",
            Description = "Nice place",
            PropertyType = "Apartment",
            Rent = 20000m,
            Deposit = 40000m,
            Location = "CMC",
            Bedrooms = 2,
            Bathrooms = 1
        };

        _propertyRepoMock.Setup(r => r.GetByIdAsync(propertyId)).ReturnsAsync(existingProperty);

        var updateRequest = new UpdatePropertyRequest(
            Title: "Hacked Apartment",
            Description: "Changed",
            PropertyType: "Apartment",
            Rent: 1000m,
            Deposit: 1000m,
            Location: "CMC",
            Bedrooms: 2,
            Bathrooms: 1
        );

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedPropertyAccessException>(() =>
            _service.UpdatePropertyAsync(attackerId, propertyId, updateRequest));
    }

    [Fact]
    public async Task GetMyPropertiesPagedAsync_ReturnsPagedResponse()
    {
        // Arrange
        var landlordId = Guid.NewGuid();
        var pagedRequest = new PagedRequest(Page: 1, PageSize: 2);
        var fakeProperties = new List<Property>
        {
            new() { Id = Guid.NewGuid(), LandlordId = landlordId, Title = "Prop 1", Description = "", PropertyType = "Apartment", Rent = 20000m, Deposit = 40000m, Location = "Bole", Bedrooms = 2, Bathrooms = 1 },
            new() { Id = Guid.NewGuid(), LandlordId = landlordId, Title = "Prop 2", Description = "", PropertyType = "Villa", Rent = 40000m, Deposit = 80000m, Location = "Sarbet", Bedrooms = 3, Bathrooms = 2 }
        };

        _propertyRepoMock
            .Setup(r => r.GetByLandlordIdPagedAsync(landlordId, 1, 2, null, null, null, false))
            .ReturnsAsync((fakeProperties, 5)); // 5 total items

        // Act
        var result = await _service.GetMyPropertiesPagedAsync(landlordId, pagedRequest, null, null, null, false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages); // 5 items / 2 per page = 3 pages
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }
}
