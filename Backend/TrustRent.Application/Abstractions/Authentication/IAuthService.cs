using TrustRent.Application.DTOs.Auth;

namespace TrustRent.Application.Abstractions.Authentication;

public interface IAuthService
{
    Task<(bool Success, string? Error, UserResponse? User)>
        RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken);

    Task<(bool Success, string? Error, LoginResponse? Response)>
        LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken);

    Task LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken);
}