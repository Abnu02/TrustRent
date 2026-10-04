using TrustRent.Domain.Enums;

namespace TrustRent.Application.DTOs.Properties;

public sealed class PropertyListResponse
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public PropertyType PropertyType { get; init; }

    public decimal Rent { get; init; }

    public decimal Deposit { get; init; }

    public string Location { get; init; } = string.Empty;

    public int Bedrooms { get; init; }

    public int Bathrooms { get; init; }

    public PropertyStatus Status { get; init; }

    public bool IsVerified { get; init; }

    public string? ImageUrl { get; init; }
}