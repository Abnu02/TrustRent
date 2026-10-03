namespace TrustRent.Application.DTOs.Verification;

public sealed class VerificationResponse
{
    public string Message { get; init; } = string.Empty;

    public Guid PropertyId { get; init; }

    public string Status { get; init; } = string.Empty;

    public bool IsVerified { get; init; }
}