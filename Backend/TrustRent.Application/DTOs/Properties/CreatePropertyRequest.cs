
using TrustRent.Domain.Enums;

namespace TrustRent.Application.DTOs.Properties;

public sealed class CreatePropertyRequest
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public PropertyType PropertyType { get; init; }

    public decimal Rent { get; init; }

    public decimal Deposit { get; init; }

    public string Location { get; init; } = string.Empty;

    public int Bedrooms { get; init; }

    public int Bathrooms { get; init; }
}