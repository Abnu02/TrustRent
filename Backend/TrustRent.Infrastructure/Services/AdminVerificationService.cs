using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrustRent.Application.DTOs.Auth;
using TrustRent.Application.DTOs.Properties;
using TrustRent.Application.DTOs.Verification;
using TrustRent.Application.Abstractions.Persistence;
using TrustRent.Application.Services;
using TrustRent.Domain.Enums;
using TrustRent.Infrastructure.Identity;

namespace TrustRent.Infrastructure.Services;

public class AdminVerificationService
    : IAdminVerificationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPropertyRepository _propertyRepository;

    public AdminVerificationService(
        UserManager<ApplicationUser> userManager,
        IPropertyRepository propertyRepository)
    {
        _userManager = userManager;
        _propertyRepository = propertyRepository;
    }

    public async Task<List<AdminLandlordResponse>> GetLandlordsAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var landlords = await _userManager.GetUsersInRoleAsync("Landlord");

        return landlords
            .Select(ToAdminLandlordResponse)
            .OrderBy(x => x.FullName)
            .ToList();
    }

    public async Task<AdminLandlordResponse?> GetLandlordAsync(
        Guid landlordId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var landlord = await FindLandlordAsync(landlordId);
        return landlord is null ? null : ToAdminLandlordResponse(landlord);
    }

    public async Task<AdminLandlordResponse> CreateLandlordAsync(
        CreateLandlordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException(
                "Full name, email, and password are required.");
        }

        var fullName = request.FullName.Trim();
        var email = request.Email.Trim();
        var phoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;

        var landlord = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = phoneNumber,
            FullName = fullName,
            IsVerified = false,
            LockoutEnabled = true
        };

        var createResult = await _userManager.CreateAsync(
            landlord,
            request.Password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", createResult.Errors.Select(x => x.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(landlord, "Landlord");
        if (!roleResult.Succeeded)
        {
            var rollbackResult = await _userManager.DeleteAsync(landlord);
            var errors = roleResult.Errors.Select(x => x.Description);

            if (!rollbackResult.Succeeded)
            {
                errors = errors.Concat(
                    rollbackResult.Errors.Select(
                        x => $"Rollback failed: {x.Description}"));
            }

            throw new InvalidOperationException(string.Join("; ", errors));
        }

        return ToAdminLandlordResponse(landlord);
    }

    public async Task<AdminLandlordResponse> UpdateLandlordAsync(
        Guid landlordId,
        UpdateLandlordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var landlord = await FindLandlordAsync(landlordId)
            ?? throw new KeyNotFoundException("Landlord was not found.");

        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException(
                "Full name and email are required.");
        }

        var fullName = request.FullName.Trim();
        var email = request.Email.Trim();

        landlord.FullName = fullName;
        landlord.Email = email;
        landlord.UserName = email;
        landlord.PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;

        var result = await _userManager.UpdateAsync(landlord);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        return ToAdminLandlordResponse(landlord);
    }

    public async Task SetLandlordActiveAsync(
        Guid landlordId,
        bool isActive,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var landlord = await FindLandlordAsync(landlordId)
            ?? throw new KeyNotFoundException("Landlord was not found.");

        // Identity lockout keeps the account and its properties while preventing new logins.
        var lockoutEnabledResult =
            await _userManager.SetLockoutEnabledAsync(landlord, true);

        if (!lockoutEnabledResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    lockoutEnabledResult.Errors.Select(x => x.Description)));
        }

        var lockoutResult = await _userManager.SetLockoutEndDateAsync(
            landlord,
            isActive ? null : DateTimeOffset.MaxValue);

        if (!lockoutResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    lockoutResult.Errors.Select(x => x.Description)));
        }
    }

    public async Task<List<UserResponse>>
        GetPendingLandlordsAsync(
            CancellationToken cancellationToken)
    {
        var landlords = await _userManager
            .GetUsersInRoleAsync("Landlord");

        return landlords
            .Where(x => !x.IsVerified)
            .Select(x => new UserResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email ?? string.Empty,
                PhoneNumber =
                    x.PhoneNumber ?? string.Empty,
                Role = "Landlord",
                IsVerified = x.IsVerified
            })
            .ToList();
    }

    private async Task<ApplicationUser?> FindLandlordAsync(Guid landlordId)
    {
        var landlord = await _userManager.FindByIdAsync(landlordId.ToString());

        if (landlord is not null &&
            !await _userManager.IsInRoleAsync(landlord, "Landlord"))
        {
            throw new InvalidOperationException(
                "The selected user is not a landlord.");
        }

        return landlord;
    }

    private static AdminLandlordResponse ToAdminLandlordResponse(
        ApplicationUser landlord)
    {
        return new AdminLandlordResponse
        {
            Id = landlord.Id,
            FullName = landlord.FullName,
            Email = landlord.Email ?? string.Empty,
            PhoneNumber = landlord.PhoneNumber ?? string.Empty,
            IsVerified = landlord.IsVerified,
            IsActive = landlord.LockoutEnd != DateTimeOffset.MaxValue
        };
    }

    public async Task VerifyLandlordAsync(
        Guid landlordId,
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
            throw new InvalidOperationException(
                "The selected user is not a landlord.");
        }

        landlord.IsVerified = true;

        var result =
            await _userManager.UpdateAsync(landlord);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(
                        x => x.Description)));
        }
    }

    public async Task RejectLandlordAsync(
        Guid landlordId,
        string reason,
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
            throw new InvalidOperationException(
                "The selected user is not a landlord.");
        }

        landlord.IsVerified = false;

        var result =
            await _userManager.UpdateAsync(landlord);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(
                        x => x.Description)));
        }

        // The documented API accepts a rejection reason.
        // We are not adding a new database field because
        // the supplied data model does not define one.
        _ = reason;
    }

    public async Task<List<PropertyListResponse>>
        GetPendingPropertiesAsync(
            CancellationToken cancellationToken)
    {
        var properties =
            await _propertyRepository.GetPendingAsync(
                cancellationToken);

        return properties
            .Select(x => new PropertyListResponse
            {
                Id = x.Id,
                Title = x.Title,
                PropertyType = x.PropertyType,
                Rent = x.Rent,
                Deposit = x.Deposit,
                Location = x.Location,
                Bedrooms = x.Bedrooms,
                Bathrooms = x.Bathrooms,
                Status = x.Status,
                IsVerified = x.IsVerified,
                ImageUrl = x.ImageUrl
            })
            .ToList();
    }

    public async Task<List<AdminPropertyResponse>> GetAllPropertiesAsync(
        CancellationToken cancellationToken)
    {
        var properties = await _propertyRepository.GetAllAsync(cancellationToken);
        if (properties.Count == 0)
        {
            return [];
        }

        var landlords = await _userManager.GetUsersInRoleAsync("Landlord");
        var landlordsById = landlords.ToDictionary(x => x.Id);

        return properties
            .Select(property =>
            {
                landlordsById.TryGetValue(property.LandlordId, out var landlord);

                return new AdminPropertyResponse
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
                    CreatedAt = property.CreatedAt,
                    ImageUrl = property.ImageUrl,
                    Landlord = new AdminPropertyLandlordResponse
                    {
                        Id = property.LandlordId,
                        FullName = landlord?.FullName ?? "Unknown landlord",
                        PhoneNumber = landlord?.PhoneNumber ?? string.Empty,
                        Email = landlord?.Email ?? string.Empty,
                        IsVerified = landlord?.IsVerified ?? false
                    }
                };
            })
            .ToList();
    }

    public async Task<VerificationResponse>
        ApprovePropertyAsync(
            Guid adminId,
            Guid propertyId,
            CancellationToken cancellationToken)
    {
        var property =
            await _propertyRepository.GetByIdAsync(
                propertyId,
                cancellationToken);

        if (property is null)
            throw new KeyNotFoundException(
                "Property was not found.");

        var landlord =
            await _userManager.FindByIdAsync(
                property.LandlordId.ToString());

        if (landlord is null)
            throw new InvalidOperationException(
                "Property landlord was not found.");

        if (!landlord.IsVerified)
            throw new InvalidOperationException(
                "Property cannot be approved because the landlord is not verified.");

        property.Approve(adminId);

        _propertyRepository.Update(property);

        await _propertyRepository.SaveChangesAsync(
            cancellationToken);

        return new VerificationResponse
        {
            Message = "Property approved successfully.",
            PropertyId = property.Id,
            Status = property.Status.ToString(),
            IsVerified = property.IsVerified
        };
    }

    public async Task<VerificationResponse>
        RejectPropertyAsync(
            Guid propertyId,
            CancellationToken cancellationToken)
    {
        var property =
            await _propertyRepository.GetByIdAsync(
                propertyId,
                cancellationToken);

        if (property is null)
            throw new KeyNotFoundException(
                "Property was not found.");

        property.Reject();

        _propertyRepository.Update(property);

        await _propertyRepository.SaveChangesAsync(
            cancellationToken);

        return new VerificationResponse
        {
            Message = "Property rejected.",
            PropertyId = property.Id,
            Status = property.Status.ToString(),
            IsVerified = property.IsVerified
        };
    }
}