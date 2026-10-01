using TrustRent.Application.Properties.DTOs;
using TrustRent.Application.Properties.Interfaces;
using TrustRent.Domain.Entities;

namespace TrustRent.Application.Properties.Services;

public sealed class PropertyService(IPropertyRepository propertyRepository) : IPropertyService
{
    public async Task<PropertyDto> CreatePropertyAsync(
        Guid landlordId,
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var property = new Property
        {
            LandlordId = landlordId,
            Title = request.Title,
            Description = request.Description,
            PropertyType = request.PropertyType,
            Rent = request.Rent,
            Deposit = request.Deposit,
            Location = request.Location,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms
        };

        await propertyRepository.AddAsync(property, cancellationToken);
        return ToDto(property);
    }

    public async Task<IReadOnlyList<PropertyDto>> GetByLandlordAsync(
        Guid landlordId,
        CancellationToken cancellationToken)
    {
        var properties = await propertyRepository.GetByLandlordAsync(landlordId, cancellationToken);
        return properties.Select(ToDto).ToArray();
    }

    public async Task<PropertyDto?> UpdatePropertyAsync(
        Guid landlordId,
        Guid propertyId,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetByIdAsync(propertyId, cancellationToken);
        if (property is null || property.LandlordId != landlordId)
        {
            return null;
        }

        property.Title = request.Title;
        property.Description = request.Description;
        property.PropertyType = request.PropertyType;
        property.Rent = request.Rent;
        property.Deposit = request.Deposit;
        property.Location = request.Location;
        property.Bedrooms = request.Bedrooms;
        property.Bathrooms = request.Bathrooms;

        await propertyRepository.UpdateAsync(property, cancellationToken);
        return ToDto(property);
    }

    private static PropertyDto ToDto(Property property)
    {
        return new PropertyDto
        {
            Id = property.Id,
            Title = property.Title,
            Rent = property.Rent,
            Location = property.Location,
            Status = property.Status,
            IsVerified = property.IsVerified
        };
    }
}