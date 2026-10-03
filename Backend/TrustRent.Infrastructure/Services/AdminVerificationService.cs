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
                Location = x.Location,
                Status = x.Status,
                IsVerified = x.IsVerified,
                ImageUrl = x.ImageUrl
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