namespace TrustRent.Domain.Properties;

public sealed class PropertyListing
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public decimal Bathrooms { get; set; }
    public int SquareFeet { get; set; }
    public decimal MonthlyRent { get; set; }
    public string Description { get; set; } = string.Empty;
    public string DeedFileNumber { get; set; } = string.Empty;
    public string RecordedOwner { get; set; } = string.Empty;
    public string ParcelId { get; set; } = string.Empty;
    public string UtilityStatus { get; set; } = string.Empty;
    public string[] PhotoUrls { get; set; } = [];
    public PropertyReviewStatus ReviewStatus { get; set; } = PropertyReviewStatus.Pending;
    public string? ReviewNote { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}
