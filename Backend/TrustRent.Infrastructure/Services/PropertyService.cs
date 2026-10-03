using Microsoft.AspNetCore.Identity;
using TrustRent.Application.Abstractions.Persistence;
using TrustRent.Application.Abstractions.Storage;
using TrustRent.Application.DTOs.Properties;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;

namespace TrustRent.Infrastructure.Services;

public class PropertyService : Application.Services.IPropertyService
{
    private readonly IPropertyRepository _repository;
    private readonly UserManager<Identity.ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorage;

    public PropertyService(
        IPropertyRepository repository,
        UserManager<Identity.ApplicationUser> userManager,
        IFileStorageService fileStorage)
    {
        _repository = repository;
        _userManager = userManager;
        _fileStorage = fileStorage;
    }

    public async Task<PropertyListResponse> CreateAsync(
        Guid landlordId,
        CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var landlord =
            await _userManager.FindByIdAsync(
                landlordId.ToString());

        if (landlord is null)
            throw new KeyNotFoundException(
                "Landlord was not found.");

        if (!await _userManager.IsInRoleAsync(
                landlord,
                "Landlord"))
        {
            throw new UnauthorizedAccessException(
                "Only landlords can create properties.");
        }

        if (!landlord.IsVerified)
        {
            throw new UnauthorizedAccessException(
                "Landlord must be verified before creating properties.");
        }

        var property = new Property(
            landlordId,
            request.Title,
            request.Description,
            request.PropertyType,
            request.Rent,
            request.Deposit,
            request.Location,
            request.Bedrooms,
            request.Bathrooms);

        await _repository.AddAsync(
            property,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return Map(property);
    }

    public async Task<List<PropertyListResponse>>
        GetMyPropertiesAsync(
            Guid landlordId,
            CancellationToken cancellationToken)
    {
        var properties =
            await _repository.GetByLandlordIdAsync(
                landlordId,
                cancellationToken);

        return properties.Select(Map).ToList();
    }

    public async Task UpdateAsync(
        Guid landlordId,
        Guid propertyId,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var property =
            await _repository.GetByIdAsync(
                propertyId,
                cancellationToken);

        if (property is null)
            throw new KeyNotFoundException(
                "Property was not found.");

        if (property.LandlordId != landlordId)
            throw new UnauthorizedAccessException(
                "You can only update your own properties.");

        property.Update(
            request.Title,
            request.Description,
            request.PropertyType,
            request.Rent,
            request.Deposit,
            request.Location,
            request.Bedrooms,
            request.Bathrooms);

        _repository.Update(property);

        await _repository.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<List<PropertyListResponse>> GetPublicAsync(
        string? location,
        decimal? minRent,
        decimal? maxRent,
        string? propertyType,
        int? bedrooms,
        int? bathrooms,
        CancellationToken cancellationToken)
    {
        PropertyType? parsedType = null;

        if (!string.IsNullOrWhiteSpace(propertyType) &&
            Enum.TryParse<PropertyType>(
                propertyType,
                true,
                out var type))
        {
            parsedType = type;
        }

        var properties =
            await _repository.GetPublicAsync(
                location,
                minRent,
                maxRent,
                parsedType,
                bedrooms,
                bathrooms,
                cancellationToken);

        var result = new List<PropertyListResponse>();

        foreach (var property in properties)
        {
            var landlord =
                await _userManager.FindByIdAsync(
                    property.LandlordId.ToString());

            if (landlord is null || !landlord.IsVerified)
                continue;

            result.Add(Map(property));
        }

        return result;
    }

    public async Task<PropertyDetailsResponse?> GetDetailsAsync(
        Guid propertyId,
        CancellationToken cancellationToken)
    {
        var property =
            await _repository.GetByIdAsync(
                propertyId,
                cancellationToken);

        if (property is null)
            return null;

        var landlord =
            await _userManager.FindByIdAsync(
                property.LandlordId.ToString());

        if (landlord is null)
            return null;

        // Public details only for trusted listings.
        if (property.Status != PropertyStatus.Approved ||
            !property.IsVerified ||
            !landlord.IsVerified)
        {
            return null;
        }

        return new PropertyDetailsResponse
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            PropertyType = property.PropertyType,
            Rent = property.Rent,
            Deposit = property.Deposit,
            Location = property.Location,
            Bedrooms = property.Bedrooms,
            Bathrooms = property.Bathrooms,
            Status = property.Status,
            IsVerified = property.IsVerified,
            VerifiedAt = property.VerifiedAt,
            ImageUrl = property.ImageUrl,

            Landlord = new LandlordResponse
            {
                Id = landlord.Id,
                FullName = landlord.FullName,
                PhoneNumber =
                    landlord.PhoneNumber ?? string.Empty,
                Email = landlord.Email ?? string.Empty,
                IsVerified = landlord.IsVerified
            }
        };
    }

    public async Task<string> AddImageAsync(
        Guid landlordId,
        Guid propertyId,
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var property =
            await _repository.GetByIdAsync(
                propertyId,
                cancellationToken);

        if (property is null)
            throw new KeyNotFoundException(
                "Property was not found.");

        if (property.LandlordId != landlordId)
            throw new UnauthorizedAccessException(
                "You can only upload images for your own property.");

        var oldImage = property.ImageUrl;

        var imageUrl =
            await _fileStorage.SavePropertyImageAsync(
                stream,
                fileName,
                contentType,
                cancellationToken);

        property.SetImage(imageUrl);

        _repository.Update(property);

        await _repository.SaveChangesAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(oldImage))
        {
            await _fileStorage.DeleteAsync(
                oldImage,
                cancellationToken);
        }

        return imageUrl;
    }

    private static PropertyListResponse Map(
        Property property)
    {
        return new PropertyListResponse
        {
            Id = property.Id,
            Title = property.Title,
            PropertyType = property.PropertyType,
            Rent = property.Rent,
            Location = property.Location,
            Status = property.Status,
            IsVerified = property.IsVerified,
            ImageUrl = property.ImageUrl
        };
    }
}