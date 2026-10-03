using TrustRent.Application.DTOs.Auth;

namespace TrustRent.Application.Abstractions.Authentication;

public interface IJwtTokenService
{
    Task<string> CreateTokenAsync(UserTokenData user);
}