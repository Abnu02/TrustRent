using TrustRent.Application.Auth.DTOs;

namespace TrustRent.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(UserDto user);
}