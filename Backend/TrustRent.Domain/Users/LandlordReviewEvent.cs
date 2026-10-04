namespace TrustRent.Domain.Users;

public sealed class LandlordReviewEvent
{
    public Guid Id { get; set; }
    public Guid LandlordUserId { get; set; }
    public Guid ActorUserId { get; set; }
    public LandlordVerificationStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
