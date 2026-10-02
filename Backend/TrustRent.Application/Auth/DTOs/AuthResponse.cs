namespace TrustRent.Application.Auth.DTOs;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}