using TrustRent.Domain.Enums;

namespace TrustRent.Application.DTOs.Properties;

public sealed class PropertyListResponse
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public PropertyType PropertyType { get; init; }

    public decimal Rent { get; init; }

    public string Location { get; init; } = string.Empty;

    public PropertyStatus Status { get; init; }

    public bool IsVerified { get; init; }

    public string? ImageUrl { get; init; }
}