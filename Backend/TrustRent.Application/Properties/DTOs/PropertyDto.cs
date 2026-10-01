using TrustRent.Domain.Enums;

namespace TrustRent.Application.Properties.DTOs;

public class PropertyDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Rent { get; set; }
    public string Location { get; set; } = string.Empty;
    public PropertyStatus Status { get; set; }
    public bool IsVerified { get; set; }
}