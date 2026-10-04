namespace TrustRent.Application.DTOs.Auth;

public sealed class AdminLandlordResponse
{
    public Guid Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public bool IsVerified { get; init; }

    public bool IsActive { get; init; }
}
