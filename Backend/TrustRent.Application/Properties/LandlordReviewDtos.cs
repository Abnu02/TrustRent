using System.ComponentModel.DataAnnotations;
using TrustRent.Domain.Users;

namespace TrustRent.Application.Properties;

public sealed record ReviewLandlordRequest
{
    [MaxLength(2000)]
    public string? Reason { get; init; }
}

public sealed record LandlordReviewResponse(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    LandlordVerificationStatus VerificationStatus,
    DateTime CreatedAt,
    DateTimeOffset? ReviewedAt,
    string? ReviewNote);
