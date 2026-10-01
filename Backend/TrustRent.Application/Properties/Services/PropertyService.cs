using Microsoft.Extensions.Logging;
using TrustRent.Application.Properties.DTOs;
using TrustRent.Application.Properties.Interfaces;
using TrustRent.Domain.Entities;

namespace TrustRent.Application.Properties.Services;

public sealed class PropertyService(
    IPropertyRepository propertyRepository,
    ILogger<PropertyService> logger) : IPropertyService
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
            PropertyType = request.PropertyType!.Value,
            Rent = request.Rent,
            Deposit = request.Deposit,
            Location = request.Location,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms
        };

        await propertyRepository.AddAsync(property, cancellationToken);
        return ToDto(property);
    }

    public Task<PagedResult<PropertyDto>> GetByLandlordAsync(
        Guid landlordId,
        GetMyPropertiesQuery query,
        CancellationToken cancellationToken)
    {
        return propertyRepository.GetByLandlordAsync(landlordId, query, cancellationToken);
    }

    public async Task<PropertyDto?> GetByIdAsync(
        Guid landlordId,
        Guid propertyId,
        CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetByIdForUpdateAsync(propertyId, cancellationToken);
        if (property is null)
        {
            logger.LogWarning("PropertyNotFound {PropertyId}", propertyId);
            return null;
        }

        if (property.LandlordId != landlordId)
        {
            logger.LogWarning("PropertyOwnershipDenied {LandlordId} {PropertyId}", landlordId, propertyId);
            return null;
        }

        return ToDto(property);
    }

    public async Task<PropertyDto?> UpdatePropertyAsync(
        Guid landlordId,
        Guid propertyId,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetByIdAsync(propertyId, cancellationToken);
        if (property is null)
        {
            logger.LogWarning("PropertyNotFound {PropertyId}", propertyId);
            return null;
        }

        if (property.LandlordId != landlordId)
        {
            logger.LogWarning("PropertyOwnershipDenied {LandlordId} {PropertyId}", landlordId, propertyId);
            return null;
        }

        property.Title = request.Title;
        property.Description = request.Description;
        property.PropertyType = request.PropertyType!.Value;
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