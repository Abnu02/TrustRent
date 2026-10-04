using TrustRent.Domain.Enums;

namespace TrustRent.Domain.Entities;

public class Property
{
    public Guid Id { get; private set; }

    public Guid LandlordId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public PropertyType PropertyType { get; private set; }

    public decimal Rent { get; private set; }

    public decimal Deposit { get; private set; }

    public string Location { get; private set; } = string.Empty;

    public int Bedrooms { get; private set; }

    public int Bathrooms { get; private set; }

    public PropertyStatus Status { get; private set; }

    public bool IsVerified { get; private set; }

    public DateTimeOffset? VerifiedAt { get; private set; }

    public Guid? VerifiedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Requested TrustRent image extension
    public string? ImageUrl { get; private set; }

    private Property()
    {
    }

    public Property(
        Guid landlordId,
        string title,
        string description,
        PropertyType propertyType,
        decimal rent,
        decimal deposit,
        string location,
        int bedrooms,
        int bathrooms)
    {
        Id = Guid.NewGuid();

        LandlordId = landlordId;
        Title = title;
        Description = description;
        PropertyType = propertyType;
        Rent = rent;
        Deposit = deposit;
        Location = location;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;

        Status = PropertyStatus.Pending;
        IsVerified = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        string title,
        string description,
        PropertyType propertyType,
        decimal rent,
        decimal deposit,
        string location,
        int bedrooms,
        int bathrooms)
    {
        Title = title;
        Description = description;
        PropertyType = propertyType;
        Rent = rent;
        Deposit = deposit;
        Location = location;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;

        // An edited property needs admin review again.
        Status = PropertyStatus.Pending;
        IsVerified = false;
        VerifiedAt = null;
        VerifiedBy = null;
    }

    public void SetImage(string imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public void Approve(Guid adminId)
    {
        Status = PropertyStatus.Approved;
        IsVerified = true;
        VerifiedAt = DateTimeOffset.UtcNow;
        VerifiedBy = adminId;
    }

    public void Reject()
    {
        Status = PropertyStatus.Rejected;
        IsVerified = false;
        VerifiedAt = null;
        VerifiedBy = null;
    }

    public void Archive()
    {
        Status = PropertyStatus.Archived;
        IsVerified = false;
        VerifiedAt = null;
        VerifiedBy = null;
    }
}