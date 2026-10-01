using Moq;
using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Application.Properties.Commands;
using TrustRent.Application.Properties.Queries;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;

namespace TrustRent.UnitTests;

public class LandlordCqrsTests
{
    private readonly Mock<ITrustRentDataStore> _dataStoreMock;
    private readonly List<Property> _properties;
    private readonly List<User> _users;
    private readonly Guid _landlordId;

    public LandlordCqrsTests()
    {
        _landlordId = Guid.NewGuid();
        _dataStoreMock = new Mock<ITrustRentDataStore>();

        _users =
        [
            new User
            {
                Id = _landlordId,
                FullName = "Abreham Bekele",
                Email = "abreham@example.com",
                PhoneNumber = "0912345678",
                Role = "Landlord",
                IsVerified = true
            }
        ];

        _properties = [];

        _dataStoreMock.Setup(d => d.Users).Returns(_users);
        _dataStoreMock.Setup(d => d.Properties).Returns(_properties);
        _dataStoreMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task CreatePropertyCommand_ShouldInitializeAsPendingAndUnverified()
    {
        // Arrange
        var handler = new CreatePropertyCommandHandler(_dataStoreMock.Object);
        var command = new CreatePropertyCommand(
            LandlordId: _landlordId,
            Title: "Modern 2 Bedroom Apartment",
            Description: "Clean and spacious near Bole.",
            PropertyType: "Apartment",
            Rent: 25000m,
            Deposit: 50000m,
            Location: "Bole, Addis Ababa",
            Bedrooms: 2,
            Bathrooms: 2
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value.Status);
        Assert.False(result.Value.IsVerified);
        Assert.Equal("Modern 2 Bedroom Apartment", result.Value.Title);
        Assert.Single(_properties);
        Assert.Equal(_landlordId, _properties[0].LandlordId);
        Assert.Equal(PropertyStatus.Pending, _properties[0].Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-500)]
    public async Task CreatePropertyCommand_InvalidRent_ReturnsFailure(decimal invalidRent)
    {
        // Arrange
        var handler = new CreatePropertyCommandHandler(_dataStoreMock.Object);
        var command = new CreatePropertyCommand(
            LandlordId: _landlordId,
            Title: "Luxury Villa",
            Description: "Beautiful home",
            PropertyType: "Villa",
            Rent: invalidRent,
            Deposit: 50000m,
            Location: "Sarbet, Addis Ababa",
            Bedrooms: 4,
            Bathrooms: 3
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("validation_error", result.Error.Code);
    }

    [Fact]
    public async Task SubmitPropertyForVerificationCommand_UpdatesStatusToPending()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        _properties.Add(new Property
        {
            Id = propertyId,
            LandlordId = _landlordId,
            Title = "Draft Apartment",
            Description = "Pending details",
            PropertyType = "Apartment",
            Rent = 20000m,
            Deposit = 40000m,
            Location = "Bole",
            Status = PropertyStatus.Rejected,
            IsVerified = false
        });

        var handler = new SubmitPropertyForVerificationCommandHandler(_dataStoreMock.Object);
        var command = new SubmitPropertyForVerificationCommand(propertyId, _landlordId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Pending", result.Value.Status);
        Assert.Equal(PropertyStatus.Pending, _properties[0].Status);
    }

    [Fact]
    public async Task UpdatePropertyCommand_UnauthorizedLandlord_ReturnsFailure()
    {
        // Arrange
        var realOwnerId = _landlordId;
        var attackerId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        _properties.Add(new Property
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
        });

        var handler = new UpdatePropertyCommandHandler(_dataStoreMock.Object);
        var command = new UpdatePropertyCommand(
            PropertyId: propertyId,
            LandlordId: attackerId,
            Title: "Hacked Apartment",
            Description: "Changed",
            PropertyType: "Apartment",
            Rent: 1000m,
            Deposit: 1000m,
            Location: "CMC",
            Bedrooms: 2,
            Bathrooms: 1
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.Error.Code);
    }

    [Fact]
    public async Task GetLandlordPropertiesQuery_ReturnsPagedResponseWithCorrectTotal()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            _properties.Add(new Property
            {
                Id = Guid.NewGuid(),
                LandlordId = _landlordId,
                Title = $"Prop {i}",
                Description = "Desc",
                PropertyType = "Apartment",
                Rent = 20000m * i,
                Deposit = 40000m * i,
                Location = "Bole",
                Bedrooms = 2,
                Bathrooms = 1,
                Status = PropertyStatus.Approved,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }

        var handler = new GetLandlordPropertiesQueryHandler(_dataStoreMock.Object);
        var query = new GetLandlordPropertiesQuery(
            LandlordId: _landlordId,
            Page: 1,
            PageSize: 2
        );

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages); // 5 items / 2 per page = 3 pages
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public async Task ApprovePropertyCommand_MarksPropertyAsApprovedAndLive()
    {
        // Arrange
        var propertyId = Guid.NewGuid();
        _properties.Add(new Property
        {
            Id = propertyId,
            LandlordId = _landlordId,
            Title = "Awaiting Property",
            Description = "Desc",
            PropertyType = "Villa",
            Rent = 35000m,
            Deposit = 70000m,
            Location = "Sarbet",
            Status = PropertyStatus.Pending,
            IsVerified = false
        });

        var handler = new ApprovePropertyCommandHandler(_dataStoreMock.Object);
        var command = new ApprovePropertyCommand(propertyId, "Licensed Auditor");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Approved", result.Value.Status);
        Assert.True(result.Value.IsVerified);
        Assert.Equal(PropertyStatus.Approved, _properties[0].Status);
        Assert.True(_properties[0].IsVerified);
    }
}
