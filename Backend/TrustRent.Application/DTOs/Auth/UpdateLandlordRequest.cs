namespace TrustRent.Application.DTOs.Auth;

public sealed class UpdateLandlordRequest
{
    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;
}
