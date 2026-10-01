using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.DTOs;

public class PropertyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
    public decimal Rent { get; set; }
    public decimal Deposit { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public PropertyStatus Status { get; set; }
    public bool IsVerified { get; set; }
}