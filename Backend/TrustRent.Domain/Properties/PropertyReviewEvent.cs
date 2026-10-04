namespace TrustRent.Domain.Properties;

public sealed class PropertyReviewEvent
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public Guid ActorUserId { get; set; }
    public PropertyReviewStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
