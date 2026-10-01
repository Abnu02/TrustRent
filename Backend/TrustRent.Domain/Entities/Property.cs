using TrustRent.Domain.Enums;

namespace TrustRent.Domain.Entities;

public class Property
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LandlordId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string PropertyType { get; set; } // "Apartment", "Condominium", "Villa"
    public decimal Rent { get; set; }
    public decimal Deposit { get; set; }
    public required string Location { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Pending;
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
