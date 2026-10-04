using TrustRent.Application.DTOs.Properties;
using TrustRent.Application.DTOs.Verification;
using TrustRent.Application.DTOs.Auth;

namespace TrustRent.Application.Services;

public interface IAdminVerificationService
{
    Task<List<AdminLandlordResponse>> GetLandlordsAsync(
        CancellationToken cancellationToken);

    Task<AdminLandlordResponse?> GetLandlordAsync(
        Guid landlordId,
        CancellationToken cancellationToken);

    Task<AdminLandlordResponse> CreateLandlordAsync(
        CreateLandlordRequest request,
        CancellationToken cancellationToken);

    Task<AdminLandlordResponse> UpdateLandlordAsync(
        Guid landlordId,
        UpdateLandlordRequest request,
        CancellationToken cancellationToken);

    Task SetLandlordActiveAsync(
        Guid landlordId,
        bool isActive,
        CancellationToken cancellationToken);

    Task<List<UserResponse>> GetPendingLandlordsAsync(
        CancellationToken cancellationToken);

    Task VerifyLandlordAsync(
        Guid landlordId,
        CancellationToken cancellationToken);

    Task RejectLandlordAsync(
        Guid landlordId,
        string reason,
        CancellationToken cancellationToken);

    Task<List<PropertyListResponse>> GetPendingPropertiesAsync(
        CancellationToken cancellationToken);

    Task<List<AdminPropertyResponse>> GetAllPropertiesAsync(
        CancellationToken cancellationToken);

    Task<VerificationResponse> ApprovePropertyAsync(
        Guid adminId,
        Guid propertyId,
        CancellationToken cancellationToken);

    Task<VerificationResponse> RejectPropertyAsync(
        Guid propertyId,
        CancellationToken cancellationToken);
}