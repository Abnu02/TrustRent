namespace TrustRent.Application.DTOs.Verification;

public sealed class RejectRequest
{
    public string Reason { get; init; } = string.Empty;
}