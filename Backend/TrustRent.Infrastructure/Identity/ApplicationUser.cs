using Microsoft.AspNetCore.Identity;
using System;
using TrustRent.Domain.Users;

namespace TrustRent.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public LandlordVerificationStatus LandlordVerificationStatus { get; set; }
    public string? LandlordVerificationNote { get; set; }
    public DateTimeOffset? LandlordReviewedAt { get; set; }
    public Guid? LandlordReviewedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
