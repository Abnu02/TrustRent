using TrustRent.Application.Properties;
using TrustRent.Domain.Users;

namespace TrustRent.Application.Interfaces;

public interface ILandlordReviewService
{
    Task<IReadOnlyList<LandlordReviewResponse>> GetPendingAsync(CancellationToken cancellationToken);
    Task<LandlordReviewResponse?> ReviewAsync(
        Guid landlordUserId,
        Guid reviewerUserId,
        LandlordVerificationStatus status,
        string? note,
        CancellationToken cancellationToken);
}
