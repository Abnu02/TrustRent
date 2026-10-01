using TrustRent.Application.Common;
using TrustRent.Application.DTOs;
using TrustRent.Application.Exceptions;
using TrustRent.Application.Interfaces;
using TrustRent.Domain.Entities;
using TrustRent.Domain.Enums;
using TrustRent.Domain.Repositories;

namespace TrustRent.Application.Services;

public class LandlordPropertyService : ILandlordPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;

    public LandlordPropertyService(IPropertyRepository propertyRepository, IUserRepository userRepository)
    {
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
    }

    public async Task<User> RegisterLandlordAsync(RegisterLandlordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new PropertyValidationException(nameof(request.FullName), "Full name is required for landlord registration.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new PropertyValidationException(nameof(request.Email), "Email address is required.");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new PropertyValidationException(nameof(request.PhoneNumber), "Phone number is required.");

        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new PropertyValidationException(nameof(request.Email), "A user with this email address already exists.");

        var landlord = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Role = "Landlord",
            IsVerified = false, // Must be verified by admin
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.AddAsync(landlord);
    }

    public async Task<PropertyCreatedResponse> CreatePropertyAsync(Guid landlordId, CreatePropertyRequest request)
    {
        // Guard Clauses (Module 1 Clean Code)
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new PropertyValidationException(nameof(request.Title), "Property title is required.");

        if (string.IsNullOrWhiteSpace(request.Location))
            throw new PropertyValidationException(nameof(request.Location), "Location is required.");

        if (request.Rent <= 0)
            throw new PropertyValidationException(nameof(request.Rent), "Rent amount must be greater than zero ETB.");

        if (request.Deposit < 0)
            throw new PropertyValidationException(nameof(request.Deposit), "Deposit amount cannot be negative.");

        var property = new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = landlordId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            PropertyType = string.IsNullOrWhiteSpace(request.PropertyType) ? "Apartment" : request.PropertyType.Trim(),
            Rent = request.Rent,
            Deposit = request.Deposit,
            Location = request.Location.Trim(),
            Bedrooms = request.Bedrooms < 1 ? 1 : request.Bedrooms,
            Bathrooms = request.Bathrooms < 1 ? 1 : request.Bathrooms,
            Status = PropertyStatus.Pending, // Automated status: Pending review
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _propertyRepository.AddAsync(property);

        return new PropertyCreatedResponse(
            created.Id,
            created.Title,
            created.Status.ToString(),
            created.IsVerified,
            created.CreatedAt
        );
    }

    public async Task<IEnumerable<MyPropertyResponse>> GetMyPropertiesAsync(Guid landlordId)
    {
        var properties = await _propertyRepository.GetByLandlordIdAsync(landlordId);

        return properties.Select(MapToResponse);
    }

    public async Task<PagedResponse<MyPropertyResponse>> GetMyPropertiesPagedAsync(
        Guid landlordId, 
        PagedRequest pagedRequest, 
        string? statusFilter, 
        string? searchTerm, 
        string? sortBy, 
        bool sortDescending)
    {
        var (items, totalCount) = await _propertyRepository.GetByLandlordIdPagedAsync(
            landlordId,
            pagedRequest.Page,
            pagedRequest.PageSize,
            statusFilter,
            searchTerm,
            sortBy,
            sortDescending
        );

        var responseDtos = items.Select(MapToResponse);

        return new PagedResponse<MyPropertyResponse>(
            responseDtos,
            totalCount,
            pagedRequest.Page,
            pagedRequest.PageSize
        );
    }

    public async Task<LandlordStatsDto> GetLandlordStatsAsync(Guid landlordId)
    {
        var properties = (await _propertyRepository.GetByLandlordIdAsync(landlordId)).ToList();

        // LINQ Aggregations (Module 1 & 5)
        var total = properties.Count;
        var pending = properties.Count(p => p.Status == PropertyStatus.Pending);
        var approved = properties.Count(p => p.Status == PropertyStatus.Approved);
        var rejected = properties.Count(p => p.Status == PropertyStatus.Rejected);
        var totalRevenue = properties
            .Where(p => p.Status == PropertyStatus.Approved)
            .Sum(p => p.Rent);

        return new LandlordStatsDto(total, pending, approved, rejected, totalRevenue);
    }

    public async Task<PropertyDetailResponse?> GetPropertyByIdAsync(Guid id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null) return null;

        var landlord = await _userRepository.GetByIdAsync(property.LandlordId);
        var landlordContact = landlord != null
            ? new LandlordContactDto(landlord.Id, landlord.FullName, landlord.PhoneNumber, landlord.Email, landlord.IsVerified)
            : new LandlordContactDto(property.LandlordId, "Verified Landlord", "0912345678", "contact@trustrent.et", false);

        return new PropertyDetailResponse(
            property.Id,
            property.Title,
            property.Description,
            property.PropertyType,
            property.Rent,
            property.Deposit,
            property.Location,
            property.Bedrooms,
            property.Bathrooms,
            property.IsVerified,
            landlordContact
        );
    }

    public async Task<PropertyDetailResponse?> UpdatePropertyAsync(Guid landlordId, Guid propertyId, UpdatePropertyRequest request)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId);
        if (property == null)
            throw new PropertyNotFoundException(propertyId);

        // Security check: only the owning landlord can update their property
        if (property.LandlordId != landlordId)
            throw new UnauthorizedPropertyAccessException(propertyId, landlordId);

        property.Title = request.Title.Trim();
        property.Description = request.Description?.Trim() ?? string.Empty;
        property.PropertyType = request.PropertyType.Trim();
        property.Rent = request.Rent;
        property.Deposit = request.Deposit;
        property.Location = request.Location.Trim();
        property.Bedrooms = request.Bedrooms;
        property.Bathrooms = request.Bathrooms;
        
        await _propertyRepository.UpdateAsync(property);

        return await GetPropertyByIdAsync(propertyId);
    }

    private static MyPropertyResponse MapToResponse(Property p) => new(
        p.Id,
        p.Title,
        p.Rent,
        p.Deposit,
        p.Location,
        p.PropertyType,
        p.Bedrooms,
        p.Bathrooms,
        p.Status.ToString(),
        p.IsVerified,
        p.CreatedAt
    );
}
