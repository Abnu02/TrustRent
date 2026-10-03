namespace TrustRent.Application.DTOs.Auth;

public sealed class UserTokenData
{
    public Guid Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public IList<string> Roles { get; init; } = new List<string>();
}