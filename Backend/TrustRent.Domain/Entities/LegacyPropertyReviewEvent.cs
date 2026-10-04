using TrustRent.Domain.Enums;

namespace TrustRent.Domain.Entities;

public sealed class LegacyPropertyReviewEvent
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public Guid ActorUserId { get; set; }
    public PropertyStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
