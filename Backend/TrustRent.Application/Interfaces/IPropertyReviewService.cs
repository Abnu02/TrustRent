using TrustRent.Application.Properties;
using TrustRent.Domain.Properties;

namespace TrustRent.Application.Interfaces;

public interface IPropertyReviewService
{
    Task<IReadOnlyList<PropertyReviewResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<PropertyReviewResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PropertyReviewResponse> CreateAsync(Guid ownerUserId, CreatePropertyRequest request, CancellationToken cancellationToken);
    Task<PropertyReviewResponse?> ReviewAsync(Guid id, Guid reviewerUserId, ReviewPropertyRequest request, CancellationToken cancellationToken);
    Task<PropertyReviewSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<PropertyReviewEventResponse>> GetAuditLogAsync(CancellationToken cancellationToken);
}
